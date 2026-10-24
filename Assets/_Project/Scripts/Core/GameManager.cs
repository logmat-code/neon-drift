using System.Collections;
using UnityEngine;
using NeonDrift.Data;
using NeonDrift.Vehicle;
using NeonDrift.Gameplay;
using NeonDrift.World;
using NeonDrift.Presentation;
using NeonDrift.Progression;

namespace NeonDrift.Core
{
    public enum GameState
    {
        Home,
        Garage,
        Playing,
        Results
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        public GameState currentState = GameState.Home;

        [Header("Configurations")]
        [SerializeField] private CarConfig carConfig;
        [SerializeField] private FlowConfig flowConfig;
        [SerializeField] private UpgradeConfig upgradeConfig;

        [Header("Core Systems")]
        [SerializeField] private VehicleController playerVehicle;
        [SerializeField] private FlowManager flowManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private TrackManager trackManager;
        [SerializeField] private TrafficManager trafficManager;
        [SerializeField] private CameraController cameraController;
        [SerializeField] private GameFeelManager gameFeelManager;

        [Header("UI Views")]
        [SerializeField] private HomeView homeView;
        [SerializeField] private GarageView garageView;
        [SerializeField] private HUDView hudView;
        [SerializeField] private ResultsView resultsView;

        [Header("World Environments")]
        [SerializeField] private GameObject homeEnvironment;
        [SerializeField] private GameObject garageEnvironment;
        [SerializeField] private GameObject gameplayWorld;

        private Vector3 playerStartPos;
        private Quaternion playerStartRot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (playerVehicle != null)
            {
                playerStartPos = playerVehicle.transform.position;
                playerStartRot = playerVehicle.transform.rotation;
            }

            SetupUIListeners();
            SetupTrackListeners();

            EnterState(GameState.Home);
        }

        private void SetupUIListeners()
        {
            if (homeView != null)
            {
                homeView.OnDriveClicked += StartRun;
                homeView.OnGarageClicked += () => EnterState(GameState.Garage);
            }

            if (garageView != null)
            {
                garageView.OnBackClicked += () => EnterState(GameState.Home);
                garageView.OnDriveClicked += StartRun;
            }

            if (resultsView != null)
            {
                resultsView.OnRetryClicked += StartRun;
                resultsView.OnGarageClicked += () => EnterState(GameState.Garage);
                resultsView.OnHomeClicked += () => EnterState(GameState.Home);
            }
        }

        private void SetupTrackListeners()
        {
            if (trackManager != null)
            {
                trackManager.OnFinishCrossed += HandleFinishLine;
            }
        }

        public void EnterState(GameState newState)
        {
            currentState = newState;

            if (homeView != null) homeView.gameObject.SetActive(newState == GameState.Home);
            if (garageView != null) garageView.gameObject.SetActive(newState == GameState.Garage);
            if (hudView != null) hudView.gameObject.SetActive(newState == GameState.Playing);
            if (resultsView != null) resultsView.gameObject.SetActive(newState == GameState.Results);

            if (homeEnvironment != null) homeEnvironment.SetActive(newState == GameState.Home);
            if (garageEnvironment != null) garageEnvironment.SetActive(newState == GameState.Garage);
            if (gameplayWorld != null) gameplayWorld.SetActive(newState == GameState.Playing || newState == GameState.Results);

            switch (newState)
            {
                case GameState.Home:
                    homeView?.RefreshProfile();
                    AudioManager.Instance?.StopEngine();
                    AudioManager.Instance?.StartMusic();
                    // Snap camera and reset player position when returning home so scene view is pristine
                    if (playerVehicle != null)
                    {
                        playerVehicle.transform.position = new Vector3(0f, 6f, 0f);
                        playerVehicle.transform.rotation = Quaternion.identity;
                        if (playerVehicle.Rigidbody != null)
                        {
                            playerVehicle.Rigidbody.linearVelocity = Vector2.zero;
                            playerVehicle.Rigidbody.angularVelocity = 0f;
                            playerVehicle.Rigidbody.position = new Vector2(0f, 6f);
                            playerVehicle.Rigidbody.rotation = 0f;
                        }
                    }
                    cameraController?.SnapToTarget();
                    trafficManager?.ClearAllTraffic();
                    break;

                case GameState.Garage:
                    garageView?.RefreshUI();
                    AudioManager.Instance?.StopEngine();
                    AudioManager.Instance?.StartMusic();
                    break;

                case GameState.Playing:
                    AudioManager.Instance?.StartEngine();
                    AudioManager.Instance?.StartMusic();
                    break;

                case GameState.Results:
                    AudioManager.Instance?.StopEngine();
                    AudioManager.Instance?.PlayGameOver();
                    break;
            }
        }

        public void StartRun()
        {
            // Reset player strictly to cannonical start position (0, 6, 0)
            playerStartPos = new Vector3(0f, 6f, 0f);
            playerStartRot = Quaternion.identity;

            if (playerVehicle != null)
            {
                playerVehicle.transform.position = playerStartPos;
                playerVehicle.transform.rotation = playerStartRot;
                if (playerVehicle.Rigidbody != null)
                {
                    playerVehicle.Rigidbody.linearVelocity = Vector2.zero;
                    playerVehicle.Rigidbody.angularVelocity = 0f;
                    playerVehicle.Rigidbody.position = playerStartPos;
                    playerVehicle.Rigidbody.rotation = 0f;
                }
                playerVehicle.currentState = DriftState.Grip;
                playerVehicle.currentSpeed = 0f;
                playerVehicle.headingAngle = playerStartRot.eulerAngles.z;
                playerVehicle.trajectoryAngle = playerVehicle.headingAngle;
                playerVehicle.currentSlipAngle = 0f;
                playerVehicle.driftDirection = 0f;
                playerVehicle.ResetInputs();
                playerVehicle.boostEnergy = playerVehicle.Config != null ? playerVehicle.Config.maxBoostEnergy : 100f;
                playerVehicle.isBoosting = false;

                // Apply upgrade bonuses
                ProgressionManager.Instance?.ApplyUpgradesToVehicle(playerVehicle);
            }

            // Immediately snap camera to vehicle so recycling frustum calculations start clean
            cameraController?.SnapToTarget();

            // Reset HUD controls & buttons
            hudView?.ResetControls();

            // Clear & reset traffic
            trafficManager?.ClearAllTraffic();

            // Reset procedural endless track (will generate initial straight starting at (0,0) and horizon)
            trackManager?.ResetTrack(0);

            // Synchronize physics transforms immediately
            Physics2D.SyncTransforms();

            // Reset scores & flow
            scoreManager?.ResetSession();
            if (flowManager != null)
            {
                flowManager.currentFlow = 0f;
                flowManager.currentMultiplier = 1;
                flowManager.currentCombo = 0;
            }

            // Reset vehicle collision & durability
            var collisionHandler = playerVehicle != null ? playerVehicle.GetComponent<CollisionHandler>() : null;
            collisionHandler?.ResetDurability();

            EnterState(GameState.Playing);
        }

        public void EndRun(string reason = "RUN COMPLETE")
        {
            if (currentState != GameState.Playing) return;

            bool completed = true;
            int score = scoreManager != null ? scoreManager.currentScore : 0;
            float longestDrift = scoreManager != null ? scoreManager.longestDrift : 0f;
            int maxCombo = scoreManager != null ? scoreManager.maxCombo : 0;
            int nearMisses = scoreManager != null ? scoreManager.nearMissCount : 0;

            int credits = ProgressionManager.Instance != null 
                ? ProgressionManager.Instance.CalculateCreditsEarned(score, completed, maxCombo, nearMisses) 
                : 250;

            ProgressionManager.Instance?.RecordRunResults(score, longestDrift, maxCombo, nearMisses, credits);

            int best = ProgressionManager.Instance != null ? ProgressionManager.Instance.CurrentData.bestScore : score;

            resultsView?.DisplayResults(reason, score, best, longestDrift, maxCombo, nearMisses, credits);
            EnterState(GameState.Results);
        }

        private void HandleFinishLine()
        {
            EndRun("RUN COMPLETE");
        }
    }
}

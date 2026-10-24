using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NeonDrift.Vehicle;
using NeonDrift.Gameplay;

namespace NeonDrift.Presentation
{
    public class HUDView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private VehicleController vehicle;
        [SerializeField] private FlowManager flowManager;
        [SerializeField] private ScoreManager scoreManager;

        [Header("Score & Multiplier")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text multiplierText;
        [SerializeField] private Text actionPopupText;

        [Header("Gauges")]
        [SerializeField] private Image flowFillImage;
        [SerializeField] private Image boostFillImage;
        [SerializeField] private GameObject boostReadyGlow;

        [Header("Touch Buttons")]
        [SerializeField] private MobileTouchButton leftButton;
        [SerializeField] private MobileTouchButton rightButton;
        [SerializeField] private MobileTouchButton driftButton;
        [SerializeField] private MobileTouchButton boostButton;

        private Coroutine popupRoutine;
        private bool controlsInitialized = false;

        private void Start()
        {
            if (flowManager != null)
            {
                flowManager.OnFlowChanged += UpdateFlowUI;
                flowManager.OnMultiplierChanged += UpdateMultiplierUI;
            }

            if (scoreManager != null)
            {
                scoreManager.OnScoreAdded += HandleScoreAdded;
            }

            SetupTouchControls();

            if (actionPopupText != null)
                actionPopupText.gameObject.SetActive(false);
        }

        public void Bind(VehicleController vc, FlowManager fm, ScoreManager sm)
        {
            vehicle = vc;
            flowManager = fm;
            scoreManager = sm;

            if (flowManager != null)
            {
                flowManager.OnFlowChanged -= UpdateFlowUI;
                flowManager.OnMultiplierChanged -= UpdateMultiplierUI;
                flowManager.OnFlowChanged += UpdateFlowUI;
                flowManager.OnMultiplierChanged += UpdateMultiplierUI;
            }

            if (scoreManager != null)
            {
                scoreManager.OnScoreAdded -= HandleScoreAdded;
                scoreManager.OnScoreAdded += HandleScoreAdded;
            }

            SetupTouchControls();
        }

        private void SetupTouchControls()
        {
            if (controlsInitialized) return;
            controlsInitialized = true;

            if (leftButton != null)
            {
                leftButton.OnPressed += () => { if (vehicle != null) vehicle.SetSteer(-1f); };
                leftButton.OnReleased += () => { if (vehicle != null) vehicle.SetSteer(0f); };
            }
            if (rightButton != null)
            {
                rightButton.OnPressed += () => { if (vehicle != null) vehicle.SetSteer(1f); };
                rightButton.OnReleased += () => { if (vehicle != null) vehicle.SetSteer(0f); };
            }
            if (driftButton != null)
            {
                driftButton.OnPressed += () => { if (vehicle != null) vehicle.SetDrift(true); };
                driftButton.OnReleased += () => { if (vehicle != null) vehicle.SetDrift(false); };
            }
            if (boostButton != null)
            {
                boostButton.OnPressed += () => { if (vehicle != null) vehicle.RequestBoost(); };
            }
        }

        public void ResetControls()
        {
            leftButton?.Release();
            rightButton?.Release();
            driftButton?.Release();
            boostButton?.Release();
            if (vehicle != null)
            {
                vehicle.ResetInputs();
            }
        }

        private void OnDisable()
        {
            ResetControls();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Debug-only overlay: hidden by default, toggle with F3 (Editor / Development builds only).
        private bool showTelemetry = false;

        private void OnGUI()
        {
            if (!showTelemetry || vehicle == null) return;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.fontSize = 13;
            boxStyle.richText = true;
            boxStyle.normal.textColor = Color.white;

            GUILayout.BeginArea(new Rect(10, 80, 310, 165), boxStyle);
            GUILayout.Label("<b>--- HANDLING TELEMETRY ---</b>");
            GUILayout.Label($"DRIFT BUTTON HELD: {(vehicle.TelemetryDriftButtonHeld ? "<color=yellow>TRUE</color>" : "<color=#888888>FALSE</color>")}");
            GUILayout.Label($"DRIFT REQUESTED: {(vehicle.TelemetryDriftRequested ? "<color=cyan>TRUE</color>" : "<color=#888888>FALSE</color>")}");
            GUILayout.Label($"VEHICLE STATE: <b>{vehicle.currentState}</b>");
            GUILayout.Label($"STEERING INPUT: {vehicle.steerInput:F2}");
            GUILayout.Label($"SLIP ANGLE: {vehicle.currentSlipAngle:F1}°");
            GUILayout.Label($"DRIFT ASSIST ACTIVE: {(vehicle.TelemetryDriftAssistActive ? "<color=#00ff88>ACTIVE</color>" : "<color=#888888>OFF</color>")}");
            GUILayout.Label($"DYNAMIC FLICK ACTIVE: {(vehicle.TelemetryDynamicFlickActive ? "<color=#ff00ff>ACTIVE</color>" : "<color=#888888>OFF</color>")}");
            GUILayout.EndArea();
        }
#endif

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(KeyCode.F3))
            {
                showTelemetry = !showTelemetry;
            }
#endif
            if (vehicle != null)
            {
                // Update Boost Bar
                if (boostFillImage != null && vehicle.Config != null)
                {
                    float ratio = vehicle.boostEnergy / vehicle.Config.maxBoostEnergy;
                    boostFillImage.fillAmount = ratio;
                    if (boostReadyGlow != null)
                        boostReadyGlow.SetActive(vehicle.boostEnergy >= 30f);
                }
            }
        }

        private void UpdateFlowUI(float flow, int multiplier)
        {
            if (flowFillImage != null)
            {
                flowFillImage.fillAmount = flow / 100f;
            }
        }

        private void UpdateMultiplierUI(int multiplier)
        {
            if (multiplierText != null)
            {
                multiplierText.text = $"x{multiplier}";
                multiplierText.color = multiplier switch
                {
                    >= 8 => new Color(1f, 0f, 0.5f), // Magenta
                    >= 5 => new Color(0f, 0.9f, 1f), // Cyan
                    >= 3 => new Color(1f, 0.7f, 0f), // Amber
                    _ => Color.white
                };
            }
        }

        private void HandleScoreAdded(int currentScore, int addedPoints, string actionLabel)
        {
            if (scoreText != null)
            {
                scoreText.text = currentScore.ToString("N0");
            }

            if (actionPopupText != null && !string.IsNullOrEmpty(actionLabel))
            {
                if (popupRoutine != null) StopCoroutine(popupRoutine);
                popupRoutine = StartCoroutine(ShowPopup($"{actionLabel}\n+{addedPoints}"));
            }
        }

        private IEnumerator ShowPopup(string text)
        {
            actionPopupText.text = text;
            actionPopupText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.1f);
            actionPopupText.gameObject.SetActive(false);
            popupRoutine = null;
        }
    }
}

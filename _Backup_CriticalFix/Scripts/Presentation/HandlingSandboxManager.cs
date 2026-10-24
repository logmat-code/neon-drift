using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Vehicle;

namespace NeonDrift.Presentation
{
    /// <summary>
    /// Dedicated controller for the Handling Sandbox.
    /// Provides live diagnostics (speed, angle, state, steer input) and reset controls.
    /// Free of any Flow, Score, Traffic, or Obstacle interference.
    /// </summary>
    public class HandlingSandboxManager : MonoBehaviour
    {
        [Header("Vehicle Reference")]
        [SerializeField] private VehicleController vehicle;

        [Header("Diagnostics UI")]
        [SerializeField] private Text stateText;
        [SerializeField] private Text speedText;
        [SerializeField] private Text angleText;
        [SerializeField] private Text steerInputText;

        [Header("Touch Buttons")]
        [SerializeField] private MobileTouchButton leftButton;
        [SerializeField] private MobileTouchButton rightButton;
        [SerializeField] private MobileTouchButton driftButton;
        [SerializeField] private MobileTouchButton boostButton;
        [SerializeField] private Button resetButton;

        private Vector3 startPosition;
        private Quaternion startRotation;

        private void Start()
        {
            if (vehicle != null)
            {
                startPosition = vehicle.transform.position;
                startRotation = vehicle.transform.rotation;
                vehicle.boostEnergy = vehicle.Config != null ? vehicle.Config.maxBoostEnergy : 100f;
                SetupButtons();
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetVehicle);
            }
        }

        private void SetupButtons()
        {
            if (leftButton != null)
            {
                leftButton.OnPressed += () => vehicle.SetSteer(-1f);
                leftButton.OnReleased += () => vehicle.SetSteer(0f);
            }

            if (rightButton != null)
            {
                rightButton.OnPressed += () => vehicle.SetSteer(1f);
                rightButton.OnReleased += () => vehicle.SetSteer(0f);
            }

            if (driftButton != null)
            {
                driftButton.OnPressed += () => vehicle.SetDrift(true);
                driftButton.OnReleased += () => vehicle.SetDrift(false);
            }

            if (boostButton != null)
            {
                boostButton.OnPressed += () =>
                {
                    vehicle.AddBoostEnergy(100f);
                    vehicle.RequestBoost();
                };
            }
        }

        private void Update()
        {
            if (vehicle == null) return;

            if (stateText != null)
            {
                stateText.text = $"STATE: {vehicle.currentState}";
                stateText.color = vehicle.currentState switch
                {
                    DriftState.Grip => new Color(0.2f, 0.9f, 0.4f),
                    DriftState.Drifting => new Color(1.0f, 0.1f, 0.6f),
                    DriftState.DriftExit => new Color(0.2f, 0.8f, 1.0f),
                    _ => Color.white
                };
            }

            if (speedText != null)
            {
                float speedKmh = vehicle.currentSpeed * 3.6f;
                speedText.text = $"{speedKmh:F0} KM/H";
            }

            if (angleText != null)
            {
                angleText.text = $"DRIFT ANGLE: {vehicle.currentDriftAngle:F1}°";
            }

            if (steerInputText != null)
            {
                string steerStr = vehicle.steerInput switch
                {
                    < -0.1f => "◀ LEFT",
                    > 0.1f => "RIGHT ▶",
                    _ => "NEUTRAL"
                };
                string driftStr = vehicle.driftHeld ? "[DRIFT ON]" : "OFF";
                string boostStr = vehicle.isBoosting ? "[BOOSTING]" : (vehicle.boostEnergy >= 30f ? "READY" : $"{vehicle.boostEnergy:F0}%");
                steerInputText.text = $"STEER: {steerStr} | DRIFT: {driftStr} | BOOST: {boostStr}";
            }
        }

        public void ResetVehicle()
        {
            if (vehicle == null) return;

            vehicle.transform.position = startPosition;
            vehicle.transform.rotation = startRotation;
            vehicle.headingAngle = startRotation.eulerAngles.z;
            vehicle.trajectoryAngle = vehicle.headingAngle;
            vehicle.currentSpeed = 0f;
            vehicle.currentState = DriftState.Grip;
            vehicle.boostEnergy = vehicle.Config != null ? vehicle.Config.maxBoostEnergy : 100f;
            vehicle.isBoosting = false;
            vehicle.SetSteer(0f);
            vehicle.SetDrift(false);

            if (vehicle.Rigidbody != null)
            {
                vehicle.Rigidbody.linearVelocity = Vector2.zero;
                vehicle.Rigidbody.angularVelocity = 0f;
            }
        }
    }
}

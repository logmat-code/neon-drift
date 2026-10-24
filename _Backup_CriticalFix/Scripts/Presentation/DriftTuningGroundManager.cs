using System;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Vehicle;

namespace NeonDrift.Presentation
{
    /// <summary>
    /// Dedicated Diagnostics and Telemetry Controller for DriftTuningGround.
    /// Provides live measurement of vehicle drift metrics, 100% isolated zone switching,
    /// touch and keyboard input handling, and debug Gizmos.
    /// Guarantees that ONLY ONE test zone is active at any time.
    /// </summary>
    public class DriftTuningGroundManager : MonoBehaviour
    {
        [Header("Vehicle & Camera References")]
        [SerializeField] private VehicleController vehicle;
        [SerializeField] private CameraController cameraController;

        [Header("Telemetry UI Elements")]
        [SerializeField] private Text speedText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text slipAngleText;
        [SerializeField] private Text radiusText;
        [SerializeField] private Text yawRateText;
        [SerializeField] private Text steerInputText;
        [SerializeField] private Text anglesText;
        [SerializeField] private Text zoneNameText;

        [Header("Touch & Interaction Buttons")]
        [SerializeField] private MobileTouchButton leftButton;
        [SerializeField] private MobileTouchButton rightButton;
        [SerializeField] private MobileTouchButton driftButton;
        [SerializeField] private MobileTouchButton boostButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button nextZoneButton;
        [SerializeField] private Button prevZoneButton;

        [Header("Calibration Zones")]
        public ZoneDefinition[] zones;
        public int currentZoneIndex = 0;

        [System.Serializable]
        public struct ZoneDefinition
        {
            public string name;
            public GameObject zoneRoot;
            public Vector3 startPosition;
            public float startHeading;
            public float targetRadius;
            public string description;
        }

        private void Start()
        {
            if (vehicle == null)
            {
                vehicle = UnityEngine.Object.FindFirstObjectByType<VehicleController>();
            }

            if (cameraController == null)
            {
                cameraController = UnityEngine.Object.FindFirstObjectByType<CameraController>();
            }

            SetupButtons();

            if (zones != null && zones.Length > 0)
            {
                TeleportToZone(0);
            }
        }

        private void SetupButtons()
        {
            if (leftButton != null)
            {
                leftButton.OnPressed += () => vehicle?.SetSteer(-1f);
                leftButton.OnReleased += () => vehicle?.SetSteer(0f);
            }

            if (rightButton != null)
            {
                rightButton.OnPressed += () => vehicle?.SetSteer(1f);
                rightButton.OnReleased += () => vehicle?.SetSteer(0f);
            }

            if (driftButton != null)
            {
                driftButton.OnPressed += () => vehicle?.SetDrift(true);
                driftButton.OnReleased += () => vehicle?.SetDrift(false);
            }

            if (boostButton != null)
            {
                boostButton.OnPressed += () =>
                {
                    vehicle?.AddBoostEnergy(100f);
                    vehicle?.RequestBoost();
                };
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(() => TeleportToZone(currentZoneIndex));
            }

            if (nextZoneButton != null)
            {
                nextZoneButton.onClick.AddListener(NextZone);
            }

            if (prevZoneButton != null)
            {
                prevZoneButton.onClick.AddListener(PrevZone);
            }
        }

        private void Update()
        {
            // Keyboard hotkeys for testing
            if (Input.GetKeyDown(KeyCode.R)) TeleportToZone(currentZoneIndex);
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.N)) NextZone();
            if (Input.GetKeyDown(KeyCode.P)) PrevZone();

            // Number keys 1-9 & 0 for direct zone teleport
            for (int i = 0; i < 10; i++)
            {
                KeyCode key = (i == 9) ? KeyCode.Alpha0 : (KeyCode)((int)KeyCode.Alpha1 + i);
                if (Input.GetKeyDown(key))
                {
                    TeleportToZone(i);
                    break;
                }
            }

            UpdateTelemetryUI();
        }

        private void UpdateTelemetryUI()
        {
            if (vehicle == null) return;

            // Speed
            if (speedText != null)
            {
                float speedKmh = vehicle.currentSpeed * 3.6f;
                speedText.text = $"{speedKmh:F0} KM/H  ({vehicle.currentSpeed:F1} m/s)";
            }

            // State
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

            // Slip Angle
            if (slipAngleText != null)
            {
                string dirStr = vehicle.driftDirection switch
                {
                    < -0.1f => "LEFT",
                    > 0.1f => "RIGHT",
                    _ => "NONE"
                };
                slipAngleText.text = $"SLIP ANGLE: {vehicle.currentDriftAngle:F1}° ({dirStr})";
            }

            // Turn Radius
            if (radiusText != null)
            {
                if (vehicle.measuredTurnRadius < 150f)
                {
                    radiusText.text = $"TURN RADIUS: {vehicle.measuredTurnRadius:F1}m";
                    radiusText.color = new Color(1.0f, 0.85f, 0.2f);
                }
                else
                {
                    radiusText.text = "TURN RADIUS: STRAIGHT (∞)";
                    radiusText.color = Color.gray;
                }
            }

            // Yaw Rate & Trajectory Turn Rate
            if (yawRateText != null)
            {
                yawRateText.text = $"YAW RATE: {vehicle.yawRate:F1}°/s  | TRAJ RATE: {vehicle.currentTrajectoryTurnRate:F1}°/s";
            }

            // Steer & Drift Input
            if (steerInputText != null)
            {
                string steerStr = vehicle.steerInput switch
                {
                    < -0.1f => "◀ FULL LEFT",
                    > 0.1f => "FULL RIGHT ▶",
                    _ => "NEUTRAL"
                };
                string driftStr = vehicle.driftHeld ? "[DRIFT ACTIVE]" : "OFF";
                steerInputText.text = $"STEER: {steerStr} | DRIFT: {driftStr}";
            }

            // Heading & Trajectory
            if (anglesText != null)
            {
                anglesText.text = $"BODY: {vehicle.headingAngle:F1}°  | TRAJECTORY: {vehicle.trajectoryAngle:F1}°";
            }

            // Zone Name
            if (zoneNameText != null && zones != null && currentZoneIndex < zones.Length)
            {
                zoneNameText.text = $"{zones[currentZoneIndex].name}\n<size=22>{zones[currentZoneIndex].description}</size>";
            }
        }

        public void TeleportToZone(int index)
        {
            if (zones == null || zones.Length == 0) return;

            currentZoneIndex = Mathf.Clamp(index, 0, zones.Length - 1);
            var z = zones[currentZoneIndex];

            // 1. STRICT ISOLATION: Activate ONLY the target zone root, deactivate all others
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i].zoneRoot != null)
                {
                    zones[i].zoneRoot.SetActive(i == currentZoneIndex);
                }
            }

            // 2. COMPLETE VEHICLE RESET TO SPAWN POINT
            if (vehicle != null)
            {
                vehicle.transform.position = z.startPosition;
                vehicle.transform.rotation = Quaternion.Euler(0f, 0f, -z.startHeading);
                vehicle.headingAngle = z.startHeading;
                vehicle.trajectoryAngle = z.startHeading;
                vehicle.currentSpeed = 0f;
                vehicle.currentState = DriftState.Grip;
                vehicle.currentDriftAngle = 0f;
                vehicle.currentSlipAngle = 0f;
                vehicle.driftDirection = 0f;
                vehicle.boostEnergy = vehicle.Config != null ? vehicle.Config.maxBoostEnergy : 100f;
                vehicle.isBoosting = false;
                vehicle.SetSteer(0f);
                vehicle.SetDrift(false);

                if (vehicle.Rigidbody != null)
                {
                    vehicle.Rigidbody.position = z.startPosition;
                    vehicle.Rigidbody.linearVelocity = Vector2.zero;
                    vehicle.Rigidbody.angularVelocity = 0f;
                }
            }

            // 3. SNAP CAMERA WITH ZERO LERP LAG
            if (cameraController != null && vehicle != null)
            {
                cameraController.SnapToTarget();
            }

            UpdateTelemetryUI();
            Debug.Log($"[DriftTuningGround] Activated Zone {currentZoneIndex + 1}/10: {z.name}");
        }

        public void NextZone()
        {
            if (zones == null || zones.Length == 0) return;
            int next = (currentZoneIndex + 1) % zones.Length;
            TeleportToZone(next);
        }

        public void PrevZone()
        {
            if (zones == null || zones.Length == 0) return;
            int prev = (currentZoneIndex - 1 + zones.Length) % zones.Length;
            TeleportToZone(prev);
        }

        private void OnDrawGizmos()
        {
            if (vehicle == null) return;

            Vector3 pos = vehicle.transform.position;

            // 1. Velocity Vector (Green)
            if (vehicle.Rigidbody != null && vehicle.Rigidbody.linearVelocity.sqrMagnitude > 0.1f)
            {
                Gizmos.color = Color.green;
                Vector3 velDir = (Vector3)vehicle.Rigidbody.linearVelocity;
                Gizmos.DrawRay(pos, velDir * 0.5f);
            }

            // 2. Body Forward Vector (Cyan)
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(pos, vehicle.transform.up * 4.0f);

            // 3. Predicted Turning Circle (Yellow wire circle)
            if (vehicle.measuredTurnRadius < 80f && vehicle.currentState == DriftState.Drifting)
            {
                Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.6f);
                float r = vehicle.measuredTurnRadius;
                Vector3 centerOffset = vehicle.transform.right * (vehicle.driftDirection * r);
                Gizmos.DrawWireSphere(pos + centerOffset, r);
            }
        }
    }
}

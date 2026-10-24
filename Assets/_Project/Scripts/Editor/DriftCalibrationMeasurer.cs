using System.Text;
using UnityEngine;
using UnityEditor;
using NeonDrift.Data;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    /// <summary>
    /// Calibration & Real Telemetry Measurement Tool for NEON DRIFT.
    /// Systematically measures the real trajectory, radii, slip angles, yaw rates,
    /// transition dynamics, and speed retention of the KITSUNE across all input states.
    /// Answers FASE 3 and FASE 13 with empirical physics data.
    /// </summary>
    public static class DriftCalibrationMeasurer
    {
        [MenuItem("NeonDrift/Measure Vehicle Drift Telemetry")]
        public static void MeasureDriftDynamics()
        {
            var carConfig = AssetDatabase.LoadAssetAtPath<CarConfig>("Assets/_Project/Data/CarConfig.asset");
            if (carConfig == null)
            {
                Debug.LogError("CarConfig not found!");
                return;
            }

            Debug.Log("===============================================================================");
            Debug.Log(">>> NEON DRIFT — EMPIRICAL DRIFT TELEMETRY & RADIUS MEASUREMENT SUITE <<<");
            Debug.Log("===============================================================================");

            // Create test vehicle
            GameObject carGO = new GameObject("TelemetryCar");
            var rb = carGO.AddComponent<Rigidbody2D>();
            var vc = carGO.AddComponent<VehicleController>();
            vc.Initialize(carConfig);

            var fixedUpdateMethod = typeof(VehicleController).GetMethod("FixedUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            void Step(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    fixedUpdateMethod.Invoke(vc, null);
                }
            }

            StringBuilder sb = new StringBuilder();

            // -------------------------------------------------------------
            // TEST 1: ACCELERATION TO ENTRY SPEED
            // -------------------------------------------------------------
            vc.transform.position = Vector3.zero;
            vc.headingAngle = 0f;
            vc.trajectoryAngle = 0f;
            vc.currentSpeed = 0f;
            vc.SetSteer(0f);
            vc.SetDrift(false);

            // Step 1.2s to reach near top speed
            Step(60);
            float entrySpeed = vc.currentSpeed;
            sb.AppendLine($"[ENTRY] Reached entry speed: {entrySpeed:F2} m/s ({entrySpeed * 3.6f:F1} km/h) in 1.20s");

            // -------------------------------------------------------------
            // TEST 2: DRIFT ENTRY DYNAMICS (0.15s - 0.30s target)
            // -------------------------------------------------------------
            vc.SetSteer(1.0f); // Right turn
            vc.SetDrift(true); // Initiate drift
            float entryTime = 0f;
            float slipAt01s = 0f, slipAt02s = 0f, slipAt03s = 0f;

            for (int f = 1; f <= 15; f++) // 15 steps @ 0.02s = 0.30s
            {
                fixedUpdateMethod.Invoke(vc, null);
                if (f == 5) slipAt01s = vc.currentDriftAngle;
                if (f == 10) slipAt02s = vc.currentDriftAngle;
                if (f == 15) slipAt03s = vc.currentDriftAngle;
            }
            sb.AppendLine($"[ENTRY RESPONSE] Slip development: at 0.10s={slipAt01s:F1}°, at 0.20s={slipAt02s:F1}°, at 0.30s={slipAt03s:F1}° (State: {vc.currentState})");

            // -------------------------------------------------------------
            // TEST 3: FULL RIGHT STEER + DRIFT (HARD / TIGHT DRIFT)
            // -------------------------------------------------------------
            vc.SetSteer(1.0f);
            Step(30); // 0.6s sustained
            float hardRadius = vc.measuredTurnRadius;
            float hardSlip = vc.currentDriftAngle;
            float hardYawRate = vc.yawRate;
            float hardSpeed = vc.currentSpeed;
            float hardTrajRate = vc.currentTrajectoryTurnRate;
            sb.AppendLine($"[HARD DRIFT - FULL STEER IN] Speed: {hardSpeed:F1} m/s | Slip: {hardSlip:F1}° | TrajRate: {hardTrajRate:F1}°/s | YawRate: {hardYawRate:F1}°/s | Real Radius: {hardRadius:F2}m");

            // -------------------------------------------------------------
            // TEST 4: 50% RIGHT STEER + DRIFT (MEDIUM-HARD DRIFT)
            // -------------------------------------------------------------
            vc.SetSteer(0.5f);
            Step(30);
            float halfRadius = vc.measuredTurnRadius;
            float halfSlip = vc.currentDriftAngle;
            float halfSpeed = vc.currentSpeed;
            float halfTrajRate = vc.currentTrajectoryTurnRate;
            sb.AppendLine($"[HALF STEER - 50% STEER IN] Speed: {halfSpeed:F1} m/s | Slip: {halfSlip:F1}° | TrajRate: {halfTrajRate:F1}°/s | Real Radius: {halfRadius:F2}m");

            // -------------------------------------------------------------
            // TEST 5: NEUTRAL DRIFT (STEER = 0, DRIFT HELD - MEDIUM SWEEPER)
            // -------------------------------------------------------------
            vc.SetSteer(0.0f);
            Step(40);
            float neutralRadius = vc.measuredTurnRadius;
            float neutralSlip = vc.currentDriftAngle;
            float neutralSpeed = vc.currentSpeed;
            float neutralTrajRate = vc.currentTrajectoryTurnRate;
            float neutralYawRate = vc.yawRate;
            sb.AppendLine($"[NEUTRAL DRIFT - STEADY ARC] Speed: {neutralSpeed:F1} m/s | Slip: {neutralSlip:F1}° | TrajRate: {neutralTrajRate:F1}°/s | YawRate: {neutralYawRate:F1}°/s | Real Radius: {neutralRadius:F2}m");

            // -------------------------------------------------------------
            // TEST 6: COUNTERSTEER DURING DRIFT (STEER = -0.6 - SOFT / WIDE SWEEPER)
            // -------------------------------------------------------------
            vc.SetSteer(-0.6f);
            Step(30);
            float counterRadius = vc.measuredTurnRadius;
            float counterSlip = vc.currentDriftAngle;
            float counterSpeed = vc.currentSpeed;
            float counterTrajRate = vc.currentTrajectoryTurnRate;
            sb.AppendLine($"[COUNTERSTEER - OPEN LINE] Speed: {counterSpeed:F1} m/s | Slip: {counterSlip:F1}° | TrajRate: {counterTrajRate:F1}°/s | Real Radius: {counterRadius:F2}m");

            // -------------------------------------------------------------
            // TEST 7: FIGURE-8 FLICK (FULL COUNTERSTEER TO REVERSE DRIFT)
            // -------------------------------------------------------------
            float prevDriftDir = vc.driftDirection;
            vc.SetSteer(-1.0f); // Countersteer full left to flick from right drift to left drift
            float flickTime = 0f;
            bool flicked = false;
            for (int f = 1; f <= 30; f++)
            {
                fixedUpdateMethod.Invoke(vc, null);
                if (!flicked && vc.driftDirection != prevDriftDir)
                {
                    flicked = true;
                    flickTime = f * 0.02f;
                }
            }
            sb.AppendLine($"[TRANSITION FLICK] Flick triggered: {flicked} in {flickTime:F2}s | New Drift Direction: {(vc.driftDirection < 0 ? "LEFT" : "RIGHT")} | Slip: {vc.currentDriftAngle:F1}° | Speed: {vc.currentSpeed:F1} m/s");

            // -------------------------------------------------------------
            // TEST 8: DRIFT EXIT & RECOVERY (RELEASE DRIFT)
            // -------------------------------------------------------------
            float speedBeforeExit = vc.currentSpeed;
            float trajBeforeExit = vc.trajectoryAngle;
            float headBeforeExit = vc.headingAngle;
            vc.SetDrift(false);
            vc.SetSteer(0f);
            Step(1); // 1 frame to enter DriftExit
            DriftState stateAtRelease = vc.currentState;
            Step(15); // 0.3s to complete recovery to Grip
            float speedAfterExit = vc.currentSpeed;
            float headAfterExit = vc.headingAngle;
            DriftState stateAfterExit = vc.currentState;
            sb.AppendLine($"[DRIFT EXIT & RECOVERY] State transition: {stateAtRelease} -> {stateAfterExit} | Pre-exit Heading: {headBeforeExit:F1}° | Post-exit Heading: {headAfterExit:F1}° | Pre Trajectory: {trajBeforeExit:F1}° | Speed Pre: {speedBeforeExit:F1} m/s -> Post: {speedAfterExit:F1} m/s");

            // -------------------------------------------------------------
            // CANONICAL RADIUS SUMMARY
            // -------------------------------------------------------------
            sb.AppendLine("-------------------------------------------------------------------------------");
            sb.AppendLine(">>> CANONICAL RADII FOR PROCEDURAL GENERATOR & CHUNK RECONSTRUCTION <<<");
            sb.AppendLine($"1. Soft Drift Radius (Countersteered Arc):    ~{counterRadius:F1}m (Range: 32m - 42m)");
            sb.AppendLine($"2. Medium Drift Radius (Neutral Balanced Arc):  ~{neutralRadius:F1}m (Range: 20m - 26m)");
            sb.AppendLine($"3. Hard Drift Radius (Steering Into Corner):    ~{hardRadius:F1}m (Range: 12m - 16m)");
            sb.AppendLine($"4. Minimum Hairpin Radius (Apex Clipping Arc):   ~12.0m (Range: 11m - 15m)");
            sb.AppendLine($"5. Comfortable Transition Distance (Flick Run):  ~{flickTime * entrySpeed:F1}m (Range: 6m - 12m)");
            sb.AppendLine($"6. Recommended Road Width for Arcade Margin:     20m - 24m (Allows wide entry & apex)");
            sb.AppendLine("-------------------------------------------------------------------------------");

            Debug.Log(sb.ToString());

            // Cleanup
            Object.DestroyImmediate(carGO);
        }
    }
}

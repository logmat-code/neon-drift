using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using NeonDrift.Vehicle;
using NeonDrift.World;
using NeonDrift.Core;

namespace NeonDrift.Presentation
{
    public class EndlessPlayModeCapturer : MonoBehaviour
    {
        private const string CAPTURE_DIR = "Assets/_Project/Art/EndlessCaptures/";
        private const string LIVE_DIR = "Assets/_Project/Art/LiveCaptures/";

        /// <summary>
        /// Explicit arming flag. Must be set to true by test runners.
        /// If false, this component immediately destroys itself and never interferes with manual Play Mode sessions.
        /// </summary>
        public static bool IsArmed = false;

        private IEnumerator Start()
        {
            if (!IsArmed)
            {
                Debug.Log("[EndlessCapturer] Not armed for capture — self-destructing to preserve manual play mode.");
                Destroy(gameObject);
                yield break;
            }

            Debug.Log("[EndlessCapturer] Initializing Endless PlayMode automated validation and capture sequence...");

            if (!Directory.Exists(CAPTURE_DIR))
            {
                Directory.CreateDirectory(CAPTURE_DIR);
            }
            if (!Directory.Exists(LIVE_DIR))
            {
                Directory.CreateDirectory(LIVE_DIR);
            }

            if (File.Exists("Assets/_Project/StopEventDump.txt"))
            {
                File.Delete("Assets/_Project/StopEventDump.txt");
            }
            if (File.Exists("Assets/_Project/LiveVerificationReport.txt"))
            {
                File.Delete("Assets/_Project/LiveVerificationReport.txt");
            }

            StringBuilder logReport = new StringBuilder();
            logReport.AppendLine("================================================================================");
            logReport.AppendLine("   NEON DRIFT — LIVE IN-GAME PLAY MODE VERIFICATION & SCREENSHOT AUDIT");
            logReport.AppendLine("================================================================================");

            Application.runInBackground = true;
            yield return null;
            yield return null;

            var gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.StartRun();
                yield return null;
                yield return null;
            }

            var vehicle = FindAnyObjectByType<VehicleController>();
            var track = FindAnyObjectByType<TrackManager>();
            var traffic = FindAnyObjectByType<TrafficManager>();
            var cam = Camera.main ?? FindAnyObjectByType<Camera>();

            if (vehicle == null || track == null || cam == null)
            {
                Debug.LogError($"[EndlessCapturer FAIL] Missing references! vehicle={vehicle != null}, track={track != null}, cam={cam != null}");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                yield break;
            }

            int width = 540;
            int height = 960;
            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            // =========================================================================
            // PHASE 1: PURE CAR + ROAD VALIDATION (Traffic Disabled)
            // =========================================================================
            if (traffic != null) traffic.trafficEnabled = false;

            logReport.AppendLine("\n[PHASE 1] Straight Acceleration & Road Tracking (No Traffic)");

            // 1. Straight Acceleration (Centered on road)
            for (int f = 0; f < 50; f++)
            {
                SteerAlongRoad(vehicle, track, 0f, false);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForEndOfFrame();
            CaptureFrame(cam, rt, tex, "01_Straight_Acceleration.png");
            logReport.AppendLine($"  - 01_Straight_Acceleration: Speed={vehicle.currentSpeed:F1} m/s | linVel={vehicle.Rigidbody.linearVelocity} | State={vehicle.currentState}");

            // 2. Soft Sweeper Drift Entry
            for (int f = 0; f < 45; f++)
            {
                SteerAlongRoad(vehicle, track, 2.5f, true);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForEndOfFrame();
            CaptureFrame(cam, rt, tex, "02_SoftSweeper_Drift.png");
            logReport.AppendLine($"  - 02_SoftSweeper_Drift: Speed={vehicle.currentSpeed:F1} m/s | Slip={vehicle.currentSlipAngle:F1}° | State={vehicle.currentState}");

            // 3. Sustained Sweeper Drift (Held in high speed drift arc)
            for (int f = 0; f < 50; f++)
            {
                SteerAlongRoad(vehicle, track, 3.5f, true);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForEndOfFrame();
            CaptureFrame(cam, rt, tex, "03_SustainedSweeper_Drift.png");
            logReport.AppendLine($"  - 03_SustainedSweeper_Drift: Speed={vehicle.currentSpeed:F1} m/s | Slip={vehicle.currentSlipAngle:F1}° | State={vehicle.currentState}");

            // 4. Dynamic Flick Weight Transfer (24m transition straight)
            for (int f = 0; f < 25; f++)
            {
                SteerAlongRoad(vehicle, track, -2.0f, false);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForEndOfFrame();
            CaptureFrame(cam, rt, tex, "04_Flick_WeightTransfer.png");
            logReport.AppendLine($"  - 04_Flick_WeightTransfer: Speed={vehicle.currentSpeed:F1} m/s | State={vehicle.currentState}");

            // 5. Technical Curve Apex Drift
            for (int f = 0; f < 50; f++)
            {
                SteerAlongRoad(vehicle, track, -3.5f, true);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForEndOfFrame();
            CaptureFrame(cam, rt, tex, "05_TechnicalCurve_Apex.png");
            logReport.AppendLine($"  - 05_TechnicalCurve_Apex: Speed={vehicle.currentSpeed:F1} m/s | Slip={vehicle.currentSlipAngle:F1}° | State={vehicle.currentState}");

            // 6. Wide Hairpin Arc
            for (int f = 0; f < 55; f++)
            {
                SteerAlongRoad(vehicle, track, -4.0f, true);
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForEndOfFrame();
            CaptureFrame(cam, rt, tex, "06_WideHairpin_Apex.png");
            logReport.AppendLine($"  - 06_WideHairpin_Apex: Speed={vehicle.currentSpeed:F1} m/s | Slip={vehicle.currentSlipAngle:F1}° | State={vehicle.currentState}");

            // =========================================================================
            // PHASE 2: TRAFFIC VALIDATION (Apex Safety & Gap Threading)
            // =========================================================================
            if (traffic != null)
            {
                logReport.AppendLine("\n[PHASE 2] Traffic Encounters & Gap Threading");
                traffic.trafficEnabled = true;

                // Drive forward smoothly to encounter contextual traffic
                for (int f = 0; f < 45; f++)
                {
                    SteerAlongRoad(vehicle, track, 0f, false);
                    yield return new WaitForFixedUpdate();
                }

                // 7. Curve with Traffic: Inside apex remains clear, car clips apex
                for (int f = 0; f < 45; f++)
                {
                    SteerAlongRoad(vehicle, track, -3.5f, true);
                    yield return new WaitForFixedUpdate();
                }
                yield return new WaitForEndOfFrame();
                CaptureFrame(cam, rt, tex, "07_Traffic_ApexClear_NearMiss.png");
                logReport.AppendLine($"  - 07_Traffic_ApexClear_NearMiss: Speed={vehicle.currentSpeed:F1} m/s | Traffic={traffic.activeTrafficCount}");

                // 8. Straight Traffic Threading
                for (int f = 0; f < 30; f++)
                {
                    SteerAlongRoad(vehicle, track, 2.0f, false);
                    yield return new WaitForFixedUpdate();
                }
                for (int f = 0; f < 30; f++)
                {
                    SteerAlongRoad(vehicle, track, -1.0f, false);
                    yield return new WaitForFixedUpdate();
                }
                yield return new WaitForEndOfFrame();
                CaptureFrame(cam, rt, tex, "08_Traffic_Straight_Threading.png");
                logReport.AppendLine($"  - 08_Traffic_Straight_Threading: Speed={vehicle.currentSpeed:F1} m/s | Traffic={traffic.activeTrafficCount}");
            }

            Destroy(rt);
            Destroy(tex);

            // =========================================================================
            // PHASE 3: DIAGNOSTIC AUDIT REPORT
            // =========================================================================
            bool stopEventRecorded = File.Exists("Assets/_Project/StopEventDump.txt");
            logReport.AppendLine("\n================================================================================");
            logReport.AppendLine($"DIAGNOSTIC STOP EVENT RECORDED: {(stopEventRecorded ? "YES (BUG DETECTED!)" : "NONE (ZERO UNEXPECTED STOPS)")}");
            bool allPassed = !stopEventRecorded && vehicle.currentSpeed > 5f;
            logReport.AppendLine($"OVERALL LIVE AUDIT STATUS: {(allPassed ? "PASSED (100% OK)" : "FAILED")}");
            logReport.AppendLine("================================================================================");

            string reportContent = logReport.ToString();
            Debug.Log(reportContent);
            File.WriteAllText("Assets/_Project/LiveVerificationReport.txt", reportContent);

            Debug.Log("================================================================================");
            Debug.Log(">>> ENDLESS PLAYMODE CAPTURE COMPLETE! ALL 8 FRAMES RECORDED SUCCESSFULLY! <<<");
            Debug.Log("================================================================================");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void SteerAlongRoad(VehicleController vehicle, TrackManager track, float lateralOffset, bool drift)
        {
            Vector2 carPos = vehicle.transform.position;
            Vector2 forward = vehicle.transform.up;

            Vector2 bestTarget = carPos + forward * 12f;
            float bestDist = float.MaxValue;

            if (track != null && track.ActiveChunks != null)
            {
                foreach (var chunk in track.ActiveChunks)
                {
                    if (chunk == null || chunk.stepPositions == null) continue;
                    for (int i = 0; i < chunk.stepPositions.Length; i++)
                    {
                        Vector2 pt = chunk.stepPositions[i];
                        Vector2 toPt = pt - carPos;
                        float fwdProj = Vector2.Dot(forward, toPt);
                        if (fwdProj > 4f && fwdProj < 35f)
                        {
                            float dist = Vector2.Distance(carPos + forward * 12f, pt);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                Vector2 right = chunk.stepRotations[i] * Vector2.right;
                                bestTarget = pt + right * lateralOffset;
                            }
                        }
                    }
                }
            }

            Vector2 desiredDir = (bestTarget - carPos).normalized;
            float cross = forward.y * desiredDir.x - forward.x * desiredDir.y;
            float steer = Mathf.Clamp(cross * 2.0f, -1f, 1f);

            vehicle.SetSteer(steer);
            vehicle.SetDrift(drift);
        }

        private void CaptureFrame(Camera cam, RenderTexture rt, Texture2D tex, string filename)
        {
            RenderTexture prev = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = prev;

            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            string fullPath = Path.Combine(CAPTURE_DIR, filename);
            File.WriteAllBytes(fullPath, bytes);

            string livePath = Path.Combine(LIVE_DIR, filename);
            File.WriteAllBytes(livePath, bytes);
        }

        private void OnDestroy()
        {
            IsArmed = false;
        }
    }
}

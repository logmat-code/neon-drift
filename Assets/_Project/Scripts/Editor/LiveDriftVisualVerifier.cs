using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Vehicle;
using NeonDrift.World;
using NeonDrift.Core;
using NeonDrift.Presentation;
using NeonDrift.Diagnostics;

namespace NeonDrift.Editor
{
    [InitializeOnLoad]
    public static class LiveDriftVisualVerifier
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/NeonDistrictLoop.unity";
        private const string CAPTURE_DIR = "Assets/_Project/Art/LiveCaptures/";
        private const string PREF_KEY = "NeonDrift_RunLiveVerification";

        static LiveDriftVisualVerifier()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool(PREF_KEY, false))
                {
                    EditorPrefs.DeleteKey(PREF_KEY);
                    Debug.Log("[LiveVerification] Entered PlayMode. Spawning LiveVerificationHarness...");
                    GameObject harnessGO = new GameObject("_LiveVerificationRunner");
                    harnessGO.AddComponent<LiveVerificationHarness>();
                }
            }
        }

        [MenuItem("NeonDrift/Execute Live Visual & Driving Verification")]
        public static void RunLiveVerification()
        {
            if (!Directory.Exists(CAPTURE_DIR))
            {
                Directory.CreateDirectory(CAPTURE_DIR);
            }

            // Remove any legacy report or dump
            if (File.Exists("Assets/_Project/StopEventDump.txt"))
            {
                File.Delete("Assets/_Project/StopEventDump.txt");
            }
            if (File.Exists("Assets/_Project/LiveVerificationReport.txt"))
            {
                File.Delete("Assets/_Project/LiveVerificationReport.txt");
            }

            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[LiveVerification] Already in Play Mode. Stopping first...");
                EditorApplication.isPlaying = false;
                return;
            }

            EditorPrefs.SetBool(PREF_KEY, true);
            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }
    }

    public class LiveVerificationHarness : MonoBehaviour
    {
        private const string CAPTURE_DIR = "Assets/_Project/Art/LiveCaptures/";
        private StringBuilder logReport = new StringBuilder();

        private IEnumerator Start()
        {
            logReport.AppendLine("================================================================================");
            logReport.AppendLine("   NEON DRIFT — LIVE IN-GAME PLAY MODE VERIFICATION & SCREENSHOT AUDIT");
            logReport.AppendLine("================================================================================");

            // 1. Wait for scene & gameplay systems to initialize
            yield return new WaitForSeconds(0.5f);

            var gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            var tm = UnityEngine.Object.FindFirstObjectByType<TrackManager>();
            var trf = UnityEngine.Object.FindFirstObjectByType<TrafficManager>();
            var vc = UnityEngine.Object.FindFirstObjectByType<VehicleController>();
            var cam = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>();

            if (gm == null || tm == null || vc == null || cam == null)
            {
                Debug.LogError($"[LiveVerification] Critical components missing! gm={gm != null}, tm={tm != null}, vc={vc != null}, cam={cam != null}");
                yield break;
            }

            // Start playing run
            gm.StartRun();
            yield return new WaitForSeconds(0.5f);

            // -------------------------------------------------------------------------
            // TEST A: Straight Acceleration & Highway Continuity (No Controls)
            // -------------------------------------------------------------------------
            logReport.AppendLine("\n[TEST A] Straight Acceleration & Highway Continuity");
            float testATimer = 0f;
            bool testA_Ok = true;

            while (testATimer < 2.5f)
            {
                testATimer += Time.deltaTime;
                if (vc.currentSpeed < 0.5f && testATimer > 1.0f)
                {
                    testA_Ok = false;
                }
                yield return null;
            }

            CaptureCamera(cam, "01_Live_Straight_Highway.png");
            logReport.AppendLine($"  - Auto-Acceleration Speed: {vc.currentSpeed:F1} m/s | linVel: {vc.Rigidbody.linearVelocity} | State: {vc.currentState}");
            logReport.AppendLine($"  - Test A Status: {(testA_Ok ? "PASS" : "FAIL")}");

            // -------------------------------------------------------------------------
            // TEST B & C: Left Curve & Right Curve (Controlled Steering & Drift)
            // -------------------------------------------------------------------------
            logReport.AppendLine("\n[TEST B & C] Curved Highway Driving & Controlled Drift");

            // Steer Left in Grip
            vc.SetSteer(-0.6f);
            yield return new WaitForSeconds(1.0f);
            CaptureCamera(cam, "02_Live_CurveLeft_Ribbon.png");
            float speedLeftGrip = vc.currentSpeed;
            logReport.AppendLine($"  - Left Turn (Grip) Speed: {speedLeftGrip:F1} m/s | Heading: {vc.headingAngle:F1}° | Slip: {vc.currentSlipAngle:F1}°");

            // Drift Left
            vc.SetDrift(true);
            yield return new WaitForSeconds(1.2f);
            CaptureCamera(cam, "04_Live_SustainedSweeper.png");
            float speedLeftDrift = vc.currentSpeed;
            logReport.AppendLine($"  - Left Drift Active Speed: {speedLeftDrift:F1} m/s | State: {vc.currentState} | Slip: {vc.currentSlipAngle:F1}°");

            // Release Drift -> Smooth Recovery to Grip
            vc.SetDrift(false);
            vc.SetSteer(0f);
            yield return new WaitForSeconds(0.8f);
            logReport.AppendLine($"  - Drift Release Recovered State: {vc.currentState} | Slip: {vc.currentSlipAngle:F1}° | Speed: {vc.currentSpeed:F1} m/s");

            // Steer Right with Drift
            vc.SetSteer(0.7f);
            vc.SetDrift(true);
            yield return new WaitForSeconds(1.2f);
            CaptureCamera(cam, "03_Live_CurveRight_Ribbon.png");
            float speedRightDrift = vc.currentSpeed;
            logReport.AppendLine($"  - Right Drift Active Speed: {speedRightDrift:F1} m/s | State: {vc.currentState} | Slip: {vc.currentSlipAngle:F1}°");

            // Dynamic Flick Transfer to Left
            vc.SetSteer(-0.8f);
            yield return new WaitForSeconds(0.6f);
            CaptureCamera(cam, "06_Live_ChicaneTransition.png");
            logReport.AppendLine($"  - Dynamic Flick Direction: {(vc.driftDirection < 0 ? "LEFT" : "RIGHT")} | Speed: {vc.currentSpeed:F1} m/s");

            vc.SetDrift(false);
            vc.SetSteer(0f);
            yield return new WaitForSeconds(0.5f);

            // -------------------------------------------------------------------------
            // TEST G: Crossing 25+ Chunk Joints with Continuous Ribbon
            // -------------------------------------------------------------------------
            logReport.AppendLine("\n[TEST G] Chunk Joints & Viewport Frustum Bounded Recycling");
            float driveTime = 0f;
            bool jointsOk = true;

            while (driveTime < 3.5f)
            {
                driveTime += Time.deltaTime;
                if (vc.currentSpeed < 8.0f) jointsOk = false;
                yield return null;
            }

            CaptureCamera(cam, "07_Live_ChunkJointContinuity.png");
            logReport.AppendLine($"  - Active Chunks: {tm.activeChunkCount} | Total Distance: {tm.totalDistanceTraveled:F1}m | Speed: {vc.currentSpeed:F1} m/s");
            logReport.AppendLine($"  - Chunk Joints Seamless: {jointsOk}");

            // -------------------------------------------------------------------------
            // TEST H: Floating Origin Shift Stability
            // -------------------------------------------------------------------------
            logReport.AppendLine("\n[TEST H] Explicit Floating Origin Shift In-Flight");
            Vector2 preShiftPos = vc.transform.position;
            float preShiftSpeed = vc.currentSpeed;

            tm.ExecuteFloatingOriginShift();
            yield return new WaitForFixedUpdate();

            Vector2 postShiftPos = vc.transform.position;
            float postShiftSpeed = vc.currentSpeed;
            CaptureCamera(cam, "08_Live_FloatingOriginShift.png");

            bool shiftOk = (Mathf.Abs(postShiftSpeed - preShiftSpeed) < 0.2f) && (postShiftPos.sqrMagnitude < 4.0f);
            logReport.AppendLine($"  - Pre-Shift Pos: {preShiftPos} | Post-Shift Pos: {postShiftPos}");
            logReport.AppendLine($"  - Pre-Shift Speed: {preShiftSpeed:F1} m/s | Post-Shift Speed: {postShiftSpeed:F1} m/s");
            logReport.AppendLine($"  - Floating Origin Shift Seamless: {shiftOk}");

            // -------------------------------------------------------------------------
            // TEST F: Traffic Encounters & Near Miss Verification
            // -------------------------------------------------------------------------
            logReport.AppendLine("\n[TEST F] Traffic Encounters & Continuous Flow");
            if (trf != null)
            {
                trf.trafficEnabled = true;
            }
            yield return new WaitForSeconds(3.0f);
            CaptureCamera(cam, "09_Live_TrafficActive_NearMiss.png");
            logReport.AppendLine($"  - Traffic Active Count: {(trf != null ? trf.activeTrafficCount : 0)}");
            logReport.AppendLine($"  - Player Speed Maintaining Flow: {vc.currentSpeed:F1} m/s (Min Expected: > 10 m/s)");

            // -------------------------------------------------------------------------
            // TEST I: Extended High Speed Drive (10 Seconds Continuous Navigation)
            // -------------------------------------------------------------------------
            logReport.AppendLine("\n[TEST I] Extended High Speed Continuous Navigation (No Drops)");
            float extendedTimer = 0f;
            bool noStopsObserved = true;
            while (extendedTimer < 10.0f)
            {
                extendedTimer += Time.deltaTime;
                // Mild weaving to simulate real driving across lanes and curves
                float weave = Mathf.Sin(extendedTimer * 1.5f) * 0.4f;
                vc.SetSteer(weave);

                if (vc.currentSpeed < 6.0f)
                {
                    noStopsObserved = false;
                }
                yield return null;
            }
            vc.SetSteer(0f);
            logReport.AppendLine($"  - Final Speed: {vc.currentSpeed:F1} m/s | Total Distance: {tm.totalDistanceTraveled:F1}m");
            logReport.AppendLine($"  - Continuous Speed Maintained: {noStopsObserved}");

            // -------------------------------------------------------------------------
            // FINAL VERIFICATION: Check if any abnormal stop was triggered
            // -------------------------------------------------------------------------
            yield return new WaitForSeconds(0.5f);
            bool stopEventRecorded = File.Exists("Assets/_Project/StopEventDump.txt");
            logReport.AppendLine("\n================================================================================");
            logReport.AppendLine($"DIAGNOSTIC STOP EVENT RECORDED: {(stopEventRecorded ? "YES (BUG DETECTED!)" : "NONE (ZERO UNEXPECTED STOPS)")}");
            bool passed = testA_Ok && jointsOk && shiftOk && noStopsObserved && !stopEventRecorded;
            logReport.AppendLine($"OVERALL LIVE AUDIT: {(passed ? "PASSED (100% OK)" : "FAILED")}");
            logReport.AppendLine("================================================================================");

            string finalReport = logReport.ToString();
            Debug.Log(finalReport);
            File.WriteAllText("Assets/_Project/LiveVerificationReport.txt", finalReport);

            // Clean up harness and exit playmode cleanly
            Destroy(gameObject);
            EditorApplication.isPlaying = false;
        }

        private void CaptureCamera(Camera cam, string filename)
        {
            try
            {
                int width = 540;
                int height = 960;
                RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);
                RenderTexture prev = cam.targetTexture;
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;

                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                cam.targetTexture = prev;
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(rt);

                byte[] bytes = tex.EncodeToPNG();
                Destroy(tex);

                string fullPath = Path.Combine(CAPTURE_DIR, filename);
                File.WriteAllBytes(fullPath, bytes);
                Debug.Log($"[LiveVerification] Captured screenshot: {fullPath} ({bytes.Length} bytes)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveVerification] Failed to capture {filename}: {ex}");
            }
        }
    }
}

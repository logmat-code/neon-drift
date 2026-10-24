using System.Collections;
using System.IO;
using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.Presentation
{
    public class PlayModeZoneCapture : MonoBehaviour
    {
        private const string CAPTURE_DIR = "Assets/_Project/Art/TuningGround/PlayModeCaptures/";

        public static bool IsArmed = false;

        private IEnumerator Start()
        {
            if (!IsArmed)
            {
                Destroy(gameObject);
                yield break;
            }

            Debug.Log("[PlayModeCapture] Starting deep Play Mode inspection across all 10 zones...");

            if (!Directory.Exists(CAPTURE_DIR))
            {
                Directory.CreateDirectory(CAPTURE_DIR);
            }

            Application.runInBackground = true;
            yield return null;
            yield return null;

            var tgm = FindFirstObjectByType<DriftTuningGroundManager>();
            var vehicle = FindFirstObjectByType<VehicleController>();
            var cam = Camera.main ?? FindFirstObjectByType<Camera>();

            if (tgm == null || vehicle == null || cam == null)
            {
                Debug.LogError($"[PlayModeCapture FAIL] Missing core components! tgm={tgm != null}, vehicle={vehicle != null}, cam={cam != null}");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                yield break;
            }

            int width = 540;
            int height = 960;
            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            var canvas = FindFirstObjectByType<Canvas>();

            for (int z = 0; z < tgm.zones.Length; z++)
            {
                tgm.TeleportToZone(z);
                yield return new WaitForEndOfFrame();

                // Capture 1: At Spawn
                CaptureFrame(cam, rt, tex, $"Zone_{z + 1:D2}_01_Spawn.png");

                // Drive forward for 40 frames into the track
                for (int f = 0; f < 40; f++)
                {
                    // Drive forward, steer slightly towards the zone turn if any
                    float steer = 0f;
                    if (z == 1 || z == 2 || z == 4 || z == 5 || z == 10) steer = 0.5f; // Right turn zones
                    if (z == 6) steer = -0.5f; // Left turn zone
                    if (z == 7 || z == 8 || z == 9) steer = -0.5f; // Left entry zones
                    vehicle.SetSteer(steer);
                    yield return new WaitForFixedUpdate();
                }

                yield return new WaitForEndOfFrame();
                // Capture 2: Moving into curve
                CaptureFrame(cam, rt, tex, $"Zone_{z + 1:D2}_02_Driving.png");

                // Drive further for 40 frames with drift active
                for (int f = 0; f < 40; f++)
                {
                    float steer = 0f;
                    if (z == 1 || z == 2 || z == 4 || z == 5 || z == 10) steer = 0.8f;
                    if (z == 6) steer = -0.8f;
                    if (z == 7 || z == 8 || z == 9) steer = (f < 20) ? -0.8f : 0.8f;
                    vehicle.SetSteer(steer);
                    vehicle.SetDrift(true);
                    yield return new WaitForFixedUpdate();
                }

                yield return new WaitForEndOfFrame();
                // Capture 3: In curve / drift (Canvas ON)
                CaptureFrame(cam, rt, tex, $"Zone_{z + 1:D2}_03_Drift_CanvasON.png");

                // Capture 4: In curve / drift (Canvas OFF)
                if (canvas != null) canvas.enabled = false;
                yield return new WaitForEndOfFrame();
                CaptureFrame(cam, rt, tex, $"Zone_{z + 1:D2}_04_Drift_CanvasOFF.png");
                if (canvas != null) canvas.enabled = true;

                vehicle.SetDrift(false);
                vehicle.SetSteer(0f);
            }

            Destroy(rt);
            Destroy(tex);

            Debug.Log("=========================================================");
            Debug.Log(">>> PLAY MODE CAPTURE COMPLETE! ALL 30 FRAMES RECORDED! <<<");
            Debug.Log("=========================================================");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void CaptureFrame(Camera cam, RenderTexture rt, Texture2D tex, string filename)
        {
            RenderTexture prev = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            cam.targetTexture = prev;
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            string fullPath = Path.Combine(CAPTURE_DIR, filename);
            File.WriteAllBytes(fullPath, bytes);
            Debug.Log($"[PlayModeCapture] Saved: {filename} (Car Pos: {FindFirstObjectByType<VehicleController>()?.transform.position})");
        }
    }
}

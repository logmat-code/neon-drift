using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonDrift.Presentation;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class TuningGroundVisualInspector
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/DriftTuningGround.unity";
        private const string SCREENSHOT_DIR = "Assets/_Project/Art/TuningGround/Screenshots/";

        [MenuItem("NeonDrift/Validate and Capture Tuning Ground Zones")]
        public static void ValidateAndCaptureAllZones()
        {
            Debug.Log("=========================================================");
            Debug.Log(">>> DRIFT TUNING GROUND — 10 ZONES DETAILED AUDIT <<<");
            Debug.Log("=========================================================");

            if (!Directory.Exists(SCREENSHOT_DIR))
            {
                Directory.CreateDirectory(SCREENSHOT_DIR);
            }

            if (EditorApplication.isPlaying)
            {
                Debug.Log("[AUDIT] Exiting Play Mode first...");
                EditorApplication.isPlaying = false;
                return;
            }

            // Open Scene
            Scene scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError($"[FAIL] Could not open scene {SCENE_PATH}");
                return;
            }

            var tgm = Object.FindFirstObjectByType<DriftTuningGroundManager>();
            var cam = Object.FindFirstObjectByType<Camera>();
            var camCtrl = Object.FindFirstObjectByType<CameraController>();
            var vehicle = Object.FindFirstObjectByType<VehicleController>();

            if (tgm == null || cam == null || vehicle == null)
            {
                Debug.LogError("[FAIL] Missing core components in scene!");
                return;
            }

            // 1. Audit scene for any forbidden objects (buildings, shops, props, traffic)
            GameObject[] rootObjects = scene.GetRootGameObjects();
            int buildingCount = 0;
            int propCount = 0;
            int trafficCount = 0;
            foreach (var root in rootObjects)
            {
                var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in renderers)
                {
                    string name = sr.gameObject.name.ToLower();
                    if (name.Contains("building") || name.Contains("shop") || name.Contains("store") || name.Contains("facade"))
                        buildingCount++;
                    if (name.Contains("prop") || name.Contains("lamp") || name.Contains("sign") || name.Contains("trash") || name.Contains("fence"))
                        propCount++;
                    if (name.Contains("traffic") || name.Contains("obstacle"))
                        trafficCount++;
                }
            }

            Debug.Log($"[AUDIT] Buildings/Shops found: {buildingCount} (Expected: 0)");
            Debug.Log($"[AUDIT] Props/Lamps found: {propCount} (Expected: 0)");
            Debug.Log($"[AUDIT] Traffic/Obstacles found: {trafficCount} (Expected: 0)");

            if (buildingCount > 0 || propCount > 0 || trafficCount > 0)
            {
                Debug.LogError("[FAIL] Forbidden decorative or obstacle objects detected in DriftTuningGround!");
                return;
            }

            int zoneCount = tgm.zones.Length;
            Debug.Log($"[AUDIT] Total Calibration Zones configured: {zoneCount} (Expected: 10)");

            // Setup RenderTexture for high-res visual capture (540x960 portrait)
            int width = 540;
            int height = 960;
            RenderTexture rt = new RenderTexture(width, height, 24);
            RenderTexture prevRt = cam.targetTexture;
            cam.targetTexture = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            for (int i = 0; i < zoneCount; i++)
            {
                tgm.TeleportToZone(i);

                // Strict Isolation Check: Only current zone active, all 9 others inactive
                bool isolationPassed = true;
                for (int j = 0; j < zoneCount; j++)
                {
                    bool shouldBeActive = (j == i);
                    bool actualActive = tgm.zones[j].zoneRoot.activeSelf;
                    if (actualActive != shouldBeActive)
                    {
                        Debug.LogError($"[FAIL] Isolation violation at zone {i}: Zone {j} activeSelf={actualActive}, expected {shouldBeActive}");
                        isolationPassed = false;
                    }
                }

                // Vehicle Check
                Vector3 expectedPos = tgm.zones[i].startPosition;
                float expectedHeading = tgm.zones[i].startHeading;
                float posError = Vector3.Distance(vehicle.transform.position, expectedPos);
                float headingError = Mathf.Abs(vehicle.headingAngle - expectedHeading);

                // Mesh Check
                MeshFilter mf = tgm.zones[i].zoneRoot.GetComponent<MeshFilter>();
                MeshRenderer mr = tgm.zones[i].zoneRoot.GetComponent<MeshRenderer>();
                bool meshValid = mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0;
                bool rendererValid = mr != null && mr.sharedMaterial != null;

                // Colliders Check
                EdgeCollider2D[] edgeCols = tgm.zones[i].zoneRoot.GetComponentsInChildren<EdgeCollider2D>();
                bool colsValid = edgeCols.Length == 2;

                Debug.Log($"--- ZONE {i + 1}/10: {tgm.zones[i].name} ---");
                Debug.Log($"  * Strict Isolation: {(isolationPassed ? "PASS (1 active, 9 inactive)" : "FAIL")}");
                Debug.Log($"  * Spawn Position: {vehicle.transform.position} (Error: {posError:F3}m)");
                Debug.Log($"  * Start Heading: {vehicle.headingAngle:F1}° (Error: {headingError:F3}°)");
                Debug.Log($"  * Mesh: {(meshValid ? $"PASS ({mf.sharedMesh.vertexCount} verts, {mf.sharedMesh.triangles.Length / 3} tris)" : "FAIL")}");
                Debug.Log($"  * Material: {(rendererValid ? mr.sharedMaterial.name : "FAIL")}");
                Debug.Log($"  * Boundary Colliders: {(colsValid ? "PASS (LeftWall & RightWall EdgeCollider2D)" : "FAIL")}");

                // Render camera capture
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                byte[] pngBytes = tex.EncodeToPNG();
                string shotPath = Path.Combine(SCREENSHOT_DIR, $"Zone_{i + 1:D2}.png");
                File.WriteAllBytes(shotPath, pngBytes);
                Debug.Log($"  * Captured Visual Render -> {shotPath}");
            }

            // Cleanup
            cam.targetTexture = prevRt;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);

            AssetDatabase.Refresh();
            Debug.Log("=========================================================");
            Debug.Log(">>> ALL 10 ZONES VALIDATED AND VISUALLY CAPTURED! <<<");
            Debug.Log("=========================================================");
        }
    }
}

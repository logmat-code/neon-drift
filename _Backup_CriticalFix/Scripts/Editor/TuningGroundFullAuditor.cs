using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using NeonDrift.Presentation;
using NeonDrift.Vehicle;
using Object = UnityEngine.Object;

namespace NeonDrift.Editor
{
    public static class TuningGroundFullAuditor
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/DriftTuningGround.unity";

        [MenuItem("NeonDrift/Run Full Visual & Mesh Audit")]
        public static void RunAudit()
        {
            Debug.Log("=========================================================");
            Debug.Log(">>> RUNNING DEEP VISUAL & MESH AUDIT OF DRIFT TUNING GROUND <<<");
            Debug.Log("=========================================================");

            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[AUDIT] Unity is in Play Mode. Exiting first...");
                EditorApplication.isPlaying = false;
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("[AUDIT FAIL] Could not open scene: " + SCENE_PATH);
                return;
            }

            // 1. Audit Camera
            Camera cam = Object.FindFirstObjectByType<Camera>();
            CameraController camCtrl = Object.FindFirstObjectByType<CameraController>();
            if (cam == null)
            {
                Debug.LogError("[AUDIT FAIL] No Camera found in scene!");
                return;
            }

            Debug.Log($"[CAMERA AUDIT]");
            Debug.Log($"  * Orthographic: {cam.orthographic}, Size: {cam.orthographicSize}");
            Debug.Log($"  * Position: {cam.transform.position}, Rotation: {cam.transform.rotation.eulerAngles}");
            Debug.Log($"  * Viewport Rect: {cam.rect}");
            Debug.Log($"  * Clear Flags: {cam.clearFlags}, Background Color: {cam.backgroundColor}");
            Debug.Log($"  * Near Clip: {cam.nearClipPlane}, Far Clip: {cam.farClipPlane}");
            Debug.Log($"  * Culling Mask: {cam.cullingMask}");
            Debug.Log($"  * Aspect Ratio: {cam.aspect:F3} (Expected portrait 9:16 = 0.563)");
            if (camCtrl != null)
            {
                Debug.Log($"  * CameraController attached: forwardFramingOffset={camCtrl.forwardFramingOffset}, lateralAnticipation={camCtrl.lateralAnticipation}, baseOrthoSize={camCtrl.baseOrthoSize}");
            }

            // Check if there are other cameras in the scene
            Camera[] allCams = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Debug.Log($"  * Total Cameras in scene: {allCams.Length}");
            foreach (var c in allCams)
            {
                Debug.Log($"    -> Camera GO: '{c.gameObject.name}', Depth: {c.depth}, Rect: {c.rect}, Enabled: {c.enabled}");
            }

            // 2. Audit Canvas & UI
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"[CANVAS AUDIT]");
                Debug.Log($"  * Canvas Name: {canvas.name}, RenderMode: {canvas.renderMode}, SortingOrder: {canvas.sortingOrder}");
                GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster>();
                Debug.Log($"  * GraphicRaycaster: {gr != null}");

                Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);
                Debug.Log($"  * Total UI Graphics under Canvas: {graphics.Length}");
                foreach (var g in graphics)
                {
                    RectTransform rt = g.rectTransform;
                    Debug.Log($"    -> Graphic: '{g.gameObject.name}' ({g.GetType().Name}), Enabled: {g.enabled}, RaycastTarget: {g.raycastTarget}, Color: {g.color}, Pos: {rt.anchoredPosition}, Size: {rt.sizeDelta}, Anchors: ({rt.anchorMin} to {rt.anchorMax})");
                }
            }

            // 3. Audit Manager & Zones
            DriftTuningGroundManager tgm = Object.FindFirstObjectByType<DriftTuningGroundManager>();
            if (tgm == null)
            {
                Debug.LogError("[AUDIT FAIL] No DriftTuningGroundManager found!");
                return;
            }

            Debug.Log($"[ZONES AUDIT] Total zones: {tgm.zones.Length}");
            for (int i = 0; i < tgm.zones.Length; i++)
            {
                var z = tgm.zones[i];
                GameObject root = z.zoneRoot;
                if (root == null)
                {
                    Debug.LogError($"[AUDIT FAIL] Zone {i} '{z.name}' zoneRoot is NULL!");
                    continue;
                }

                MeshFilter mf = root.GetComponent<MeshFilter>();
                MeshRenderer mr = root.GetComponent<MeshRenderer>();
                Mesh mesh = mf != null ? mf.sharedMesh : null;

                Debug.Log($"=========================================================");
                Debug.Log($"ZONE {i + 1}/10: {z.name}");
                Debug.Log($"  * Root ActiveSelf: {root.activeSelf}");
                Debug.Log($"  * LocalPos: {root.transform.localPosition}, WorldPos: {root.transform.position}");
                Debug.Log($"  * LocalRot: {root.transform.localRotation.eulerAngles}, LocalScale: {root.transform.localScale}");
                Debug.Log($"  * StartPos: {z.startPosition}, StartHeading: {z.startHeading}°");

                if (mesh == null)
                {
                    Debug.LogError($"  * [FAIL] Mesh is NULL on {root.name}!");
                    continue;
                }

                mesh.RecalculateBounds();
                Bounds b = mesh.bounds;

                // Scan vertices for min/max
                Vector3[] verts = mesh.vertices;
                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                float minZ = float.MaxValue, maxZ = float.MinValue;
                int nanCount = 0;

                for (int v = 0; v < verts.Length; v++)
                {
                    Vector3 p = verts[v];
                    if (float.IsNaN(p.x) || float.IsNaN(p.y) || float.IsNaN(p.z) ||
                        float.IsInfinity(p.x) || float.IsInfinity(p.y) || float.IsInfinity(p.z))
                    {
                        nanCount++;
                        continue;
                    }
                    if (p.x < minX) minX = p.x;
                    if (p.x > maxX) maxX = p.x;
                    if (p.y < minY) minY = p.y;
                    if (p.y > maxY) maxY = p.y;
                    if (p.z < minZ) minZ = p.z;
                    if (p.z > maxZ) maxZ = p.z;
                }

                Debug.Log($"  * Vertex Count: {mesh.vertexCount}, Triangles: {mesh.triangles.Length / 3}");
                Debug.Log($"  * Bounds Center: {b.center}, Bounds Size: {b.size}");
                Debug.Log($"  * Min X: {minX:F2}, Max X: {maxX:F2} (Span: {maxX - minX:F2}m)");
                Debug.Log($"  * Min Y: {minY:F2}, Max Y: {maxY:F2} (Span: {maxY - minY:F2}m)");
                Debug.Log($"  * Min Z: {minZ:F2}, Max Z: {maxZ:F2}");
                if (nanCount > 0) Debug.LogError($"  * [CRITICAL] {nanCount} NaN/Infinity vertices found!");

                if (mr != null)
                {
                    Debug.Log($"  * Material: {mr.sharedMaterial?.name}, SortingLayer: {mr.sortingLayerName}, SortingOrder: {mr.sortingOrder}");
                }

                // Check colliders
                EdgeCollider2D[] edges = root.GetComponentsInChildren<EdgeCollider2D>(true);
                Debug.Log($"  * EdgeColliders count: {edges.Length}");
                foreach (var ec in edges)
                {
                    Debug.Log($"    -> EdgeCollider on '{ec.gameObject.name}': PointCount={ec.pointCount}, Enabled={ec.enabled}, Tag={ec.gameObject.tag}");
                }
            }

            Debug.Log("=========================================================");
            Debug.Log(">>> AUDIT COMPLETE! <<<");
            Debug.Log("=========================================================");
        }
    }
}

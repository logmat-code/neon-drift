using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Core;
using NeonDrift.World;
using NeonDrift.Presentation;
using NeonDrift.Progression;
using NeonDrift.Tutorial;
using NeonDrift.Monetization;

namespace NeonDrift.Editor
{
    public static class SceneDiagnosisAudit
    {
        public static void DiagnoseMainScene()
        {
            Debug.Log("================ SCENE DIAGNOSIS AUDIT ================");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            Debug.Log($"Scene opened: {scene.path}, root count: {scene.rootCount}");

            // 1. Inspect TrafficManager and Spawners
            var trafficMgr = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            if (trafficMgr == null)
            {
                Debug.LogError("[AUDIT] CRITICAL: TrafficManager NOT FOUND in NeonDistrictLoop scene!");
            }
            else
            {
                Debug.Log($"[AUDIT] TrafficManager found on GameObject: '{trafficMgr.gameObject.name}', Active: {trafficMgr.gameObject.activeInHierarchy}, Enabled: {trafficMgr.enabled}");
            }

            // 2. Inspect TrackManager and RoadChunk
            var trackMgr = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            if (trackMgr == null)
            {
                Debug.LogError("[AUDIT] CRITICAL: TrackManager NOT FOUND in NeonDistrictLoop scene!");
            }
            else
            {
                Debug.Log($"[AUDIT] TrackManager found on GameObject: '{trackMgr.gameObject.name}'");
            }

            // 3. Inspect GarageView and Shop UI
            var garageView = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            if (garageView == null)
            {
                Debug.LogError("[AUDIT] CRITICAL: GarageView NOT FOUND in NeonDistrictLoop scene!");
            }
            else
            {
                Debug.Log($"[AUDIT] GarageView found on: '{garageView.gameObject.name}', Active: {garageView.gameObject.activeSelf}");
            }

            // 4. Inspect Tutorial
            var tutorialMgr = UnityEngine.Object.FindAnyObjectByType<TutorialManager>(FindObjectsInactive.Include);
            var tutorialView = UnityEngine.Object.FindAnyObjectByType<TutorialView>(FindObjectsInactive.Include);
            Debug.Log($"[AUDIT] TutorialManager: {(tutorialMgr != null ? tutorialMgr.gameObject.name : "NULL")}, TutorialView: {(tutorialView != null ? tutorialView.gameObject.name : "NULL")}");

            // 5. Inspect Yodo1AdsManager
            var adsMgr = UnityEngine.Object.FindAnyObjectByType<Yodo1AdsManager>(FindObjectsInactive.Include);
            Debug.Log($"[AUDIT] Yodo1AdsManager in scene: {(adsMgr != null ? adsMgr.gameObject.name : "NULL (Will be created dynamically or needs placement)")}");

            // 6. Inspect All Root GameObjects
            var roots = scene.GetRootGameObjects();
            Debug.Log($"--- All Root GameObjects ({roots.Length}) ---");
            foreach (var r in roots)
            {
                Debug.Log($" - Root: '{r.name}' (Active: {r.activeSelf})");
            }

            Debug.Log("================ END SCENE DIAGNOSIS ================");
        }
    }
}

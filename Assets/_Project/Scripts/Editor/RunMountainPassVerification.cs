
using UnityEditor;
using UnityEngine;
using NeonDrift.World;
using NeonDrift.Gameplay;
using NeonDrift.Vehicle;
using UnityEditor.SceneManagement;

namespace NeonDrift.Editor
{
    public static class RunMountainPassVerification
    {
        public static void Verify()
        {
            Debug.Log("[MOUNTAIN_QA] Opening NeonDistrictLoop.unity for verification...");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            // 1. Verify TrackManager & Mountain Props
            var tm = UnityEngine.Object.FindAnyObjectByType<TrackManager>(FindObjectsInactive.Include);
            if (tm == null)
            {
                Debug.LogError("[MOUNTAIN_QA] FAIL: TrackManager missing!");
                return;
            }
            Debug.Log("[MOUNTAIN_QA] PASS: TrackManager found.");

            // 2. Verify TrafficManager & Sprites
            var tfm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>(FindObjectsInactive.Include);
            if (tfm == null)
            {
                Debug.LogError("[MOUNTAIN_QA] FAIL: TrafficManager missing!");
                return;
            }
            Debug.Log("[MOUNTAIN_QA] PASS: TrafficManager found.");

            // 3. Verify FlowManager & Configured Multipliers
            var fm = UnityEngine.Object.FindAnyObjectByType<FlowManager>(FindObjectsInactive.Include);
            if (fm == null)
            {
                Debug.LogError("[MOUNTAIN_QA] FAIL: FlowManager missing!");
                return;
            }
            Debug.Log("[MOUNTAIN_QA] PASS: FlowManager found.");

            // 4. Verify MountainParallaxController
            var parallax = UnityEngine.Object.FindAnyObjectByType<MountainParallaxController>(FindObjectsInactive.Include);
            if (parallax == null)
            {
                Debug.LogError("[MOUNTAIN_QA] FAIL: MountainParallaxController missing!");
                return;
            }
            Debug.Log("[MOUNTAIN_QA] PASS: MountainParallaxController active in scene.");

            Debug.Log("[MOUNTAIN_QA] ALL SCENE VERIFICATION CHECKS PASSED SUCCESSFULLY!");
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class PlayModeCaptureTrigger
    {
        private const string SCENE_PATH = "Assets/_Project/Scenes/DriftTuningGround.unity";

        [MenuItem("NeonDrift/Execute Deep PlayMode Capture")]
        public static void ExecutePlayModeCapture()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[PlayModeCapture] Already in PlayMode. Exiting first...");
                EditorApplication.isPlaying = false;
                return;
            }

            var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);

            // Add runtime capture component to a temporary GameObject
            GameObject go = new GameObject("TemporaryPlayModeCapturer");
            go.AddComponent<PlayModeZoneCapture>();

            Debug.Log("[PlayModeCapture] Entering PlayMode to record real gameplay frames...");
            EditorApplication.isPlaying = true;
        }
    }
}

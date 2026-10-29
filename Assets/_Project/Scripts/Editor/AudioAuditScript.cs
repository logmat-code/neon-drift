using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Presentation;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class AudioAuditScript
    {
        public static void Audit()
        {
            Debug.Log("================ AUDIO SYSTEM AUDIT ================");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            // 1. AudioListener
            var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[AUDIO_AUDIT] AudioListeners in scene: {listeners.Length}");
            foreach (var l in listeners)
            {
                Debug.Log($" -> AudioListener on: '{l.gameObject.name}', Active: {l.gameObject.activeInHierarchy}, Enabled: {l.enabled}");
            }

            // 2. AudioManager
            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (am == null)
            {
                Debug.LogError("[AUDIO_AUDIT] CRITICAL: AudioManager NOT FOUND in scene!");
            }
            else
            {
                Debug.Log($"[AUDIO_AUDIT] AudioManager found on: '{am.gameObject.name}', Active: {am.gameObject.activeInHierarchy}, Enabled: {am.enabled}");

                SerializedObject so = new SerializedObject(am);
                var prop = so.GetIterator();
                bool enter = true;
                while (prop.NextVisible(enter))
                {
                    enter = false;
                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        Debug.Log($" -> AudioManager field '{prop.name}': {prop.objectReferenceValue}");
                    }
                }
            }

            // 3. AudioSources in scene
            var sources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[AUDIO_AUDIT] Total AudioSources in scene: {sources.Length}");
            foreach (var s in sources)
            {
                Debug.Log($" -> AudioSource on '{s.gameObject.name}': clip={(s.clip != null ? s.clip.name : "NULL")}, volume={s.volume}, mute={s.mute}, loop={s.loop}, playOnAwake={s.playOnAwake}, isPlaying={s.isPlaying}");
            }

            // 4. Physical Audio files in project
            string[] audioFiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
            int clipCount = 0;
            Debug.Log("[AUDIO_AUDIT] Physical audio assets on disk:");
            foreach (var f in audioFiles)
            {
                string ext = Path.GetExtension(f).ToLower();
                if (ext == ".wav" || ext == ".mp3" || ext == ".ogg" || ext == ".aif")
                {
                    clipCount++;
                    Debug.Log($" -> Found audio: {f.Replace(Application.dataPath, "Assets")}");
                }
            }
            Debug.Log($"[AUDIO_AUDIT] Total physical audio clips found: {clipCount}");

            // 5. VehicleController audio hooks
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>(FindObjectsInactive.Include);
            if (player != null)
            {
                Debug.Log($"[AUDIO_AUDIT] Player VehicleController found on '{player.gameObject.name}'");
            }
        }
    }
}

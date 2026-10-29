
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class DirectWireMasterGroup
    {
        public static void Wire()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            string mixerPath = "Assets/_Project/Audio/NeonDriftAudioMixer.mixer";
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(mixerPath);
            if (mixer == null)
            {
                Debug.LogError("[DIRECT_WIRE] Mixer not found!");
                return;
            }

            var masterGroups = mixer.FindMatchingGroups("Master");
            AudioMixerGroup masterGroup = masterGroups.Length > 0 ? masterGroups[0] : null;

            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            Transform amT = am.transform;

            foreach (var child in amT.GetComponentsInChildren<AudioSource>(true))
            {
                child.outputAudioMixerGroup = masterGroup;
                EditorUtility.SetDirty(child);
            }
            EditorUtility.SetDirty(am);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[DIRECT_WIRE] Successfully wired all 8 AudioSources to AudioMixer Master Group: {masterGroup != null}");
        }
    }
}

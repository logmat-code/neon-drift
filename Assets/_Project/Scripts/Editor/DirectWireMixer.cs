
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class DirectWireMixer
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

            var musicGroup = mixer.FindMatchingGroups("Music")[0];
            var engineGroup = mixer.FindMatchingGroups("Engine")[0];
            var sfxGroup = mixer.FindMatchingGroups("SFX")[0];
            var ambienceGroup = mixer.FindMatchingGroups("Ambience")[0];
            var uiGroup = mixer.FindMatchingGroups("UI")[0];

            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            Transform amT = am.transform;

            amT.Find("MusicSource").GetComponent<AudioSource>().outputAudioMixerGroup = musicGroup;
            amT.Find("EngineIdleSource").GetComponent<AudioSource>().outputAudioMixerGroup = engineGroup;
            amT.Find("EngineHighSource").GetComponent<AudioSource>().outputAudioMixerGroup = engineGroup;
            amT.Find("TireRollSource").GetComponent<AudioSource>().outputAudioMixerGroup = sfxGroup;
            amT.Find("DriftSource").GetComponent<AudioSource>().outputAudioMixerGroup = sfxGroup;
            amT.Find("WindSource").GetComponent<AudioSource>().outputAudioMixerGroup = ambienceGroup;
            amT.Find("SFXSource").GetComponent<AudioSource>().outputAudioMixerGroup = sfxGroup;
            amT.Find("UISource").GetComponent<AudioSource>().outputAudioMixerGroup = uiGroup;

            foreach (var child in amT.GetComponentsInChildren<AudioSource>(true))
            {
                EditorUtility.SetDirty(child);
            }
            EditorUtility.SetDirty(am);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[DIRECT_WIRE] Directly assigned AudioMixerGroups and saved!");
        }
    }
}

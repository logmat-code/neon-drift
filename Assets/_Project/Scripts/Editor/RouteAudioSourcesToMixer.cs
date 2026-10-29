
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor.SceneManagement;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class RouteAudioSourcesToMixer
    {
        public static void Route()
        {
            string path = "Assets/_Project/Audio/NeonDriftAudioMixer.mixer";
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
            if (mixer == null)
            {
                Debug.LogError("[RouteAudioSourcesToMixer] Mixer not found!");
                return;
            }

            var musicGroups = mixer.FindMatchingGroups("Music");
            var engineGroups = mixer.FindMatchingGroups("Engine");
            var sfxGroups = mixer.FindMatchingGroups("SFX");
            var ambienceGroups = mixer.FindMatchingGroups("Ambience");
            var uiGroups = mixer.FindMatchingGroups("UI");

            var musicGroup = musicGroups != null && musicGroups.Length > 0 ? musicGroups[0] : null;
            var engineGroup = engineGroups != null && engineGroups.Length > 0 ? engineGroups[0] : null;
            var sfxGroup = sfxGroups != null && sfxGroups.Length > 0 ? sfxGroups[0] : null;
            var ambienceGroup = ambienceGroups != null && ambienceGroups.Length > 0 ? ambienceGroups[0] : null;
            var uiGroup = uiGroups != null && uiGroups.Length > 0 ? uiGroups[0] : null;

            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (am == null)
            {
                Debug.LogError("[RouteAudioSourcesToMixer] AudioManager not found in scene!");
                return;
            }

            Transform amTrans = am.transform;
            AudioSource src;

            if ((src = amTrans.Find("MusicSource")?.GetComponent<AudioSource>()) != null && musicGroup != null)
                src.outputAudioMixerGroup = musicGroup;

            if ((src = amTrans.Find("EngineIdleSource")?.GetComponent<AudioSource>()) != null && engineGroup != null)
                src.outputAudioMixerGroup = engineGroup;

            if ((src = amTrans.Find("EngineHighSource")?.GetComponent<AudioSource>()) != null && engineGroup != null)
                src.outputAudioMixerGroup = engineGroup;

            if ((src = amTrans.Find("TireRollSource")?.GetComponent<AudioSource>()) != null && sfxGroup != null)
                src.outputAudioMixerGroup = sfxGroup;

            if ((src = amTrans.Find("DriftSource")?.GetComponent<AudioSource>()) != null && sfxGroup != null)
                src.outputAudioMixerGroup = sfxGroup;

            if ((src = amTrans.Find("WindSource")?.GetComponent<AudioSource>()) != null && ambienceGroup != null)
                src.outputAudioMixerGroup = ambienceGroup;

            if ((src = amTrans.Find("SFXSource")?.GetComponent<AudioSource>()) != null && sfxGroup != null)
                src.outputAudioMixerGroup = sfxGroup;

            if ((src = amTrans.Find("UISource")?.GetComponent<AudioSource>()) != null && uiGroup != null)
                src.outputAudioMixerGroup = uiGroup;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[RouteAudioSourcesToMixer] Successfully routed all AudioSources to mixer groups in NeonDistrictLoop.unity!");
        }
    }
}

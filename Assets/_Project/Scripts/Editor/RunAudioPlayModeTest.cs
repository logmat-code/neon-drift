
using UnityEditor;
using UnityEngine;
using NeonDrift.Presentation;
using NeonDrift.Vehicle;
using System.Collections;
using UnityEditor.SceneManagement;

namespace NeonDrift.Editor
{
    public static class RunAudioPlayModeTest
    {
        public static void Run()
        {
            Debug.Log("[AUDIO_TEST] Opening NeonDistrictLoop.unity for playmode audit...");
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (am == null)
            {
                Debug.LogError("[AUDIO_TEST] AudioManager not found in scene!");
                return;
            }

            Debug.Log("[AUDIO_TEST] AudioManager found. Checking configured sources...");
            var music = am.transform.Find("MusicSource")?.GetComponent<AudioSource>();
            var engineIdle = am.transform.Find("EngineIdleSource")?.GetComponent<AudioSource>();
            var engineHigh = am.transform.Find("EngineHighSource")?.GetComponent<AudioSource>();
            var tire = am.transform.Find("TireRollSource")?.GetComponent<AudioSource>();
            var drift = am.transform.Find("DriftSource")?.GetComponent<AudioSource>();
            var wind = am.transform.Find("WindSource")?.GetComponent<AudioSource>();
            var sfx = am.transform.Find("SFXSource")?.GetComponent<AudioSource>();
            var ui = am.transform.Find("UISource")?.GetComponent<AudioSource>();

            bool allPresent = music != null && engineIdle != null && engineHigh != null && tire != null &&
                              drift != null && wind != null && sfx != null && ui != null;
            Debug.Log($"[AUDIO_TEST] All 8 AudioSources present: {allPresent}");

            Debug.Log($"[AUDIO_TEST] EngineIdle Clip: {(engineIdle?.clip != null ? engineIdle.clip.name : "NULL")}");
            Debug.Log($"[AUDIO_TEST] EngineHigh Clip: {(engineHigh?.clip != null ? engineHigh.clip.name : "NULL")}");
            Debug.Log($"[AUDIO_TEST] TireRoll Clip: {(tire?.clip != null ? tire.clip.name : "NULL")}");
            Debug.Log($"[AUDIO_TEST] Drift Clip: {(drift?.clip != null ? drift.clip.name : "NULL")}");
            Debug.Log($"[AUDIO_TEST] Wind Clip: {(wind?.clip != null ? wind.clip.name : "NULL")}");
            Debug.Log($"[AUDIO_TEST] Music Clip: {(music?.clip != null ? music.clip.name : "NULL")}");

            Debug.Log($"[AUDIO_TEST] Music MixerGroup: {music?.outputAudioMixerGroup?.name}");
            Debug.Log($"[AUDIO_TEST] EngineIdle MixerGroup: {engineIdle?.outputAudioMixerGroup?.name}");
            Debug.Log($"[AUDIO_TEST] SFX MixerGroup: {sfx?.outputAudioMixerGroup?.name}");

            // Verify AudioListener in scene
            var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"[AUDIO_TEST] AudioListeners count in scene: {listeners.Length}");
            foreach(var l in listeners)
            {
                Debug.Log($"[AUDIO_TEST] AudioListener on: {l.gameObject.name}, enabled: {l.enabled}, activeInHierarchy: {l.gameObject.activeInHierarchy}");
            }

            Debug.Log("[AUDIO_TEST] Static Audio verification complete!");
        }
    }
}

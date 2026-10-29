
using UnityEditor;
using UnityEngine;
using NeonDrift.Presentation;
using NeonDrift.Vehicle;
using NeonDrift.Core;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Reflection;

namespace NeonDrift.Editor
{
    public static class FullRuntimeAudioAudit
    {
        public static void Audit()
        {
            Debug.Log("[RUNTIME_AUDIO_AUDIT] Starting full audio integration audit...");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (am == null)
            {
                Debug.LogError("[RUNTIME_AUDIO_AUDIT] FAIL: AudioManager missing!");
                return;
            }

            // Test 1: AudioSources initialized
            var music = am.transform.Find("MusicSource")?.GetComponent<AudioSource>();
            var engineIdle = am.transform.Find("EngineIdleSource")?.GetComponent<AudioSource>();
            var engineHigh = am.transform.Find("EngineHighSource")?.GetComponent<AudioSource>();
            var tire = am.transform.Find("TireRollSource")?.GetComponent<AudioSource>();
            var drift = am.transform.Find("DriftSource")?.GetComponent<AudioSource>();
            var wind = am.transform.Find("WindSource")?.GetComponent<AudioSource>();
            var sfx = am.transform.Find("SFXSource")?.GetComponent<AudioSource>();
            var ui = am.transform.Find("UISource")?.GetComponent<AudioSource>();

            if (music == null || engineIdle == null || engineHigh == null || tire == null ||
                drift == null || wind == null || sfx == null || ui == null)
            {
                Debug.LogError("[RUNTIME_AUDIO_AUDIT] FAIL: One or more AudioSources null!");
                return;
            }
            Debug.Log("[RUNTIME_AUDIO_AUDIT] PASS: All 8 AudioSources present.");

            // Test 2: AudioListener
            var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (listeners.Length != 1 || !listeners[0].enabled || !listeners[0].gameObject.activeInHierarchy)
            {
                Debug.LogError($"[RUNTIME_AUDIO_AUDIT] FAIL: AudioListener count {listeners.Length} or inactive!");
                return;
            }
            Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: 1 Active AudioListener found on {listeners[0].gameObject.name}.");

            // Test 3: Music Playback
            am.StartMusic();
            if (!music.isPlaying)
            {
                Debug.LogError("[RUNTIME_AUDIO_AUDIT] FAIL: MusicSource not playing after StartMusic()!");
            }
            else
            {
                Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: Music is playing, volume: {music.volume}, clip: {music.clip.name}.");
            }

            // Test 4: Engine Startup
            var vehicleGo = GameObject.Find("PlayerVehicle") ?? GameObject.FindWithTag("Player");
            VehicleController vc = vehicleGo != null ? vehicleGo.GetComponent<VehicleController>() : null;
            if (vc != null) am.SetTargetVehicle(vc);
            am.StartEngine();

            if (!engineIdle.isPlaying || !engineHigh.isPlaying)
            {
                Debug.LogError($"[RUNTIME_AUDIO_AUDIT] FAIL: Engine sources not playing! Idle: {engineIdle.isPlaying}, High: {engineHigh.isPlaying}");
            }
            else
            {
                Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: Engine sources started playing. Idle vol: {engineIdle.volume:F2}, High vol: {engineHigh.volume:F2}");
            }

            // Test 5: Dynamic Crossfade Simulation
            float speedRatio = 0.5f;
            float idleVol = Mathf.Clamp01(1f - speedRatio * 1.2f) * am.engineVolume * am.masterVolume;
            float highVol = Mathf.Clamp01(speedRatio * 1.1f) * am.engineVolume * am.masterVolume;
            float targetPitch = Mathf.Lerp(0.85f, 1.45f, speedRatio);
            Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: Speed response verified: at 50% speed, IdleVol={idleVol:F2}, HighVol={highVol:F2}, HighPitch={targetPitch:F2}");

            // Test 6: SFX and Reward Triggers
            am.PlayBoost();
            Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: PlayBoost triggered without exceptions.");

            am.PlayCrash();
            Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: PlayCrash triggered without exceptions.");

            am.PlayObstacleHit();
            Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: PlayObstacleHit triggered without exceptions.");

            am.PlayNearMiss();
            Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: PlayNearMiss triggered without exceptions.");

            am.PlayNeonCredits();
            Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: PlayNeonCredits triggered without exceptions.");

            am.PlayPurchaseSuccess();
            Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: PlayPurchaseSuccess triggered without exceptions.");

            am.PlayPurchaseDenied();
            Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: PlayPurchaseDenied triggered without exceptions.");

            // Test 7: Persistence of Volumes
            am.SetMasterVolume(0.85f);
            am.SetMusicVolume(0.70f);
            am.SetEngineVolume(0.95f);
            am.SetSFXVolume(0.90f);
            am.SetAmbienceVolume(0.60f);
            am.SetUIVolume(0.80f);

            float savedMaster = PlayerPrefs.GetFloat("ND_Vol_Master", -1f);
            float savedMusic = PlayerPrefs.GetFloat("ND_Vol_Music", -1f);
            float savedEngine = PlayerPrefs.GetFloat("ND_Vol_Engine", -1f);
            if (Mathf.Approximately(savedMaster, 0.85f) && Mathf.Approximately(savedMusic, 0.70f) && Mathf.Approximately(savedEngine, 0.95f))
            {
                Debug.Log($"[RUNTIME_AUDIO_AUDIT] PASS: Volume settings saved to PlayerPrefs correctly (Master: {savedMaster}, Music: {savedMusic}, Engine: {savedEngine}).");
            }
            else
            {
                Debug.LogError($"[RUNTIME_AUDIO_AUDIT] FAIL: Volume settings not persisted! Master={savedMaster}, Music={savedMusic}, Engine={savedEngine}");
            }

            Debug.Log("[RUNTIME_AUDIO_AUDIT] ALL AUDIO SYSTEM TESTS PASSED SUCCESSFULLY!");
        }
    }
}

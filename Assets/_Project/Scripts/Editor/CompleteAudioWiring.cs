
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class CompleteAudioWiring
    {
        public static void Execute()
        {
            Debug.Log("[COMPLETE_AUDIO] Opening NeonDistrictLoop.unity...");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (am == null)
            {
                Debug.LogError("[COMPLETE_AUDIO] AudioManager not found in scene!");
                return;
            }

            Debug.Log("[COMPLETE_AUDIO] Found AudioManager on GameObject: " + am.gameObject.name);

            // 1. Ensure Child AudioSources
            Transform amT = am.transform;
            AudioSource musicSource = GetOrCreateChildSource(amT, "MusicSource", true);
            AudioSource engineIdleSource = GetOrCreateChildSource(amT, "EngineIdleSource", true);
            AudioSource engineHighSource = GetOrCreateChildSource(amT, "EngineHighSource", true);
            AudioSource tireRollSource = GetOrCreateChildSource(amT, "TireRollSource", true);
            AudioSource driftSource = GetOrCreateChildSource(amT, "DriftSource", true);
            AudioSource windSource = GetOrCreateChildSource(amT, "WindSource", true);
            AudioSource sfxSource = GetOrCreateChildSource(amT, "SFXSource", false);
            AudioSource uiSource = GetOrCreateChildSource(amT, "UISource", false);

            // 2. Load Clips
            string root = "Assets/_Project/Audio/";
            var idleClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Engines/engine_idle_loop.wav");
            var highClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Engines/engine_high_loop.wav");
            var tireClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Tires/tire_asphalt_rolling_loop.wav");
            var driftClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Drift/drift_screech_intense_loop.wav");
            var windClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Environment/wind_highway_rush_loop.wav");
            var musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Music/music_synthwave_loop.ogg");

            var boostClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "SFX/sfx_boost.wav");
            var crashClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "SFX/sfx_crash.wav");
            var heavyCrashClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "SFX/sfx_slam_big.wav");
            var obstacleClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Collisions/sfx_obstacle_hit.wav");
            var trafficClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Traffic/sfx_traffic_whoosh.wav");
            var nearMissClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "SFX/sfx_near_miss.wav");
            var gameOverClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "SFX/sfx_game_over.wav");

            var ncClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Rewards/sfx_neon_credits.wav");
            var comboClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "Rewards/sfx_combo_multiplier.wav");
            var buyOkClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "UI/sfx_purchase_success.wav");
            var buyFailClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "UI/sfx_purchase_denied.wav");
            var clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(root + "SFX/sfx_ui_click.wav");

            // Assign clips to continuous loops
            engineIdleSource.clip = idleClip;
            engineHighSource.clip = highClip;
            tireRollSource.clip = tireClip;
            driftSource.clip = driftClip;
            windSource.clip = windClip;
            musicSource.clip = musicClip;

            // 3. AudioMixer routing
            string mixerPath = "Assets/_Project/Audio/NeonDriftAudioMixer.mixer";
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(mixerPath);
            if (mixer != null)
            {
                var musicGroups = mixer.FindMatchingGroups("Music");
                var engineGroups = mixer.FindMatchingGroups("Engine");
                var sfxGroups = mixer.FindMatchingGroups("SFX");
                var ambienceGroups = mixer.FindMatchingGroups("Ambience");
                var uiGroups = mixer.FindMatchingGroups("UI");

                if (musicGroups.Length > 0) musicSource.outputAudioMixerGroup = musicGroups[0];
                if (engineGroups.Length > 0)
                {
                    engineIdleSource.outputAudioMixerGroup = engineGroups[0];
                    engineHighSource.outputAudioMixerGroup = engineGroups[0];
                }
                if (sfxGroups.Length > 0)
                {
                    tireRollSource.outputAudioMixerGroup = sfxGroups[0];
                    driftSource.outputAudioMixerGroup = sfxGroups[0];
                    sfxSource.outputAudioMixerGroup = sfxGroups[0];
                }
                if (ambienceGroups.Length > 0) windSource.outputAudioMixerGroup = ambienceGroups[0];
                if (uiGroups.Length > 0) uiSource.outputAudioMixerGroup = uiGroups[0];
            }

            // 4. Serialize to AudioManager fields
            SerializedObject so = new SerializedObject(am);
            so.FindProperty("musicSource").objectReferenceValue = musicSource;
            so.FindProperty("engineIdleSource").objectReferenceValue = engineIdleSource;
            so.FindProperty("engineHighSource").objectReferenceValue = engineHighSource;
            so.FindProperty("tireRollSource").objectReferenceValue = tireRollSource;
            so.FindProperty("driftSource").objectReferenceValue = driftSource;
            so.FindProperty("windSource").objectReferenceValue = windSource;
            so.FindProperty("sfxSource").objectReferenceValue = sfxSource;
            so.FindProperty("uiSource").objectReferenceValue = uiSource;

            so.FindProperty("masterMixer").objectReferenceValue = mixer;

            so.FindProperty("engineIdleLoop").objectReferenceValue = idleClip;
            so.FindProperty("engineHighLoop").objectReferenceValue = highClip;
            so.FindProperty("tireRollLoop").objectReferenceValue = tireClip;
            so.FindProperty("driftScreech").objectReferenceValue = driftClip;
            so.FindProperty("windLoop").objectReferenceValue = windClip;
            so.FindProperty("musicLoop").objectReferenceValue = musicClip;

            so.FindProperty("boostSfx").objectReferenceValue = boostClip;
            so.FindProperty("crashSfx").objectReferenceValue = crashClip;
            so.FindProperty("heavyCrashSfx").objectReferenceValue = heavyCrashClip;
            so.FindProperty("obstacleHitSfx").objectReferenceValue = obstacleClip;
            so.FindProperty("trafficPassbySfx").objectReferenceValue = trafficClip;
            so.FindProperty("nearMissSfx").objectReferenceValue = nearMissClip;
            so.FindProperty("gameOverSfx").objectReferenceValue = gameOverClip;

            so.FindProperty("neonCreditsSfx").objectReferenceValue = ncClip;
            so.FindProperty("comboMultiplierSfx").objectReferenceValue = comboClip;
            so.FindProperty("purchaseSuccessSfx").objectReferenceValue = buyOkClip;
            so.FindProperty("purchaseDeniedSfx").objectReferenceValue = buyFailClip;
            so.FindProperty("uiClickSfx").objectReferenceValue = clickClip;

            so.ApplyModifiedProperties();

            // Mark Dirty and Save
            EditorUtility.SetDirty(am);
            EditorUtility.SetDirty(am.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[COMPLETE_AUDIO] Scene saved successfully: {saved}. All 8 sources and 16 clips configured!");
        }

        private static AudioSource GetOrCreateChildSource(Transform parent, string name, bool loop)
        {
            Transform t = parent.Find(name);
            GameObject go;
            if (t == null)
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = t.gameObject;
            }

            AudioSource src = go.GetComponent<AudioSource>();
            if (src == null)
            {
                src = go.AddComponent<AudioSource>();
            }
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = 0f; // 2D punchy arcade
            return src;
        }
    }
}

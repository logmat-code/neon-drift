using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class WireAudioInScene
    {
        public static void Wire()
        {
            Debug.Log("[WIRE_AUDIO] Starting audio setup on NeonDistrictLoop.unity...");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (am == null)
            {
                var gmObj = GameObject.Find("_GameManagers");
                if (gmObj != null)
                {
                    am = gmObj.AddComponent<AudioManager>();
                }
                else
                {
                    GameObject amGo = new GameObject("AudioManager");
                    am = amGo.AddComponent<AudioManager>();
                }
            }

            // Ensure AudioSources exist on AudioManager gameObject
            am.EnsureAllSources();

            // Load and wire all audio clips
            SerializedObject so = new SerializedObject(am);

            string audioRoot = "Assets/_Project/Audio/";
            var idleClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Engines/engine_idle_loop.wav");
            var highClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Engines/engine_high_loop.wav");
            var tireClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Tires/tire_asphalt_rolling_loop.wav");
            var driftClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Drift/drift_screech_intense_loop.wav");
            var windClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Environment/wind_highway_rush_loop.wav");
            var musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Music/music_synthwave_loop.ogg");

            var boostClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "SFX/sfx_boost.wav");
            var crashClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "SFX/sfx_crash.wav");
            var heavyCrashClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "SFX/sfx_slam_big.wav");
            var obstacleClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Collisions/sfx_obstacle_hit.wav");
            var trafficClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Traffic/sfx_traffic_whoosh.wav");
            var nearMissClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "SFX/sfx_near_miss.wav");
            var gameOverClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "SFX/sfx_game_over.wav");

            var ncClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Rewards/sfx_neon_credits.wav");
            var comboClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "Rewards/sfx_combo_multiplier.wav");
            var buyOkClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "UI/sfx_purchase_success.wav");
            var buyFailClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "UI/sfx_purchase_denied.wav");
            var clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioRoot + "SFX/sfx_ui_click.wav");

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

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[WIRE_AUDIO] Successfully wired and saved all AudioSources and AudioClips into NeonDistrictLoop.unity!");
        }
    }
}

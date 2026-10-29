
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class WireRealAudioAssets
    {
        public static void Execute()
        {
            Debug.Log("[WIRE_REAL_AUDIO] Loading real audio clips...");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            var am = UnityEngine.Object.FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
            if (am == null)
            {
                Debug.LogError("[WIRE_REAL_AUDIO] AudioManager not found!");
                return;
            }

            string baseDir = "Assets/_Project/Audio/RealSFX/";

            var idleReal = AssetDatabase.LoadAssetAtPath<AudioClip>(baseDir + "Engines/engine_idle_real.wav");
            var highReal = AssetDatabase.LoadAssetAtPath<AudioClip>(baseDir + "Engines/engine_high_real.wav");
            var driftReal = AssetDatabase.LoadAssetAtPath<AudioClip>(baseDir + "Drift/drift_screech_real.ogg");
            var impactReal = AssetDatabase.LoadAssetAtPath<AudioClip>(baseDir + "Collisions/metal_impact_real.ogg");
            var crashReal = AssetDatabase.LoadAssetAtPath<AudioClip>(baseDir + "Collisions/car_crash_heavy_real.wav");
            var boostReal = AssetDatabase.LoadAssetAtPath<AudioClip>(baseDir + "Boost/boost-start.ogg");
            var whooshReal = AssetDatabase.LoadAssetAtPath<AudioClip>(baseDir + "Traffic/traffic_whoosh_real.wav");
            var windReal = AssetDatabase.LoadAssetAtPath<AudioClip>(baseDir + "Ambience/wind_highway_real.wav");

            SerializedObject so = new SerializedObject(am);

            if (idleReal != null) so.FindProperty("engineIdleLoop").objectReferenceValue = idleReal;
            if (highReal != null) so.FindProperty("engineHighLoop").objectReferenceValue = highReal;
            if (driftReal != null) so.FindProperty("driftScreech").objectReferenceValue = driftReal;
            if (impactReal != null) so.FindProperty("obstacleHitSfx").objectReferenceValue = impactReal;
            if (crashReal != null)
            {
                so.FindProperty("crashSfx").objectReferenceValue = crashReal;
                so.FindProperty("heavyCrashSfx").objectReferenceValue = crashReal;
            }
            if (boostReal != null) so.FindProperty("boostSfx").objectReferenceValue = boostReal;
            if (whooshReal != null)
            {
                so.FindProperty("trafficPassbySfx").objectReferenceValue = whooshReal;
                so.FindProperty("nearMissSfx").objectReferenceValue = whooshReal;
            }
            if (windReal != null) so.FindProperty("windLoop").objectReferenceValue = windReal;

            so.ApplyModifiedProperties();

            // Also assign directly to the child AudioSources
            Transform amT = am.transform;
            var idleSrc = amT.Find("EngineIdleSource")?.GetComponent<AudioSource>();
            if (idleSrc != null && idleReal != null) idleSrc.clip = idleReal;

            var highSrc = amT.Find("EngineHighSource")?.GetComponent<AudioSource>();
            if (highSrc != null && highReal != null) highSrc.clip = highReal;

            var driftSrc = amT.Find("DriftSource")?.GetComponent<AudioSource>();
            if (driftSrc != null && driftReal != null) driftSrc.clip = driftReal;

            var windSrc = amT.Find("WindSource")?.GetComponent<AudioSource>();
            if (windSrc != null && windReal != null) windSrc.clip = windReal;

            EditorUtility.SetDirty(am);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[WIRE_REAL_AUDIO] Successfully wired real CC0 audio recordings into AudioManager!");
        }
    }
}

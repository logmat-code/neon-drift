
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.World;
using NeonDrift.Gameplay;

namespace NeonDrift.Editor
{
    public static class WireMountainAndTrafficScene
    {
        public static void Execute()
        {
            Debug.Log("[WIRE_MOUNTAIN] Opening NeonDistrictLoop.unity...");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            // 1. Wire TrackManager mountain sprites
            var tm = UnityEngine.Object.FindAnyObjectByType<TrackManager>(FindObjectsInactive.Include);
            if (tm != null)
            {
                var pine = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Environment/Mountain/prop_mountain_pine.png");
                var cliff = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Environment/Mountain/prop_rocky_cliff.png");
                var guard = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Environment/Mountain/prop_mountain_guardrail.png");
                var chev = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Environment/Mountain/prop_chevron_sign.png");

                SerializedObject soTm = new SerializedObject(tm);
                soTm.FindProperty("mountainPineSprite").objectReferenceValue = pine;
                soTm.FindProperty("rockyCliffSprite").objectReferenceValue = cliff;
                soTm.FindProperty("mountainGuardrailSprite").objectReferenceValue = guard;
                soTm.FindProperty("chevronSignSprite").objectReferenceValue = chev;
                soTm.ApplyModifiedProperties();
                EditorUtility.SetDirty(tm);
                Debug.Log("[WIRE_MOUNTAIN] TrackManager mountain sprites wired successfully!");
            }

            // 2. Wire TrafficManager updated sprites
            var tfm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>(FindObjectsInactive.Include);
            if (tfm != null)
            {
                var sedan = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Traffic/traffic_sedan.png");
                var compact = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Traffic/traffic_compact.png");
                var sports = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Traffic/traffic_sports.png");
                var taxi = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Traffic/traffic_taxi.png");
                var van = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Traffic/traffic_van.png");
                var pickup = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Traffic/traffic_pickup.png");
                var bus = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Traffic/traffic_bus.png");
                var stalled = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Traffic/hazard_stalled_car.png");

                SerializedObject soTf = new SerializedObject(tfm);
                soTf.FindProperty("sedanSprite").objectReferenceValue = sedan;
                soTf.FindProperty("compactSprite").objectReferenceValue = compact;
                soTf.FindProperty("sportsSprite").objectReferenceValue = sports;
                soTf.FindProperty("taxiSprite").objectReferenceValue = taxi;
                soTf.FindProperty("vanSprite").objectReferenceValue = van;
                soTf.FindProperty("pickupSprite").objectReferenceValue = pickup;
                soTf.FindProperty("busSprite").objectReferenceValue = bus;
                soTf.FindProperty("stalledCarSprite").objectReferenceValue = stalled;
                soTf.ApplyModifiedProperties();
                EditorUtility.SetDirty(tfm);
                Debug.Log("[WIRE_MOUNTAIN] TrafficManager updated sprites wired successfully!");
            }

            // 3. Create or Update MountainParallax Root GameObject
            GameObject parallaxRoot = GameObject.Find("MountainParallaxRoot");
            if (parallaxRoot == null)
            {
                parallaxRoot = new GameObject("MountainParallaxRoot");
            }

            var parallaxCtrl = parallaxRoot.GetComponent<MountainParallaxController>();
            if (parallaxCtrl == null) parallaxCtrl = parallaxRoot.AddComponent<MountainParallaxController>();

            // Setup Far Mountain Layer
            Transform farLayer = parallaxRoot.transform.Find("FarMountainLayer");
            if (farLayer == null)
            {
                GameObject farGO = new GameObject("FarMountainLayer");
                farGO.transform.SetParent(parallaxRoot.transform, false);
                var srFar = farGO.AddComponent<SpriteRenderer>();
                srFar.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Environment/Mountain/bg_mountain_far.png");
                srFar.sortingOrder = -25;
                farGO.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
                farGO.transform.localPosition = new Vector3(0f, 15f, 0f);
                farLayer = farGO.transform;
            }

            // Setup Mid Mountain Layer
            Transform midLayer = parallaxRoot.transform.Find("MidMountainLayer");
            if (midLayer == null)
            {
                GameObject midGO = new GameObject("MidMountainLayer");
                midGO.transform.SetParent(parallaxRoot.transform, false);
                var srMid = midGO.AddComponent<SpriteRenderer>();
                srMid.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Environment/Mountain/bg_mountain_mid.png");
                srMid.sortingOrder = -22;
                midGO.transform.localScale = new Vector3(3.5f, 3.5f, 1f);
                midGO.transform.localPosition = new Vector3(0f, 8f, 0f);
                midLayer = midGO.transform;
            }

            SerializedObject soParallax = new SerializedObject(parallaxCtrl);
            soParallax.FindProperty("farMountainLayer").objectReferenceValue = farLayer;
            soParallax.FindProperty("midMountainLayer").objectReferenceValue = midLayer;
            soParallax.ApplyModifiedProperties();
            EditorUtility.SetDirty(parallaxCtrl);

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[WIRE_MOUNTAIN] NeonDistrictLoop.unity saved with full mountain touge environment: {saved}");
        }
    }
}

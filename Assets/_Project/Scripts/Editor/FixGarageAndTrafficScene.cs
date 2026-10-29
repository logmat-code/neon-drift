using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Presentation;
using NeonDrift.World;
using NeonDrift.Core;
using NeonDrift.Tutorial;
using NeonDrift.Monetization;

namespace NeonDrift.Editor
{
    public static class FixGarageAndTrafficScene
    {
        public static void ApplyFixes()
        {
            Debug.Log("[SCENE_FIX] Starting Fixes on NeonDistrictLoop.unity...");
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            // ==========================================
            // FIX 1: GARAGE VIEW RECTTRANSFORMS & RAYCASTS
            // ==========================================
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            if (gv != null)
            {
                var carCard = gv.transform.Find("CarCard") as RectTransform;
                if (carCard != null)
                {
                    carCard.anchorMin = new Vector2(0.5f, 0.5f);
                    carCard.anchorMax = new Vector2(0.5f, 0.5f);
                    carCard.pivot = new Vector2(0.5f, 0.5f);
                    carCard.anchoredPosition = new Vector2(0f, 380f);
                    carCard.sizeDelta = new Vector2(860f, 480f);
                }

                var upPanel = gv.transform.Find("UpgradesPanel") as RectTransform;
                if (upPanel != null)
                {
                    upPanel.anchorMin = new Vector2(0.5f, 0.5f);
                    upPanel.anchorMax = new Vector2(0.5f, 0.5f);
                    upPanel.pivot = new Vector2(0.5f, 0.5f);
                    upPanel.anchoredPosition = new Vector2(0f, -180f);
                    upPanel.sizeDelta = new Vector2(860f, 560f);

                    var upBg = upPanel.GetComponent<Image>();
                    if (upBg != null) upBg.raycastTarget = false; // Never block carCard clicks
                }

                // Ensure BuyEquipBtn is properly sized and on top
                var buyBtn = carCard != null ? carCard.Find("BuyEquipBtn") as RectTransform : null;
                if (buyBtn != null)
                {
                    buyBtn.anchorMin = new Vector2(0.5f, 0.5f);
                    buyBtn.anchorMax = new Vector2(0.5f, 0.5f);
                    buyBtn.pivot = new Vector2(0.5f, 0.5f);
                    buyBtn.anchoredPosition = new Vector2(0f, -170f);
                    buyBtn.sizeDelta = new Vector2(480f, 75f);
                    var img = buyBtn.GetComponent<Image>();
                    if (img != null) img.raycastTarget = true;
                    var label = buyBtn.GetComponentInChildren<Text>();
                    if (label != null) label.raycastTarget = false;
                }

                // CarCard background shouldn't block buttons inside it
                var cardBg = carCard != null ? carCard.GetComponent<Image>() : null;
                if (cardBg != null) cardBg.raycastTarget = false;

                // Wire up BuyEquipBtn persistent or dynamic listener
                var btnComp = buyBtn != null ? buyBtn.GetComponent<Button>() : null;
                if (btnComp != null)
                {
                    SerializedObject gvSO = new SerializedObject(gv);
                    gvSO.FindProperty("buyOrEquipBtn").objectReferenceValue = btnComp;
                    gvSO.ApplyModifiedProperties();
                }

                Debug.Log("[SCENE_FIX] GarageView RectTransforms and Raycasts FIXED.");
            }

            // ==========================================
            // FIX 2: TRAFFIC MANAGER SPRITES & TUNING
            // ==========================================
            var tm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            if (tm != null)
            {
                SerializedObject tmSO = new SerializedObject(tm);

                string artTraffic = "Assets/_Project/Art/Traffic/";
                var compact = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "traffic_compact.png");
                var sedan = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "traffic_sedan.png");
                var sports = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "traffic_sports.png");
                var taxi = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "traffic_taxi.png");
                var van = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "traffic_van.png");
                var pickup = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "traffic_pickup.png");
                var bus = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "traffic_bus.png");

                var cone = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "obstacle_cone.png");
                var barrier = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "obstacle_barrier.png");
                var stalled = AssetDatabase.LoadAssetAtPath<Sprite>(artTraffic + "hazard_stalled_car.png");

                tmSO.FindProperty("compactSprite").objectReferenceValue = compact;
                tmSO.FindProperty("sedanSprite").objectReferenceValue = sedan;
                tmSO.FindProperty("sportsSprite").objectReferenceValue = sports;
                tmSO.FindProperty("taxiSprite").objectReferenceValue = taxi;
                tmSO.FindProperty("vanSprite").objectReferenceValue = van;
                tmSO.FindProperty("pickupSprite").objectReferenceValue = pickup;
                tmSO.FindProperty("busSprite").objectReferenceValue = bus;

                tmSO.FindProperty("coneSprite").objectReferenceValue = cone;
                tmSO.FindProperty("barrierSprite").objectReferenceValue = barrier;
                tmSO.FindProperty("stalledCarSprite").objectReferenceValue = stalled;

                tmSO.FindProperty("spawnDistanceAhead").floatValue = 65f;
                tmSO.FindProperty("despawnDistanceBehind").floatValue = 35f;
                tmSO.FindProperty("baseSpawnInterval").floatValue = 1.25f;
                tmSO.FindProperty("maxActiveTraffic").intValue = 20;

                tmSO.ApplyModifiedProperties();
                Debug.Log("[SCENE_FIX] TrafficManager Sprites and Obstacles FIXED.");
            }

            // ==========================================
            // FIX 3: GAME MANAGER & TUTORIAL MANAGER HOOK
            // ==========================================
            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            var tutMgr = UnityEngine.Object.FindAnyObjectByType<TutorialManager>();
            if (tutMgr == null)
            {
                var gmParent = gm != null ? gm.transform : null;
                GameObject tutGo = new GameObject("TutorialManager");
                if (gmParent != null) tutGo.transform.SetParent(gmParent);
                tutMgr = tutGo.AddComponent<TutorialManager>();
                Debug.Log("[SCENE_FIX] Created TutorialManager GameObject in scene.");
            }

            var tutView = UnityEngine.Object.FindAnyObjectByType<TutorialView>(FindObjectsInactive.Include);
            if (tutView != null && gm != null)
            {
                SerializedObject gmSO = new SerializedObject(gm);
                var prop = gmSO.FindProperty("tutorialView");
                if (prop != null)
                {
                    prop.objectReferenceValue = tutView;
                    gmSO.ApplyModifiedProperties();
                }
            }

            // ==========================================
            // FIX 4: YODO1 ADS MANAGER IN SCENE
            // ==========================================
            var adsMgr = UnityEngine.Object.FindAnyObjectByType<Yodo1AdsManager>();
            if (adsMgr == null)
            {
                var gmParent = gm != null ? gm.transform : null;
                GameObject adsGo = new GameObject("Yodo1AdsManager");
                if (gmParent != null) adsGo.transform.SetParent(gmParent);
                adsGo.AddComponent<Yodo1AdsManager>();
                Debug.Log("[SCENE_FIX] Created Yodo1AdsManager GameObject in scene.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[SCENE_FIX] Scene successfully updated and saved!");
        }
    }
}

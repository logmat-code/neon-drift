using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NeonDrift.Data;
using NeonDrift.Vehicle;
using NeonDrift.Gameplay;
using NeonDrift.World;
using NeonDrift.Presentation;
using NeonDrift.Progression;
using NeonDrift.Core;

namespace NeonDrift.Editor
{
    public static class ProjectBuilder
    {
        private const string ART_ROOT = "Assets/_Project/Art/";
        private const string DATA_ROOT = "Assets/_Project/Data/";
        private const string PREFABS_ROOT = "Assets/_Project/Prefabs/";
        private const string SCENES_ROOT = "Assets/_Project/Scenes/";

        [MenuItem("NeonDrift/Build Everything")]
        public static void BuildAll()
        {
            Debug.Log("[NeonDrift] Beginning Full Automated Project Assembly (Portrait 9:16)...");

            EnsureFolders();
            CreateScriptableObjects();
            CreatePrefabs();
            BuildPlayableScene();
            BuildHandlingSandbox();
            BuildDriftTuningGround();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NeonDrift] === Assembly Complete! Portrait 9:16 MVP, Handling Sandbox, and Drift Tuning Ground Ready. ===");
        }

        [MenuItem("NeonDrift/Build Endless Scene (NeonDistrictLoop)")]
        public static void BuildEndlessSceneMenu()
        {
            EnsureFolders();
            BuildPlayableScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NeonDrift] === Endless Scene (NeonDistrictLoop) Rebuilt Successfully! ===");
        }

        [MenuItem("NeonDrift/Build Handling Sandbox")]
        public static void BuildHandlingSandboxMenu()
        {
            EnsureFolders();
            CreateScriptableObjects();
            CreatePrefabs();
            BuildHandlingSandbox();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NeonDrift] === Handling Sandbox Rebuilt Successfully! ===");
        }

        public static void BuildDriftTuningGroundMenu()
        {
            EnsureFolders();
            CreateScriptableObjects();
            CreatePrefabs();
            DriftTuningGroundBuilder.BuildDriftTuningGround();
            Debug.Log("[NeonDrift] === Drift Tuning Ground Rebuilt Successfully! ===");
        }

        private static void EnsureFolders()
        {
            if (!Directory.Exists(DATA_ROOT)) Directory.CreateDirectory(DATA_ROOT);
            if (!Directory.Exists(PREFABS_ROOT)) Directory.CreateDirectory(PREFABS_ROOT);
            if (!Directory.Exists(SCENES_ROOT)) Directory.CreateDirectory(SCENES_ROOT);
            AssetDatabase.Refresh();
        }

        private static void CreateScriptableObjects()
        {
            // CarConfig
            string carPath = DATA_ROOT + "CarConfig.asset";
            CarConfig carConfig = AssetDatabase.LoadAssetAtPath<CarConfig>(carPath);
            if (carConfig == null)
            {
                carConfig = ScriptableObject.CreateInstance<CarConfig>();
                AssetDatabase.CreateAsset(carConfig, carPath);
            }

            // Update tuning to new arcade parameters
            carConfig.baseTopSpeed = 19f;
            carConfig.baseAcceleration = 15f;
            carConfig.turnRate = 130f;
            carConfig.lowSpeedSteerMultiplier = 1.25f;
            carConfig.highSpeedSteerMultiplier = 0.85f;
            carConfig.minDriftSpeed = 6.0f;
            carConfig.driftBaseTurnRate = 58f;
            carConfig.driftMinTurnRate = 28f;
            carConfig.driftMaxTurnRate = 100f;
            carConfig.driftCorneringRate = 72f;
            carConfig.baseDriftSlipAngle = 28f;
            carConfig.maxDriftSlipAngle = 48f;
            carConfig.minDriftSlipAngle = 14f;
            carConfig.slipAngleRate = 95f;
            carConfig.driftSpeedRetention = 0.95f;
            carConfig.hardDriftSpeedRetention = 0.88f;
            carConfig.driftExitDuration = 0.20f;
            carConfig.transitionFlickThreshold = 16f;
            carConfig.perfectExitMinAngle = 18f;
            carConfig.perfectExitMaxAngle = 46f;
            carConfig.perfectExitImpulse = 3.5f;
            carConfig.driftChainWindow = 1.2f;

            // FlowConfig
            string flowPath = DATA_ROOT + "FlowConfig.asset";
            FlowConfig flowConfig = AssetDatabase.LoadAssetAtPath<FlowConfig>(flowPath);
            if (flowConfig == null)
            {
                flowConfig = ScriptableObject.CreateInstance<FlowConfig>();
                AssetDatabase.CreateAsset(flowConfig, flowPath);
            }

            // UpgradeConfig
            string upPath = DATA_ROOT + "UpgradeConfig.asset";
            UpgradeConfig upConfig = AssetDatabase.LoadAssetAtPath<UpgradeConfig>(upPath);
            if (upConfig == null)
            {
                upConfig = ScriptableObject.CreateInstance<UpgradeConfig>();
                AssetDatabase.CreateAsset(upConfig, upPath);
            }

            EditorUtility.SetDirty(carConfig);
            EditorUtility.SetDirty(flowConfig);
            EditorUtility.SetDirty(upConfig);
        }

        private static void CreatePrefabs()
        {
            Sprite kitsuneSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Vehicles/kitsune_s13.png");
            Sprite smokeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "VFX/vfx_drift_smoke.png");
            Sprite skidSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "VFX/vfx_skidmark.png");
            Sprite streakSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "VFX/vfx_speed_streak.png");
            Sprite beamSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "VFX/vfx_headlight_beam.png");

            CarConfig carConfig = AssetDatabase.LoadAssetAtPath<CarConfig>(DATA_ROOT + "CarConfig.asset");

            // Build Player Prefab (Rescaled to 0.72 for optimal road/vehicle ratio)
            GameObject playerGO = new GameObject("KitsuneS13");
            playerGO.tag = "Player";
            playerGO.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            
            SpriteRenderer sr = playerGO.AddComponent<SpriteRenderer>();
            sr.sprite = kitsuneSprite;
            sr.sortingOrder = 0;

            BoxCollider2D col = playerGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.1f, 4.0f); // Effective physical size ~1.51m x 2.88m

            Rigidbody2D rb = playerGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.4f;
            rb.angularDamping = 4f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            VehicleController vc = playerGO.AddComponent<VehicleController>();
            vc.Initialize(carConfig);

            VehicleVFX vfx = playerGO.AddComponent<VehicleVFX>();
            vfx.Initialize(vc, smokeSprite, skidSprite, streakSprite, beamSprite);

            playerGO.AddComponent<CollisionHandler>();

            string playerPrefabPath = PREFABS_ROOT + "Vehicles/KitsuneS13.prefab";
            if (!Directory.Exists(PREFABS_ROOT + "Vehicles")) Directory.CreateDirectory(PREFABS_ROOT + "Vehicles");
            PrefabUtility.SaveAsPrefabAsset(playerGO, playerPrefabPath);
            Object.DestroyImmediate(playerGO);

            // Traffic Prefabs (Rescaled to 0.72)
            CreateTrafficPrefab("Compact", ART_ROOT + "Traffic/traffic_compact.png", new Vector2(1.9f, 3.4f), TrafficCar.CarType.Compact);
            CreateTrafficPrefab("Sedan", ART_ROOT + "Traffic/traffic_sedan.png", new Vector2(2.1f, 4.0f), TrafficCar.CarType.Sedan);
            CreateTrafficPrefab("Van", ART_ROOT + "Traffic/traffic_van.png", new Vector2(2.4f, 5.0f), TrafficCar.CarType.Van);
        }

        private static void CreateTrafficPrefab(string name, string spritePath, Vector2 colliderSize, TrafficCar.CarType carType)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            GameObject go = new GameObject($"Traffic_{name}");
            go.tag = "Traffic";
            go.transform.localScale = new Vector3(0.72f, 0.72f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = colliderSize;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            TrafficCar tc = go.AddComponent<TrafficCar>();
            tc.ResetCar(carType, 13f, 0);

            string folder = PREFABS_ROOT + "Traffic/";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            PrefabUtility.SaveAsPrefabAsset(go, folder + $"Traffic_{name}.prefab");
            Object.DestroyImmediate(go);
        }

        private static void BuildPlayableScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Core Systems & Managers
            GameObject managersGO = new GameObject("_GameManagers");
            GameManager gm = managersGO.AddComponent<GameManager>();
            ProgressionManager pm = managersGO.AddComponent<ProgressionManager>();
            AudioManager am = managersGO.AddComponent<AudioManager>();
            GameFeelManager gfm = managersGO.AddComponent<GameFeelManager>();

            // Wire audio clips
            am.CustomBoostClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/sfx_boost.wav");
            am.CustomCrashClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/sfx_crash.wav");
            am.CustomHeavyCrashClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/sfx_slam_big.wav");
            am.CustomUiClickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/sfx_ui_click.wav");
            am.CustomGameOverClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/sfx_game_over.wav");
            am.CustomNearMissClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/sfx_near_miss.wav");
            am.CustomMusicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/music_gameplay_synthwave.ogg");

            UpgradeConfig upConfig = AssetDatabase.LoadAssetAtPath<UpgradeConfig>(DATA_ROOT + "UpgradeConfig.asset");
            CarConfig carConfig = AssetDatabase.LoadAssetAtPath<CarConfig>(DATA_ROOT + "CarConfig.asset");
            FlowConfig flowConfig = AssetDatabase.LoadAssetAtPath<FlowConfig>(DATA_ROOT + "FlowConfig.asset");
            CarCatalog carCatalog = AssetDatabase.LoadAssetAtPath<CarCatalog>(DATA_ROOT + "CarCatalog.asset");
            CustomizationCatalog customCatalog = AssetDatabase.LoadAssetAtPath<CustomizationCatalog>(DATA_ROOT + "CustomizationCatalog.asset");

            SerializedObject soPM = new SerializedObject(pm);
            soPM.FindProperty("upgradeConfig").objectReferenceValue = upConfig;
            soPM.FindProperty("carCatalog").objectReferenceValue = carCatalog;
            soPM.FindProperty("customCatalog").objectReferenceValue = customCatalog;
            soPM.ApplyModifiedProperties();

            // 2. Portrait Camera
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.backgroundColor = new Color(0.045f, 0.050f, 0.065f, 1f); // Proving ground uniform dark tarmac
            cam.clearFlags = CameraClearFlags.SolidColor;

            camGO.AddComponent<AudioListener>();
            CameraController camCtrl = camGO.AddComponent<CameraController>();
            camCtrl.framingMode = CameraController.FramingMode.FixedForwardY;
            camCtrl.forwardFramingOffset = 10.0f;
            camCtrl.lateralAnticipation = 0.20f;
            camCtrl.baseOrthoSize = 17.0f;
            camCtrl.boostOrthoSize = 19.5f;

            // 3. World Parent
            GameObject worldParent = new GameObject("World");

            // 4. Player Vehicle
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_ROOT + "Vehicles/KitsuneS13.prefab");
            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, worldParent.transform);
            playerInstance.transform.position = new Vector3(0f, 6f, 0f);
            playerInstance.transform.rotation = Quaternion.identity;
            VehicleController vc = playerInstance.GetComponent<VehicleController>();
            playerInstance.AddComponent<NeonDrift.Diagnostics.VehicleStopDiagnostic>();

            camCtrl.SetTarget(vc);

            // Track Manager (Endless Procedural Generator)
            GameObject trackGO = new GameObject("TrackManager");
            trackGO.transform.SetParent(worldParent.transform);
            TrackManager tm = trackGO.AddComponent<TrackManager>();

            Material roadMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/M_RoadRibbon.mat");
            Sprite roadSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Road/road_straight_4lane.png");
            Sprite finishSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Road/road_finish_line.png");
            Sprite towerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Buildings/building_neon_tower.png");
            Sprite shopSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Buildings/building_shop.png");
            Sprite lampSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Props/prop_street_lamp.png");
            Sprite vmSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Props/prop_vending_machine.png");

            SerializedObject soTM = new SerializedObject(tm);
            soTM.FindProperty("playerVehicle").objectReferenceValue = vc;
            soTM.FindProperty("roadMaterial").objectReferenceValue = roadMat;
            soTM.FindProperty("roadSprite").objectReferenceValue = roadSprite;
            soTM.FindProperty("finishSprite").objectReferenceValue = finishSprite;
            soTM.FindProperty("buildingTowerSprite").objectReferenceValue = towerSprite;
            soTM.FindProperty("buildingShopSprite").objectReferenceValue = shopSprite;
            soTM.FindProperty("lampSprite").objectReferenceValue = lampSprite;
            soTM.FindProperty("vendingSprite").objectReferenceValue = vmSprite;
            soTM.FindProperty("activeSeed").intValue = 424242;
            soTM.ApplyModifiedProperties();

            // 5. Gameplay Logic
            GameObject gameplayGO = new GameObject("GameplayLogic");
            gameplayGO.transform.SetParent(worldParent.transform);
            FlowManager fm = gameplayGO.AddComponent<FlowManager>();
            ScoreManager sm = gameplayGO.AddComponent<ScoreManager>();
            NearMissDetector nmd = gameplayGO.AddComponent<NearMissDetector>();

            fm.Initialize(flowConfig, vc);
            sm.Initialize(fm, vc);
            nmd.Initialize(fm, vc);

            // Traffic Manager
            GameObject trafficGO = new GameObject("TrafficManager");
            trafficGO.transform.SetParent(worldParent.transform);
            TrafficManager trfMgr = trafficGO.AddComponent<TrafficManager>();

            Sprite compactSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Traffic/traffic_compact.png");
            Sprite sedanSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Traffic/traffic_sedan.png");
            Sprite sportsSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Traffic/traffic_sports.png");
            Sprite taxiSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Traffic/traffic_taxi.png");
            Sprite vanSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Traffic/traffic_van.png");
            Sprite pickupSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Traffic/traffic_pickup.png");
            Sprite busSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Traffic/traffic_bus.png");

            trfMgr.Initialize(vc, tm, compactSprite, sedanSprite, sportsSprite, taxiSprite, vanSprite, pickupSprite, busSprite);

            SerializedObject soTraffic = new SerializedObject(trfMgr);
            soTraffic.FindProperty("playerVehicle").objectReferenceValue = vc;
            soTraffic.FindProperty("trackManager").objectReferenceValue = tm;
            soTraffic.FindProperty("compactSprite").objectReferenceValue = compactSprite;
            soTraffic.FindProperty("sedanSprite").objectReferenceValue = sedanSprite;
            soTraffic.FindProperty("sportsSprite").objectReferenceValue = sportsSprite;
            soTraffic.FindProperty("taxiSprite").objectReferenceValue = taxiSprite;
            soTraffic.FindProperty("vanSprite").objectReferenceValue = vanSprite;
            soTraffic.FindProperty("pickupSprite").objectReferenceValue = pickupSprite;
            soTraffic.FindProperty("busSprite").objectReferenceValue = busSprite;
            soTraffic.ApplyModifiedProperties();

            // 6. Home Environment Scene (Portrait)
            GameObject homeEnv = new GameObject("HomeEnvironment");
            Sprite homeBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Backgrounds/bg_neon_district_skyline_portrait.png");
            if (homeBgSprite == null) homeBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Backgrounds/bg_neon_district_skyline.png");
            
            GameObject homeBgGO = new GameObject("HomeBackground");
            homeBgGO.transform.SetParent(homeEnv.transform);
            homeBgGO.transform.position = new Vector3(0f, 0f, 10f);
            homeBgGO.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
            SpriteRenderer homeBgSR = homeBgGO.AddComponent<SpriteRenderer>();
            homeBgSR.sprite = homeBgSprite;
            homeBgSR.sortingOrder = -50;

            // Parked Kitsune in foreground
            GameObject parkedCar = new GameObject("ParkedKitsune");
            parkedCar.transform.SetParent(homeEnv.transform);
            parkedCar.transform.position = new Vector3(0f, -0.6f, 0f);
            parkedCar.transform.rotation = Quaternion.Euler(0f, 0f, 15f);
            parkedCar.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            SpriteRenderer pCarSR = parkedCar.AddComponent<SpriteRenderer>();
            pCarSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Vehicles/kitsune_s13.png");
            pCarSR.sortingOrder = 1;

            // 7. Garage Environment Scene (Portrait)
            GameObject garageEnv = new GameObject("GarageEnvironment");
            Sprite garageBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Backgrounds/bg_garage_interior_portrait.png");
            if (garageBgSprite == null) garageBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Backgrounds/bg_garage_interior.png");

            GameObject garageBgGO = new GameObject("GarageBackground");
            garageBgGO.transform.SetParent(garageEnv.transform);
            garageBgGO.transform.position = new Vector3(0f, 0f, 10f);
            garageBgGO.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
            SpriteRenderer garageBgSR = garageBgGO.AddComponent<SpriteRenderer>();
            garageBgSR.sprite = garageBgSprite;
            garageBgSR.sortingOrder = -50;

            // Kitsune on garage lift
            GameObject liftCar = new GameObject("ShowcaseKitsune");
            liftCar.transform.SetParent(garageEnv.transform);
            liftCar.transform.position = new Vector3(0f, 3.2f, 0f);
            liftCar.transform.rotation = Quaternion.identity;
            liftCar.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            SpriteRenderer liftCarSR = liftCar.AddComponent<SpriteRenderer>();
            liftCarSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Vehicles/kitsune_s13.png");
            liftCarSR.sortingOrder = 1;

            // 8. Canvas & UI (Mobile Portrait 1080 x 1920 reference)
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f; // Match width for responsive vertical scaling
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();

            // Build UI Views
            HomeView homeView = CreateHomeUI(canvasGO.transform);
            GarageView garageView = CreateGarageUI(canvasGO.transform, upConfig);
            HUDView hudView = CreateHUDUI(canvasGO.transform, vc, fm, sm);
            ResultsView resultsView = CreateResultsUI(canvasGO.transform);
            TutorialView tutView = CreateTutorialUI(canvasGO.transform);

            // Connect GameManager
            SerializedObject soGM = new SerializedObject(gm);
            soGM.FindProperty("carConfig").objectReferenceValue = carConfig;
            soGM.FindProperty("flowConfig").objectReferenceValue = flowConfig;
            soGM.FindProperty("upgradeConfig").objectReferenceValue = upConfig;

            soGM.FindProperty("playerVehicle").objectReferenceValue = vc;
            soGM.FindProperty("flowManager").objectReferenceValue = fm;
            soGM.FindProperty("scoreManager").objectReferenceValue = sm;
            soGM.FindProperty("trackManager").objectReferenceValue = tm;
            soGM.FindProperty("trafficManager").objectReferenceValue = trfMgr;
            soGM.FindProperty("cameraController").objectReferenceValue = camCtrl;
            soGM.FindProperty("gameFeelManager").objectReferenceValue = gfm;

            soGM.FindProperty("homeView").objectReferenceValue = homeView;
            soGM.FindProperty("garageView").objectReferenceValue = garageView;
            soGM.FindProperty("hudView").objectReferenceValue = hudView;
            soGM.FindProperty("resultsView").objectReferenceValue = resultsView;

            soGM.FindProperty("homeEnvironment").objectReferenceValue = homeEnv;
            soGM.FindProperty("garageEnvironment").objectReferenceValue = garageEnv;
            soGM.FindProperty("gameplayWorld").objectReferenceValue = worldParent;

            soGM.ApplyModifiedProperties();

            // Save Scene
            string scenePath = SCENES_ROOT + "NeonDistrictLoop.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            // Add to Build Settings
            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;
        }

        private static HomeView CreateHomeUI(Transform parent)
        {
            GameObject root = CreateUIPanel("HomeView", parent);
            HomeView view = root.AddComponent<HomeView>();

            // Logo & Subtitle
            Text title = CreateUIText("Title", root.transform, "NEON DRIFT.", 88, new Color(0f, 0.9f, 1f), new Vector2(0, 640), new Vector2(900, 140));
            title.fontStyle = FontStyle.Bold;

            Text sub = CreateUIText("Subtitle", root.transform, "— MIDNIGHT DRIFT METROPOLIS —", 26, new Color(1f, 0f, 0.5f), new Vector2(0, 560), new Vector2(800, 50));

            // Profile info
            Text best = CreateUIText("BestScore", root.transform, "BEST: 0", 32, Color.white, new Vector2(0, 460), new Vector2(500, 50));
            Text cr = CreateUIText("Credits", root.transform, "1,000 NC", 32, new Color(1f, 0.75f, 0f), new Vector2(0, 410), new Vector2(500, 50));

            // Buttons (Clean vertical stack in bottom third)
            Button btnDrive = CreateUIButton("BtnDrive", root.transform, "DRIVE", new Color(0f, 0.8f, 0.95f), new Vector2(0, -380), new Vector2(460, 95));
            Button btnGarage = CreateUIButton("BtnGarage", root.transform, "GARAGE", new Color(0.85f, 0.1f, 0.5f), new Vector2(0, -495), new Vector2(460, 85));
            Button btnSettings = CreateUIButton("BtnSettings", root.transform, "SETTINGS", new Color(0.3f, 0.4f, 0.55f), new Vector2(0, -600), new Vector2(460, 75));

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("driveButton").objectReferenceValue = btnDrive;
            so.FindProperty("garageButton").objectReferenceValue = btnGarage;
            so.FindProperty("settingsButton").objectReferenceValue = btnSettings;
            so.FindProperty("bestScoreText").objectReferenceValue = best;
            so.FindProperty("creditsText").objectReferenceValue = cr;
            so.ApplyModifiedProperties();

            return view;
        }

        private static GarageView CreateGarageUI(Transform parent, UpgradeConfig config)
        {
            GameObject root = CreateUIPanel("GarageView", parent);
            GarageView view = root.AddComponent<GarageView>();

            Text title = CreateUIText("GarageTitle", root.transform, "GARAGE & SHOWROOM", 52, new Color(1f, 0.05f, 0.5f), new Vector2(0, 800), new Vector2(800, 70));
            title.fontStyle = FontStyle.Bold;

            Text crText = CreateUIText("CreditsText", root.transform, "1,000 NC", 36, new Color(1f, 0.75f, 0f), new Vector2(0, 735), new Vector2(600, 50));
            crText.fontStyle = FontStyle.Bold;

            // Vehicle Showroom Card (Center Y = +380)
            GameObject carCard = CreateUIPanel("CarCard", root.transform);
            RectTransform cardRT = carCard.GetComponent<RectTransform>();
            cardRT.anchoredPosition = new Vector2(0, 390);
            cardRT.sizeDelta = new Vector2(860, 480);
            Image cardBg = carCard.GetComponent<Image>();
            cardBg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

            Text cName = CreateUIText("CarName", carCard.transform, "KITSUNE S13", 42, new Color(0f, 0.9f, 1f), new Vector2(0, 180), new Vector2(700, 60));
            cName.fontStyle = FontStyle.Bold;

            Text cArch = CreateUIText("CarArch", carCard.transform, "BALANCED STREET DRIFTER", 24, new Color(0.7f, 0.8f, 0.95f), new Vector2(0, 130), new Vector2(700, 40));

            // Car Preview Sprite Image
            GameObject imgGO = new GameObject("CarPreview");
            imgGO.transform.SetParent(carCard.transform, false);
            RectTransform imgRT = imgGO.AddComponent<RectTransform>();
            imgRT.anchoredPosition = new Vector2(0, 0);
            imgRT.sizeDelta = new Vector2(160, 240);
            Image carImg = imgGO.AddComponent<Image>();
            carImg.preserveAspect = true;

            Button prevBtn = CreateUIButton("PrevBtn", carCard.transform, "<", new Color(0.3f, 0.4f, 0.5f), new Vector2(-360, 0), new Vector2(80, 120));
            Button nextBtn = CreateUIButton("NextBtn", carCard.transform, ">", new Color(0.3f, 0.4f, 0.5f), new Vector2(360, 0), new Vector2(80, 120));

            Button buyEquipBtn = CreateUIButton("BuyEquipBtn", carCard.transform, "EQUIPPED", new Color(0f, 0.8f, 0.95f), new Vector2(0, -170), new Vector2(480, 75));
            Text buyEquipText = buyEquipBtn.GetComponentInChildren<Text>();

            // Upgrades Panel (6 Categories, Y = -180)
            GameObject upPanel = CreateUIPanel("UpgradesPanel", root.transform);
            RectTransform upRT = upPanel.GetComponent<RectTransform>();
            upRT.anchoredPosition = new Vector2(0, -180);
            upRT.sizeDelta = new Vector2(860, 560);
            Image upBg = upPanel.GetComponent<Image>();
            upBg.color = new Color(0.05f, 0.06f, 0.10f, 0.90f);

            CreateUIText("UpTitle", upPanel.transform, "PERFORMANCE UPGRADES (5 TIERS)", 26, Color.white, new Vector2(0, 240), new Vector2(700, 40));

            var (eBtn, eLvl, eCost) = CreateUpgradeRow("EngineRow", upPanel.transform, "ENGINE (TOP SPEED)", 170);
            var (tbBtn, tbLvl, tbCost) = CreateUpgradeRow("TurboRow", upPanel.transform, "TURBO (BOOST POWER)", 100);
            var (stBtn, stLvl, stCost) = CreateUpgradeRow("SteerRow", upPanel.transform, "STEERING (AGILITY)", 30);
            var (drBtn, drLvl, drCost) = CreateUpgradeRow("DriftRow", upPanel.transform, "DRIFT (CONTROL & STABILITY)", -40);
            var (brBtn, brLvl, brCost) = CreateUpgradeRow("BrakesRow", upPanel.transform, "BRAKES (TRACTION RECOVERY)", -110);
            var (duBtn, duLvl, duCost) = CreateUpgradeRow("DurabilityRow", upPanel.transform, "DURABILITY (ARMOR HITS)", -180);

            // Rewarded Ad Bonus Button (Earn Free NC)
            Button adBonusBtn = CreateUIButton("AdBonusBtn", root.transform, "WATCH AD — +250 NC", new Color(1f, 0.65f, 0.1f), new Vector2(0, -560), new Vector2(460, 75));

            Button backBtn = CreateUIButton("BackBtn", root.transform, "< BACK", new Color(0.4f, 0.5f, 0.65f), new Vector2(-240, -780), new Vector2(260, 80));
            Button driveBtn = CreateUIButton("DriveBtn", root.transform, "DRIVE >", new Color(0f, 0.85f, 1f), new Vector2(240, -780), new Vector2(320, 90));

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("creditsText").objectReferenceValue = crText;
            so.FindProperty("carNameText").objectReferenceValue = cName;
            so.FindProperty("carArchetypeText").objectReferenceValue = cArch;
            so.FindProperty("carPreviewImage").objectReferenceValue = carImg;
            so.FindProperty("prevCarBtn").objectReferenceValue = prevBtn;
            so.FindProperty("nextCarBtn").objectReferenceValue = nextBtn;
            so.FindProperty("buyOrEquipBtn").objectReferenceValue = buyEquipBtn;
            so.FindProperty("buyOrEquipBtnText").objectReferenceValue = buyEquipText;

            so.FindProperty("upgradeEngineBtn").objectReferenceValue = eBtn;
            so.FindProperty("engineLvlText").objectReferenceValue = eLvl;
            so.FindProperty("engineCostText").objectReferenceValue = eCost;

            so.FindProperty("upgradeTurboBtn").objectReferenceValue = tbBtn;
            so.FindProperty("turboLvlText").objectReferenceValue = tbLvl;
            so.FindProperty("turboCostText").objectReferenceValue = tbCost;

            so.FindProperty("upgradeSteerBtn").objectReferenceValue = stBtn;
            so.FindProperty("steerLvlText").objectReferenceValue = stLvl;
            so.FindProperty("steerCostText").objectReferenceValue = stCost;

            so.FindProperty("upgradeDriftBtn").objectReferenceValue = drBtn;
            so.FindProperty("driftLvlText").objectReferenceValue = drLvl;
            so.FindProperty("driftCostText").objectReferenceValue = drCost;

            so.FindProperty("upgradeBrakesBtn").objectReferenceValue = brBtn;
            so.FindProperty("brakesLvlText").objectReferenceValue = brLvl;
            so.FindProperty("brakesCostText").objectReferenceValue = brCost;

            so.FindProperty("upgradeDurabilityBtn").objectReferenceValue = duBtn;
            so.FindProperty("durabilityLvlText").objectReferenceValue = duLvl;
            so.FindProperty("durabilityCostText").objectReferenceValue = duCost;

            so.FindProperty("watchAdBonusBtn").objectReferenceValue = adBonusBtn;
            so.FindProperty("backBtn").objectReferenceValue = backBtn;
            so.FindProperty("driveBtn").objectReferenceValue = driveBtn;
            so.ApplyModifiedProperties();

            return view;
        }

        private static HUDView CreateHUDUI(Transform parent, VehicleController vc, FlowManager fm, ScoreManager sm)
        {
            GameObject root = CreateUIPanel("HUDView", parent);
            HUDView view = root.AddComponent<HUDView>();

            // Score Top Center (Y = +840)
            Text scoreText = CreateUIText("ScoreText", root.transform, "0", 62, Color.white, new Vector2(0, 840), new Vector2(600, 80));
            scoreText.fontStyle = FontStyle.Bold;

            // Flow Bar Top Center (Y = +770)
            Sprite barBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_bar_bg.png");
            Sprite barFillCyan = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_bar_fill_cyan.png");
            Sprite barFillMag = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_bar_fill_magenta.png");

            GameObject flowBarObj = new GameObject("FlowGauge");
            flowBarObj.transform.SetParent(root.transform);
            RectTransform flowRT = flowBarObj.AddComponent<RectTransform>();
            flowRT.anchoredPosition = new Vector2(40, 770);
            flowRT.sizeDelta = new Vector2(380, 22);

            Image flowBg = flowBarObj.AddComponent<Image>();
            flowBg.sprite = barBgSprite;
            flowBg.type = Image.Type.Sliced;

            GameObject flowFillObj = new GameObject("FlowFill");
            flowFillObj.transform.SetParent(flowBarObj.transform);
            RectTransform fillRT = flowFillObj.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            Image flowFill = flowFillObj.AddComponent<Image>();
            flowFill.sprite = barFillMag;
            flowFill.type = Image.Type.Filled;
            flowFill.fillMethod = Image.FillMethod.Horizontal;
            flowFill.fillAmount = 0.5f;

            // Multiplier Badge (X = -230, Y = +770)
            Text multText = CreateUIText("MultiplierBadge", root.transform, "x1", 46, new Color(0f, 0.9f, 1f), new Vector2(-230, 770), new Vector2(130, 60));
            multText.fontStyle = FontStyle.Bold;

            // Boost Meter (X = 40, Y = +725)
            GameObject boostBarObj = new GameObject("BoostGauge");
            boostBarObj.transform.SetParent(root.transform);
            RectTransform boostRT = boostBarObj.AddComponent<RectTransform>();
            boostRT.anchoredPosition = new Vector2(40, 725);
            boostRT.sizeDelta = new Vector2(320, 14);

            Image boostBg = boostBarObj.AddComponent<Image>();
            boostBg.sprite = barBgSprite;
            boostBg.type = Image.Type.Sliced;

            GameObject boostFillObj = new GameObject("BoostFill");
            boostFillObj.transform.SetParent(boostBarObj.transform);
            RectTransform bfillRT = boostFillObj.AddComponent<RectTransform>();
            bfillRT.anchorMin = Vector2.zero;
            bfillRT.anchorMax = Vector2.one;
            bfillRT.sizeDelta = Vector2.zero;
            Image boostFill = boostFillObj.AddComponent<Image>();
            boostFill.sprite = barFillCyan;
            boostFill.type = Image.Type.Filled;
            boostFill.fillMethod = Image.FillMethod.Horizontal;
            boostFill.fillAmount = 0.0f;

            // Contextual Action Popup (Y = +520)
            Text popup = CreateUIText("ActionPopup", root.transform, "DRIFT\n+450", 46, new Color(1f, 0.85f, 0.1f), new Vector2(0, 520), new Vector2(600, 110));
            popup.fontStyle = FontStyle.Bold;

            // Touch Steering Buttons (Bottom Left, Y = -740)
            Sprite btnLeftSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_left.png");
            Sprite btnRightSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_right.png");
            Sprite btnDriftSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_drift.png");
            Sprite btnBoostSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_boost.png");

            Vector2 btnSize = new Vector2(210, 210);
            var leftBtn = CreateTouchButton("TouchLeft", root.transform, btnLeftSp, new Vector2(-360, -740), btnSize, "LEFT");
            var rightBtn = CreateTouchButton("TouchRight", root.transform, btnRightSp, new Vector2(-140, -740), btnSize, "RIGHT");

            // Touch Drift & Boost Buttons (Bottom Right, Y = -740)
            var driftBtn = CreateTouchButton("TouchDrift", root.transform, btnDriftSp, new Vector2(140, -740), btnSize, "DRIFT");
            var boostBtn = CreateTouchButton("TouchBoost", root.transform, btnBoostSp, new Vector2(360, -740), btnSize, "BOOST");

            // Connect HUD
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("vehicle").objectReferenceValue = vc;
            so.FindProperty("flowManager").objectReferenceValue = fm;
            so.FindProperty("scoreManager").objectReferenceValue = sm;

            so.FindProperty("scoreText").objectReferenceValue = scoreText;
            so.FindProperty("multiplierText").objectReferenceValue = multText;
            so.FindProperty("actionPopupText").objectReferenceValue = popup;
            so.FindProperty("flowFillImage").objectReferenceValue = flowFill;
            so.FindProperty("boostFillImage").objectReferenceValue = boostFill;

            so.FindProperty("leftButton").objectReferenceValue = leftBtn;
            so.FindProperty("rightButton").objectReferenceValue = rightBtn;
            so.FindProperty("driftButton").objectReferenceValue = driftBtn;
            so.FindProperty("boostButton").objectReferenceValue = boostBtn;

            so.ApplyModifiedProperties();
            return view;
        }

        private static ResultsView CreateResultsUI(Transform parent)
        {
            GameObject root = CreateUIPanel("ResultsView", parent);
            ResultsView view = root.AddComponent<ResultsView>();

            // Modal Box in Center Portrait
            GameObject box = CreateUIPanel("ResultsBox", root.transform);
            RectTransform brt = box.GetComponent<RectTransform>();
            brt.sizeDelta = new Vector2(860, 1260);
            Image bImg = box.GetComponent<Image>();
            bImg.color = new Color(0.04f, 0.05f, 0.09f, 0.95f);

            Text title = CreateUIText("Title", box.transform, "RUN COMPLETE", 58, new Color(0f, 0.9f, 1f), new Vector2(0, 480), new Vector2(700, 80));
            title.fontStyle = FontStyle.Bold;

            Text finalSc = CreateUIText("FinalScore", box.transform, "SCORE: 0", 44, Color.white, new Vector2(0, 360), new Vector2(650, 60));
            Text bestSc = CreateUIText("BestScore", box.transform, "BEST: 0", 30, new Color(0.7f, 0.8f, 0.95f), new Vector2(0, 280), new Vector2(650, 50));
            Text longestDr = CreateUIText("LongestDrift", box.transform, "LONGEST DRIFT: 0.0s", 28, Color.white, new Vector2(0, 190), new Vector2(650, 45));
            Text maxCb = CreateUIText("MaxCombo", box.transform, "MAX COMBO: x0", 28, Color.white, new Vector2(0, 120), new Vector2(650, 45));
            Text nearMs = CreateUIText("NearMisses", box.transform, "NEAR MISSES: 0", 28, Color.white, new Vector2(0, 50), new Vector2(650, 45));
            Text crEarned = CreateUIText("CreditsEarned", box.transform, "+0 CR", 46, new Color(1f, 0.75f, 0f), new Vector2(0, -50), new Vector2(650, 60));
            crEarned.fontStyle = FontStyle.Bold;

            Button retryBtn = CreateUIButton("RetryBtn", box.transform, "RETRY RUN", new Color(0f, 0.85f, 1f), new Vector2(0, -180), new Vector2(460, 85));
            Button doubleNCBtn = CreateUIButton("DoubleNCBtn", box.transform, "2X REWARD (WATCH AD)", new Color(1f, 0.7f, 0.1f), new Vector2(0, -275), new Vector2(460, 75));
            Button shareBtn = CreateUIButton("ShareBtn", box.transform, "SHARE STORY CARD", new Color(0.9f, 0.1f, 0.5f), new Vector2(0, -360), new Vector2(460, 70));
            Button garageBtn = CreateUIButton("GarageBtn", box.transform, "GARAGE", new Color(0.2f, 0.8f, 0.5f), new Vector2(-160, -450), new Vector2(220, 75));
            Button homeBtn = CreateUIButton("HomeBtn", box.transform, "MAIN MENU", new Color(0.3f, 0.4f, 0.55f), new Vector2(160, -450), new Vector2(220, 75));

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("finalScoreText").objectReferenceValue = finalSc;
            so.FindProperty("bestScoreText").objectReferenceValue = bestSc;
            so.FindProperty("longestDriftText").objectReferenceValue = longestDr;
            so.FindProperty("maxComboText").objectReferenceValue = maxCb;
            so.FindProperty("nearMissesText").objectReferenceValue = nearMs;
            so.FindProperty("creditsEarnedText").objectReferenceValue = crEarned;
            so.FindProperty("retryButton").objectReferenceValue = retryBtn;
            so.FindProperty("garageButton").objectReferenceValue = garageBtn;
            so.FindProperty("homeButton").objectReferenceValue = homeBtn;
            so.FindProperty("doubleCreditsButton").objectReferenceValue = doubleNCBtn;
            so.FindProperty("shareButton").objectReferenceValue = shareBtn;
            so.ApplyModifiedProperties();

            return view;
        }

        private static TutorialView CreateTutorialUI(Transform parent)
        {
            GameObject root = CreateUIPanel("TutorialView", parent);
            TutorialView view = root.AddComponent<TutorialView>();

            GameObject card = CreateUIPanel("TutorialCard", root.transform);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.anchoredPosition = new Vector2(0, 150);
            cardRT.sizeDelta = new Vector2(860, 680);
            Image cardBg = card.GetComponent<Image>();
            cardBg.color = new Color(0.04f, 0.05f, 0.09f, 0.95f);

            Text title = CreateUIText("TutorialTitle", card.transform, "PHASE 1: STEERING", 44, new Color(0f, 0.9f, 1f), new Vector2(0, 240), new Vector2(740, 60));
            title.fontStyle = FontStyle.Bold;

            Text instruction = CreateUIText("Instruction", card.transform, "Tap LEFT or RIGHT to navigate through highway lanes.", 32, Color.white, new Vector2(0, 100), new Vector2(740, 120));

            Text hint = CreateUIText("Hint", card.transform, "Keep smooth rhythm between curbs to avoid speed penalties.", 26, new Color(1f, 0.8f, 0.2f), new Vector2(0, -50), new Vector2(740, 100));

            Button nextBtn = CreateUIButton("NextBtn", card.transform, "NEXT PHASE >", new Color(0f, 0.85f, 1f), new Vector2(0, -180), new Vector2(440, 80));
            Button skipBtn = CreateUIButton("SkipBtn", card.transform, "SKIP TUTORIAL", new Color(0.4f, 0.5f, 0.6f), new Vector2(0, -270), new Vector2(320, 60));

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("instructionText").objectReferenceValue = instruction;
            so.FindProperty("hintText").objectReferenceValue = hint;
            so.FindProperty("nextButton").objectReferenceValue = nextBtn;
            so.FindProperty("skipButton").objectReferenceValue = skipBtn;
            so.ApplyModifiedProperties();

            root.SetActive(false);
            return view;
        }

        // UI Creation Helpers
        private static GameObject CreateUIPanel(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            Image img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            return go;
        }

        private static Text CreateUIText(string name, Transform parent, string text, int fontSize, Color color, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Text txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return txt;
        }

        private static Button CreateUIButton(string name, Transform parent, string label, Color bgColor, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = bgColor;

            Button btn = go.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = bgColor * 1.2f;
            cb.pressedColor = bgColor * 0.8f;
            btn.colors = cb;

            CreateUIText("Label", go.transform, label, Mathf.RoundToInt(size.y * 0.42f), Color.white, Vector2.zero, size);
            return btn;
        }

        private static Image CreateUIBar(string name, Transform parent, string label, Color fillColor, float yPos)
        {
            CreateUIText(label + "_Label", parent, label, 22, new Color(0.7f, 0.8f, 0.9f), new Vector2(-220, yPos), new Vector2(160, 30));

            GameObject barBg = new GameObject(name + "_Bg");
            barBg.transform.SetParent(parent, false);
            RectTransform bgRT = barBg.AddComponent<RectTransform>();
            bgRT.anchoredPosition = new Vector2(80, yPos);
            bgRT.sizeDelta = new Vector2(380, 24);

            Image imgBg = barBg.AddComponent<Image>();
            imgBg.color = new Color(0.1f, 0.12f, 0.18f);

            GameObject barFill = new GameObject(name + "_Fill");
            barFill.transform.SetParent(barBg.transform, false);
            RectTransform fillRT = barFill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;

            Image fill = barFill.AddComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0.5f;

            return fill;
        }

        private static (Button, Text, Text) CreateUpgradeRow(string name, Transform parent, string title, float yPos)
        {
            GameObject row = new GameObject(name);
            row.transform.SetParent(parent, false);
            RectTransform rrt = row.AddComponent<RectTransform>();
            rrt.anchoredPosition = new Vector2(0, yPos);
            rrt.sizeDelta = new Vector2(800, 75);

            Text lvlText = CreateUIText("LvlText", row.transform, title + " LV.0", 22, Color.white, new Vector2(-150, 0), new Vector2(420, 50));
            lvlText.alignment = TextAnchor.MiddleLeft;

            GameObject btnGO = new GameObject("UpgradeBtn");
            btnGO.transform.SetParent(row.transform, false);
            RectTransform brt = btnGO.AddComponent<RectTransform>();
            brt.anchoredPosition = new Vector2(240, 0);
            brt.sizeDelta = new Vector2(200, 55);

            Image bImg = btnGO.AddComponent<Image>();
            bImg.color = new Color(0f, 0.75f, 0.9f);

            Button btn = btnGO.AddComponent<Button>();
            Text costText = CreateUIText("CostText", btnGO.transform, "CR 400", 22, Color.white, Vector2.zero, new Vector2(200, 55));

            return (btn, lvlText, costText);
        }

        private static MobileTouchButton CreateTouchButton(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size, string tag = "Button")
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = true;

            var mtb = go.AddComponent<MobileTouchButton>();
            mtb.buttonTag = tag;
            return mtb;
        }

        private static Button CreateClickButton(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;

            Button btn = go.AddComponent<Button>();
            return btn;
        }

        public static void BuildHandlingSandbox()
        {
            Debug.Log("[NeonDrift] Building Dedicated Handling Sandbox (HandlingSandbox.unity)...");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CarConfig carConfig = AssetDatabase.LoadAssetAtPath<CarConfig>(DATA_ROOT + "CarConfig.asset");
            Sprite roadSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Road/road_straight_4lane.png");
            Sprite lampSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "Props/prop_street_lamp.png");

            // 1. Portrait Camera
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 15.0f;
            cam.backgroundColor = new Color(0.024f, 0.031f, 0.059f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.AddComponent<AudioListener>();
            CameraController camCtrl = camGO.AddComponent<CameraController>();
            camCtrl.framingMode = CameraController.FramingMode.Omnidirectional;
            camCtrl.baseOrthoSize = 15.0f;
            camCtrl.boostOrthoSize = 17.0f;
            camCtrl.velocityLeadTime = 0.20f;
            camCtrl.maxLeadDistance = 5.0f;

            // 2. World & Sandbox Track (Dedicated Multi-Curve Drift Circuit)
            GameObject worldParent = new GameObject("SandboxWorld");

            GameObject trackGO = new GameObject("SandboxTrack");
            trackGO.transform.SetParent(worldParent.transform);

            Vector2 pos = Vector2.zero;
            float heading = 0f;
            float segmentLength = 4f;

            // 10 Curated Handling & Drift Curve Typologies:
            // 1. Suave larga der.
            // 2. Suave larga izq.
            // 3. Hairpin der.
            // 4. Hairpin izq.
            // 5. S-curve (R->L)
            // 6. S-curve (L->R)
            // 7. Sweeper largo
            // 8. Drift Chain (curvas consecutivas)
            // 9. Recta corta de aceleración
            // 10. Final técnico (chicanes enlazadas)
            var testSections = new (float length, float curvature, string label)[]
            {
                (50f, 0f, "Salida Corta"),
                (130f, 28f, "1. Suave Larga Derecha"),
                (30f, 0f, "Transición"),
                (130f, -28f, "2. Suave Larga Izquierda"),
                (25f, 0f, "9. Recta Corta"),
                (110f, 55f, "3. Hairpin Derecho"),
                (30f, 0f, "Transición"),
                (110f, -55f, "4. Hairpin Izquierdo"),
                (25f, 0f, "Transición"),
                (65f, 35f, "5. S-Curve R->L (Parte 1 Der)"),
                (65f, -35f, "5. S-Curve R->L (Parte 2 Izq)"),
                (25f, 0f, "Transición"),
                (65f, -35f, "6. S-Curve L->R (Parte 1 Izq)"),
                (65f, 35f, "6. S-Curve L->R (Parte 2 Der)"),
                (25f, 0f, "Transición"),
                (200f, -48f, "7. Sweeper Largo Sostenido"),
                (35f, 0f, "Recuperación"),
                (75f, 40f, "8. Drift Chain 1"),
                (20f, 0f, "Chain Link 1"),
                (75f, -40f, "8. Drift Chain 2"),
                (20f, 0f, "Chain Link 2"),
                (75f, 40f, "8. Drift Chain 3"),
                (40f, 0f, "Recta de Aceleración Boost"),
                (45f, -34f, "10. Final Técnico Chicane 1"),
                (45f, 34f, "10. Final Técnico Chicane 2"),
                (45f, -34f, "10. Final Técnico Chicane 3"),
                (45f, 34f, "10. Final Técnico Chicane 4"),
                (60f, 0f, "Recta Final Meta")
            };

            foreach (var sec in testSections)
            {
                int steps = Mathf.CeilToInt(sec.length / segmentLength);
                float deltaAngle = sec.curvature / steps;
                for (int i = 0; i < steps; i++)
                {
                    heading += deltaAngle;
                    Quaternion rot = Quaternion.Euler(0f, 0f, -heading);
                    Vector2 fwd = rot * Vector2.up;
                    Vector2 right = rot * Vector2.right;

                    // Wide asphalt foundation to eliminate black voids
                    GameObject ground = new GameObject("GroundFoundation");
                    ground.transform.SetParent(trackGO.transform);
                    ground.transform.position = (Vector3)pos;
                    ground.transform.rotation = rot;
                    ground.transform.localScale = new Vector3(3.5f, 1f, 1f);
                    SpriteRenderer gsr = ground.AddComponent<SpriteRenderer>();
                    gsr.sprite = roadSprite;
                    gsr.sortingOrder = -25;

                    // Spawn wide road (2 parallel tiles = 28m width)
                    for (int lane = -1; lane <= 1; lane += 2)
                    {
                        GameObject roadTile = new GameObject("RoadTile");
                        roadTile.transform.SetParent(trackGO.transform);
                        roadTile.transform.position = (Vector3)(pos + right * (lane * 7f));
                        roadTile.transform.rotation = rot;
                        SpriteRenderer sr = roadTile.AddComponent<SpriteRenderer>();
                        sr.sprite = roadSprite;
                        sr.sortingOrder = -10;
                    }

                    // Guardrails to prevent falling off the track
                    GameObject wallL = new GameObject("WallL");
                    wallL.transform.SetParent(trackGO.transform);
                    wallL.transform.position = (Vector3)(pos - right * 14.5f);
                    wallL.transform.rotation = rot;
                    var colL = wallL.AddComponent<BoxCollider2D>();
                    colL.size = new Vector2(1f, segmentLength + 0.1f);

                    GameObject wallR = new GameObject("WallR");
                    wallR.transform.SetParent(trackGO.transform);
                    wallR.transform.position = (Vector3)(pos + right * 14.5f);
                    wallR.transform.rotation = rot;
                    var colR = wallR.AddComponent<BoxCollider2D>();
                    colR.size = new Vector2(1f, segmentLength + 0.1f);

                    // Curbside street lamps
                    if (i % 6 == 0 && lampSprite != null)
                    {
                        GameObject lampL = new GameObject("LampL");
                        lampL.transform.SetParent(trackGO.transform);
                        lampL.transform.position = (Vector3)(pos - right * 15f);
                        lampL.transform.rotation = rot;
                        SpriteRenderer lsr = lampL.AddComponent<SpriteRenderer>();
                        lsr.sprite = lampSprite;
                        lsr.sortingOrder = 2;

                        GameObject lampR = new GameObject("LampR");
                        lampR.transform.SetParent(trackGO.transform);
                        lampR.transform.position = (Vector3)(pos + right * 15f);
                        lampR.transform.rotation = rot;
                        SpriteRenderer rsr = lampR.AddComponent<SpriteRenderer>();
                        rsr.sprite = lampSprite;
                        rsr.sortingOrder = 2;
                    }

                    pos += fwd * segmentLength;
                }
            }

            // 3. Player Vehicle
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_ROOT + "Vehicles/KitsuneS13.prefab");
            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, worldParent.transform);
            playerInstance.transform.position = new Vector3(0f, 4f, 0f);
            playerInstance.transform.rotation = Quaternion.identity;
            VehicleController vc = playerInstance.GetComponent<VehicleController>();
            camCtrl.SetTarget(vc);

            // 4. UI Canvas & Telemetry HUD
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f;
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();

            GameObject hudPanel = CreateUIPanel("SandboxHUD", canvasGO.transform);

            // Header info (Top, Y = +840 to +600)
            Text title = CreateUIText("SandboxTitle", hudPanel.transform, "HANDLING SANDBOX", 52, new Color(0f, 0.9f, 1f), new Vector2(0, 840), new Vector2(800, 70));
            title.fontStyle = FontStyle.Bold;

            Text stateText = CreateUIText("StateText", hudPanel.transform, "STATE: GRIP", 38, new Color(0.2f, 0.9f, 0.4f), new Vector2(0, 775), new Vector2(600, 50));
            stateText.fontStyle = FontStyle.Bold;

            Text speedText = CreateUIText("SpeedText", hudPanel.transform, "0 KM/H", 48, Color.white, new Vector2(0, 715), new Vector2(600, 60));
            speedText.fontStyle = FontStyle.Bold;

            Text angleText = CreateUIText("AngleText", hudPanel.transform, "DRIFT ANGLE: 0.0°", 32, new Color(1f, 0.85f, 0.2f), new Vector2(0, 655), new Vector2(600, 45));

            Text steerText = CreateUIText("SteerText", hudPanel.transform, "STEER: NEUTRAL | DRIFT: OFF | BOOST: READY", 28, new Color(0.7f, 0.85f, 1f), new Vector2(0, 605), new Vector2(900, 40));

            // Reset Button
            Button resetBtn = CreateUIButton("ResetBtn", hudPanel.transform, "RESET ↺", new Color(0.9f, 0.2f, 0.4f), new Vector2(360, 840), new Vector2(220, 70));

            // Touch Controls at bottom (identical portrait positions, 210x210)
            Sprite btnLeftSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_left.png");
            Sprite btnRightSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_right.png");
            Sprite btnDriftSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_drift.png");
            Sprite btnBoostSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_boost.png");

            Vector2 btnSize = new Vector2(210, 210);
            var leftTouchBtn = CreateTouchButton("TouchLeft", hudPanel.transform, btnLeftSp, new Vector2(-360, -740), btnSize, "LEFT");
            var rightTouchBtn = CreateTouchButton("TouchRight", hudPanel.transform, btnRightSp, new Vector2(-140, -740), btnSize, "RIGHT");
            var driftTouchBtn = CreateTouchButton("TouchDrift", hudPanel.transform, btnDriftSp, new Vector2(140, -740), btnSize, "DRIFT");
            var boostTouchBtn = CreateTouchButton("TouchBoost", hudPanel.transform, btnBoostSp, new Vector2(360, -740), btnSize, "BOOST");

            // Manager
            GameObject mgrGO = new GameObject("HandlingSandboxManager");
            HandlingSandboxManager sbm = mgrGO.AddComponent<HandlingSandboxManager>();

            SerializedObject soSBM = new SerializedObject(sbm);
            soSBM.FindProperty("vehicle").objectReferenceValue = vc;
            soSBM.FindProperty("stateText").objectReferenceValue = stateText;
            soSBM.FindProperty("speedText").objectReferenceValue = speedText;
            soSBM.FindProperty("angleText").objectReferenceValue = angleText;
            soSBM.FindProperty("steerInputText").objectReferenceValue = steerText;
            soSBM.FindProperty("leftButton").objectReferenceValue = leftTouchBtn;
            soSBM.FindProperty("rightButton").objectReferenceValue = rightTouchBtn;
            soSBM.FindProperty("driftButton").objectReferenceValue = driftTouchBtn;
            soSBM.FindProperty("boostButton").objectReferenceValue = boostTouchBtn;
            soSBM.FindProperty("resetButton").objectReferenceValue = resetBtn;
            soSBM.ApplyModifiedProperties();

            string scenePath = SCENES_ROOT + "HandlingSandbox.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("[NeonDrift] Handling Sandbox saved to " + scenePath);

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(SCENES_ROOT + "NeonDistrictLoop.unity", true),
                new EditorBuildSettingsScene(scenePath, true),
                new EditorBuildSettingsScene(SCENES_ROOT + "DriftTuningGround.unity", true)
            };
            EditorBuildSettings.scenes = buildScenes;
        }

        public static void BuildDriftTuningGround()
        {
            DriftTuningGroundBuilder.BuildDriftTuningGround();
        }
    }
}

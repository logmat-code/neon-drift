using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NeonDrift.Data;
using NeonDrift.Vehicle;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    /// <summary>
    /// Dedicated Procedural Geometry and Scene Builder for DriftTuningGround.
    /// Builds 10 strictly isolated calibration zones using pure mathematical ribbon meshes.
    /// Completely free of buildings, shops, lamps, traffic, or procedural chunks.
    /// Aesthetic: Pure Dark Asphalt + Lines + Car.
    /// </summary>
    public static class DriftTuningGroundBuilder
    {
        private const string ART_ROOT = "Assets/_Project/Art/";
        private const string TUNING_ART_ROOT = "Assets/_Project/Art/TuningGround/";
        private const string PREFABS_ROOT = "Assets/_Project/Prefabs/";
        private const string SCENES_ROOT = "Assets/_Project/Scenes/";

        [MenuItem("NeonDrift/Build Drift Tuning Ground")]
        public static void BuildDriftTuningGround()
        {
            Debug.Log("[NeonDrift] Building Clean, Isolated Drift Tuning Ground (10 Zones, Pure Ribbon Meshes)...");

            EnsureFolders();

            // 1. Create clean material
            Material roadMat = AssetDatabase.LoadAssetAtPath<Material>(TUNING_ART_ROOT + "M_TuningRoad.mat");
            if (roadMat == null)
            {
                Shader s = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit");
                if (s == null) s = Shader.Find("Sprites/Default");
                roadMat = new Material(s) { name = "M_TuningRoad" };
                AssetDatabase.CreateAsset(roadMat, TUNING_ART_ROOT + "M_TuningRoad.mat");
            }

            // 2. Create clean empty scene
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Camera (Solid uniform dark floor, no void borders)
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 15.0f;
            cam.backgroundColor = new Color(0.045f, 0.050f, 0.065f, 1f); // Proving ground uniform dark tarmac
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.AddComponent<AudioListener>();
            CameraController camCtrl = camGO.AddComponent<CameraController>();
            camCtrl.framingMode = CameraController.FramingMode.Omnidirectional;
            camCtrl.baseOrthoSize = 15.0f;
            camCtrl.boostOrthoSize = 17.0f;
            camCtrl.velocityLeadTime = 0.16f;
            camCtrl.maxLeadDistance = 4.0f;

            // 4. World Root, Continuous Floor & Zones Root
            GameObject worldParent = new GameObject("World");
            GameObject floorGO = new GameObject("TuningGroundFloor");
            floorGO.transform.SetParent(worldParent.transform);
            Mesh floorMesh = CreateFloorMesh(800f, 800f);
            SaveAndAttachFloorMesh(floorGO, floorMesh, roadMat, "Mesh_TuningGroundFloor");

            GameObject zonesRoot = new GameObject("CalibrationZones");
            zonesRoot.transform.SetParent(worldParent.transform);

            // 5. Build the 10 Isolated Zones
            DriftTuningGroundManager.ZoneDefinition[] zoneDefs = new DriftTuningGroundManager.ZoneDefinition[10];

            // ZONE 1: STRAIGHT (180m)
            zoneDefs[0] = BuildZone1_Straight(zonesRoot.transform, roadMat);

            // ZONE 2: BIG CIRCLE (R=35m)
            zoneDefs[1] = BuildZone2_BigCircle(zonesRoot.transform, roadMat);

            // ZONE 3: MEDIUM CIRCLE (R=24m)
            zoneDefs[2] = BuildZone3_MediumCircle(zonesRoot.transform, roadMat);

            // ZONE 4: LONG SWEEPER (180°, R=30m)
            zoneDefs[3] = BuildZone4_LongSweeper(zonesRoot.transform, roadMat);

            // ZONE 5: RIGHT 90° (R=32m)
            zoneDefs[4] = BuildZone5_Right90(zonesRoot.transform, roadMat);

            // ZONE 6: LEFT 90° (R=32m)
            zoneDefs[5] = BuildZone6_Left90(zonesRoot.transform, roadMat);

            // ZONE 7: FIGURE 8 (R=26m)
            zoneDefs[6] = BuildZone7_Figure8(zonesRoot.transform, roadMat);

            // ZONE 8: WIDE S-CURVE (R=28m)
            zoneDefs[7] = BuildZone8_WideS(zonesRoot.transform, roadMat);

            // ZONE 9: CHICANE TRANSITION (R=30m)
            zoneDefs[8] = BuildZone9_Transition(zonesRoot.transform, roadMat);

            // ZONE 10: WIDE HAIRPIN (R=22m)
            zoneDefs[9] = BuildZone10_WideHairpin(zonesRoot.transform, roadMat);

            // Set Zone 0 Active, and all other 9 inactive
            for (int i = 0; i < zoneDefs.Length; i++)
            {
                if (zoneDefs[i].zoneRoot != null)
                {
                    zoneDefs[i].zoneRoot.SetActive(i == 0);
                }
            }

            // 6. Instantiate Player Vehicle
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_ROOT + "Vehicles/KitsuneS13.prefab");
            GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, worldParent.transform);
            playerInstance.transform.position = zoneDefs[0].startPosition;
            playerInstance.transform.rotation = Quaternion.Euler(0f, 0f, -zoneDefs[0].startHeading);
            VehicleController vc = playerInstance.GetComponent<VehicleController>();
            camCtrl.SetTarget(vc);

            // Snap camera immediately
            camCtrl.SnapToTarget();

            // 7. UI Canvas & Telemetry HUD
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1.0f;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0f;
            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();

            GameObject hudPanel = new GameObject("TuningHUD");
            hudPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform hudRT = hudPanel.AddComponent<RectTransform>();
            hudRT.anchorMin = Vector2.zero;
            hudRT.anchorMax = Vector2.one;
            hudRT.sizeDelta = Vector2.zero;

            // Header Telemetry Text Elements
            Text title = CreateUIText("Title", hudPanel.transform, "DRIFT TUNING GROUND — CALIBRATION", 42, new Color(0f, 0.9f, 1f), new Vector2(0, 880), new Vector2(960, 56));
            title.fontStyle = FontStyle.Bold;

            Text zoneName = CreateUIText("ZoneName", hudPanel.transform, "ZONE 1: STRAIGHT (180m)", 28, new Color(1f, 0.85f, 0.2f), new Vector2(0, 815), new Vector2(960, 60));
            zoneName.fontStyle = FontStyle.Bold;

            Text stateText = CreateUIText("StateText", hudPanel.transform, "STATE: GRIP", 36, new Color(0.2f, 0.9f, 0.4f), new Vector2(0, 755), new Vector2(600, 48));
            stateText.fontStyle = FontStyle.Bold;

            Text speedText = CreateUIText("SpeedText", hudPanel.transform, "0 KM/H  (0.0 m/s)", 42, Color.white, new Vector2(0, 705), new Vector2(600, 50));
            speedText.fontStyle = FontStyle.Bold;

            Text slipText = CreateUIText("SlipText", hudPanel.transform, "SLIP ANGLE: 0.0° (NONE)", 30, new Color(1f, 0.2f, 0.6f), new Vector2(0, 655), new Vector2(600, 40));

            Text radiusText = CreateUIText("RadiusText", hudPanel.transform, "TURN RADIUS: STRAIGHT (∞)", 30, new Color(1f, 0.85f, 0.2f), new Vector2(0, 610), new Vector2(600, 40));

            Text yawRateText = CreateUIText("YawRateText", hudPanel.transform, "YAW RATE: 0.0°/s  | TRAJ RATE: 0.0°/s", 25, new Color(0.6f, 0.8f, 1f), new Vector2(0, 565), new Vector2(900, 36));

            Text steerText = CreateUIText("SteerText", hudPanel.transform, "STEER: NEUTRAL | DRIFT: OFF", 25, new Color(0.8f, 0.9f, 1f), new Vector2(0, 525), new Vector2(900, 36));

            Text anglesText = CreateUIText("AnglesText", hudPanel.transform, "BODY: 0.0°  | TRAJECTORY: 0.0°", 24, new Color(0.65f, 0.7f, 0.8f), new Vector2(0, 485), new Vector2(900, 32));

            // Zone switching buttons (PREV / RESET / NEXT)
            Button prevBtn = CreateUIButton("PrevZoneBtn", hudPanel.transform, "◀ PREV", new Color(0.25f, 0.4f, 0.55f), new Vector2(-280, 435), new Vector2(220, 55));
            Button resetBtn = CreateUIButton("ResetBtn", hudPanel.transform, "RESET ↺", new Color(0.85f, 0.2f, 0.35f), new Vector2(0, 435), new Vector2(240, 55));
            Button nextBtn = CreateUIButton("NextZoneBtn", hudPanel.transform, "NEXT ▶", new Color(0.25f, 0.4f, 0.55f), new Vector2(280, 435), new Vector2(220, 55));

            // Touch Controls
            Sprite btnLeftSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_left.png");
            Sprite btnRightSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_right.png");
            Sprite btnDriftSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_drift.png");
            Sprite btnBoostSp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_ROOT + "UI/ui_btn_boost.png");

            Vector2 btnSize = new Vector2(210, 210);
            var leftTouchBtn = CreateTouchButton("TouchLeft", hudPanel.transform, btnLeftSp, new Vector2(-360, -740), btnSize, "LEFT");
            var rightTouchBtn = CreateTouchButton("TouchRight", hudPanel.transform, btnRightSp, new Vector2(-140, -740), btnSize, "RIGHT");
            var driftTouchBtn = CreateTouchButton("TouchDrift", hudPanel.transform, btnDriftSp, new Vector2(140, -740), btnSize, "DRIFT");
            var boostTouchBtn = CreateTouchButton("TouchBoost", hudPanel.transform, btnBoostSp, new Vector2(360, -740), btnSize, "BOOST");

            // 8. DriftTuningGroundManager
            GameObject mgrGO = new GameObject("DriftTuningGroundManager");
            DriftTuningGroundManager tgm = mgrGO.AddComponent<DriftTuningGroundManager>();

            SerializedObject soTGM = new SerializedObject(tgm);
            soTGM.FindProperty("vehicle").objectReferenceValue = vc;
            soTGM.FindProperty("cameraController").objectReferenceValue = camCtrl;
            soTGM.FindProperty("speedText").objectReferenceValue = speedText;
            soTGM.FindProperty("stateText").objectReferenceValue = stateText;
            soTGM.FindProperty("slipAngleText").objectReferenceValue = slipText;
            soTGM.FindProperty("radiusText").objectReferenceValue = radiusText;
            soTGM.FindProperty("yawRateText").objectReferenceValue = yawRateText;
            soTGM.FindProperty("steerInputText").objectReferenceValue = steerText;
            soTGM.FindProperty("anglesText").objectReferenceValue = anglesText;
            soTGM.FindProperty("zoneNameText").objectReferenceValue = zoneName;

            soTGM.FindProperty("leftButton").objectReferenceValue = leftTouchBtn;
            soTGM.FindProperty("rightButton").objectReferenceValue = rightTouchBtn;
            soTGM.FindProperty("driftButton").objectReferenceValue = driftTouchBtn;
            soTGM.FindProperty("boostButton").objectReferenceValue = boostTouchBtn;
            soTGM.FindProperty("resetButton").objectReferenceValue = resetBtn;
            soTGM.FindProperty("nextZoneButton").objectReferenceValue = nextBtn;
            soTGM.FindProperty("prevZoneButton").objectReferenceValue = prevBtn;

            // Assign zones array
            SerializedProperty zonesProp = soTGM.FindProperty("zones");
            zonesProp.arraySize = 10;
            for (int i = 0; i < 10; i++)
            {
                SerializedProperty elem = zonesProp.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("name").stringValue = zoneDefs[i].name;
                elem.FindPropertyRelative("zoneRoot").objectReferenceValue = zoneDefs[i].zoneRoot;
                elem.FindPropertyRelative("startPosition").vector3Value = zoneDefs[i].startPosition;
                elem.FindPropertyRelative("startHeading").floatValue = zoneDefs[i].startHeading;
                elem.FindPropertyRelative("targetRadius").floatValue = zoneDefs[i].targetRadius;
                elem.FindPropertyRelative("description").stringValue = zoneDefs[i].description;
            }

            soTGM.ApplyModifiedProperties();

            // 9. Save Scene
            string scenePath = SCENES_ROOT + "DriftTuningGround.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NeonDrift] === Drift Tuning Ground Built Successfully at " + scenePath + " ===");
        }

        private static void EnsureFolders()
        {
            if (!Directory.Exists(TUNING_ART_ROOT))
            {
                Directory.CreateDirectory(TUNING_ART_ROOT);
                AssetDatabase.Refresh();
            }
        }

        // =========================================================================
        // ZONE BUILDERS (10 DEDICATED, ISOLATED ROAD RIBBONS)
        // =========================================================================

        // =========================================================================
        // MATHEMATICALLY CONTINUOUS PATH BUILDER (C1 TANGENT & NORMAL CONTINUITY)
        // =========================================================================

        private class RibbonPathBuilder
        {
            public readonly List<Vector2> points = new List<Vector2>();
            public readonly List<Vector2> normals = new List<Vector2>();
            public Vector2 currentPos;
            public Vector2 currentTangent;

            public RibbonPathBuilder(Vector2 startPos, Vector2 startTangent)
            {
                currentPos = startPos;
                currentTangent = startTangent.normalized;
                points.Add(currentPos);
                normals.Add(new Vector2(currentTangent.y, -currentTangent.x));
            }

            public void AddStraight(float length, float step = 3.0f)
            {
                int steps = Mathf.Max(1, Mathf.CeilToInt(length / step));
                float dt = length / steps;
                for (int i = 0; i < steps; i++)
                {
                    currentPos += currentTangent * dt;
                    points.Add(currentPos);
                    normals.Add(new Vector2(currentTangent.y, -currentTangent.x));
                }
            }

            public void AddLeftTurn(float angleDeg, float radius, float step = 3.0f)
            {
                // Left turn center: currentPos + (-currentTangent.y, currentTangent.x) * radius
                Vector2 leftNormal = new Vector2(-currentTangent.y, currentTangent.x);
                Vector2 center = currentPos + leftNormal * radius;
                Vector2 rEntry = currentPos - center;
                float alpha0 = Mathf.Atan2(rEntry.y, rEntry.x);
                float arcLen = radius * Mathf.Deg2Rad * angleDeg;
                int steps = Mathf.Max(2, Mathf.CeilToInt(arcLen / step));

                for (int i = 1; i <= steps; i++)
                {
                    float alpha = alpha0 + (angleDeg * Mathf.Deg2Rad) * (i / (float)steps);
                    Vector2 pos = center + new Vector2(Mathf.Cos(alpha), Mathf.Sin(alpha)) * radius;
                    Vector2 tangent = new Vector2(-Mathf.Sin(alpha), Mathf.Cos(alpha));
                    points.Add(pos);
                    normals.Add(new Vector2(tangent.y, -tangent.x));
                }

                float endAlpha = alpha0 + angleDeg * Mathf.Deg2Rad;
                currentPos = points[points.Count - 1];
                currentTangent = new Vector2(-Mathf.Sin(endAlpha), Mathf.Cos(endAlpha));
            }

            public void AddRightTurn(float angleDeg, float radius, float step = 3.0f)
            {
                // Right turn center: currentPos + (currentTangent.y, -currentTangent.x) * radius
                Vector2 rightNormal = new Vector2(currentTangent.y, -currentTangent.x);
                Vector2 center = currentPos + rightNormal * radius;
                Vector2 rEntry = currentPos - center;
                float alpha0 = Mathf.Atan2(rEntry.y, rEntry.x);
                float arcLen = radius * Mathf.Deg2Rad * angleDeg;
                int steps = Mathf.Max(2, Mathf.CeilToInt(arcLen / step));

                for (int i = 1; i <= steps; i++)
                {
                    float alpha = alpha0 - (angleDeg * Mathf.Deg2Rad) * (i / (float)steps);
                    Vector2 pos = center + new Vector2(Mathf.Cos(alpha), Mathf.Sin(alpha)) * radius;
                    Vector2 tangent = new Vector2(Mathf.Sin(alpha), -Mathf.Cos(alpha));
                    points.Add(pos);
                    normals.Add(new Vector2(tangent.y, -tangent.x));
                }

                float endAlpha = alpha0 - angleDeg * Mathf.Deg2Rad;
                currentPos = points[points.Count - 1];
                currentTangent = new Vector2(Mathf.Sin(endAlpha), -Mathf.Cos(endAlpha));
            }
        }

        // =========================================================================
        // ZONE BUILDERS (10 DEDICATED, ISOLATED ROAD RIBBONS)
        // =========================================================================

        private static DriftTuningGroundManager.ZoneDefinition BuildZone1_Straight(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_01_Straight");
            zoneGO.transform.SetParent(parent);

            var pb = new RibbonPathBuilder(new Vector2(0f, -30f), Vector2.up);
            pb.AddStraight(180f);

            Mesh mesh = CreateRoadMesh(pb.points, pb.normals, 22f, false);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone01_Straight");
            AttachColliders(zoneGO, pb.points, pb.normals, 22f, false);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "1. STRAIGHTAWAY (180m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(0f, -20f, 0f),
                startHeading = 0f,
                targetRadius = 999f,
                description = "180m Straight (Width 22m). Acceleration, steering response & drift entry."
            };
        }

        private static DriftTuningGroundManager.ZoneDefinition BuildZone2_BigCircle(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_02_BigCircle");
            zoneGO.transform.SetParent(parent);

            List<Vector2> pts = new List<Vector2>();
            List<Vector2> nrms = new List<Vector2>();

            int samples = 120;
            float radius = 35f;
            for (int i = 0; i < samples; i++)
            {
                float angle = (i * 360f / samples) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                pts.Add(pos);
                nrms.Add(pos.normalized);
            }

            Mesh mesh = CreateRoadMesh(pts, nrms, 22f, true);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone02_BigCircle");
            AttachColliders(zoneGO, pts, nrms, 22f, true);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "2. BIG SKIDPAD CIRCLE (R=35m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(0f, -35f, 0f),
                startHeading = 90f,
                targetRadius = 35f,
                description = "Continuous Skidpad (R=35m, Width 22m). High-speed countersteer balance."
            };
        }

        private static DriftTuningGroundManager.ZoneDefinition BuildZone3_MediumCircle(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_03_MediumCircle");
            zoneGO.transform.SetParent(parent);

            List<Vector2> pts = new List<Vector2>();
            List<Vector2> nrms = new List<Vector2>();

            int samples = 120;
            float radius = 24f;
            for (int i = 0; i < samples; i++)
            {
                float angle = (i * 360f / samples) * Mathf.Deg2Rad;
                Vector2 pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                pts.Add(pos);
                nrms.Add(pos.normalized);
            }

            Mesh mesh = CreateRoadMesh(pts, nrms, 22f, true);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone03_MediumCircle");
            AttachColliders(zoneGO, pts, nrms, 22f, true);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "3. MEDIUM SKIDPAD CIRCLE (R=24m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(0f, -24f, 0f),
                startHeading = 90f,
                targetRadius = 24f,
                description = "Continuous Skidpad (R=24m, Width 22m). Neutral balanced drift sweet spot."
            };
        }

        private static DriftTuningGroundManager.ZoneDefinition BuildZone4_LongSweeper(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_04_LongSweeper");
            zoneGO.transform.SetParent(parent);

            var pb = new RibbonPathBuilder(new Vector2(-35f, -60f), Vector2.up);
            pb.AddStraight(60f);
            pb.AddRightTurn(180f, 35f);
            pb.AddStraight(60f);

            Mesh mesh = CreateRoadMesh(pb.points, pb.normals, 22f, false);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone04_LongSweeper");
            AttachColliders(zoneGO, pb.points, pb.normals, 22f, false);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "4. LONG 180° SWEEPER (R=35m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(-35f, -50f, 0f),
                startHeading = 0f,
                targetRadius = 35f,
                description = "180° Sweeper (R=35m, Width 22m). Entry -> 180° Sustained Arc -> Exit."
            };
        }

        private static DriftTuningGroundManager.ZoneDefinition BuildZone5_Right90(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_05_Right90");
            zoneGO.transform.SetParent(parent);

            var pb = new RibbonPathBuilder(new Vector2(-32f, -60f), Vector2.up);
            pb.AddStraight(60f);
            pb.AddRightTurn(90f, 32f);
            pb.AddStraight(70f);

            Mesh mesh = CreateRoadMesh(pb.points, pb.normals, 22f, false);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone05_Right90");
            AttachColliders(zoneGO, pb.points, pb.normals, 22f, false);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "5. RIGHT 90° SWEEPER (R=32m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(-32f, -50f, 0f),
                startHeading = 0f,
                targetRadius = 32f,
                description = "Right 90° Corner (R=32m, Width 22m). High-speed entry into right drift."
            };
        }

        private static DriftTuningGroundManager.ZoneDefinition BuildZone6_Left90(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_06_Left90");
            zoneGO.transform.SetParent(parent);

            var pb = new RibbonPathBuilder(new Vector2(32f, -60f), Vector2.up);
            pb.AddStraight(60f);
            pb.AddLeftTurn(90f, 32f);
            pb.AddStraight(70f);

            Mesh mesh = CreateRoadMesh(pb.points, pb.normals, 22f, false);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone06_Left90");
            AttachColliders(zoneGO, pb.points, pb.normals, 22f, false);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "6. LEFT 90° SWEEPER (R=32m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(32f, -50f, 0f),
                startHeading = 0f,
                targetRadius = 32f,
                description = "Left 90° Corner (R=32m, Width 22m). High-speed entry into left drift."
            };
        }

        private static DriftTuningGroundManager.ZoneDefinition BuildZone7_Figure8(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_07_Figure8");
            zoneGO.transform.SetParent(parent);

            // Clean, non-intersecting Figure-8 Transfer Circuit:
            // Entry Straight -> Left Drift Lobe (200°, R=28m) -> 70m Diagonal Transfer Straight -> Right Drift Lobe (200°, R=28m) -> Exit Straight
            var pb = new RibbonPathBuilder(new Vector2(0f, -70f), Vector2.up);
            pb.AddStraight(50f);
            pb.AddLeftTurn(200f, 28f);
            pb.AddStraight(70f);
            pb.AddRightTurn(200f, 28f);
            pb.AddStraight(50f);

            Mesh mesh = CreateRoadMesh(pb.points, pb.normals, 22f, false);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone07_Figure8");
            AttachColliders(zoneGO, pb.points, pb.normals, 22f, false);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "7. FIGURE-8 CIRCUIT (R=28m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(0f, -60f, 0f),
                startHeading = 0f,
                targetRadius = 28f,
                description = "Figure-8 Transfer (R=28m, Width 22m). Left Drift -> 70m Flick Transfer -> Right Drift."
            };
        }

        private static DriftTuningGroundManager.ZoneDefinition BuildZone8_WideS(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_08_WideS");
            zoneGO.transform.SetParent(parent);

            var pb = new RibbonPathBuilder(new Vector2(0f, -60f), Vector2.up);
            pb.AddStraight(50f);
            pb.AddLeftTurn(60f, 28f);
            pb.AddStraight(25f);
            pb.AddRightTurn(60f, 28f);
            pb.AddStraight(50f);

            Mesh mesh = CreateRoadMesh(pb.points, pb.normals, 22f, false);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone08_WideS");
            AttachColliders(zoneGO, pb.points, pb.normals, 22f, false);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "8. WIDE S-CURVES (R=28m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(0f, -50f, 0f),
                startHeading = 0f,
                targetRadius = 28f,
                description = "Wide S-Curves (R=28m, Width 22m). Rhythmic weight transfer balance."
            };
        }

        private static DriftTuningGroundManager.ZoneDefinition BuildZone9_Transition(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_09_Transition");
            zoneGO.transform.SetParent(parent);

            var pb = new RibbonPathBuilder(new Vector2(0f, -60f), Vector2.up);
            pb.AddStraight(40f);
            pb.AddLeftTurn(45f, 30f);
            pb.AddStraight(20f);
            pb.AddRightTurn(90f, 30f);
            pb.AddStraight(20f);
            pb.AddLeftTurn(45f, 30f);
            pb.AddStraight(50f);

            Mesh mesh = CreateRoadMesh(pb.points, pb.normals, 22f, false);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone09_Transition");
            AttachColliders(zoneGO, pb.points, pb.normals, 22f, false);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "9. CHICANE FLICK (R=30m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(0f, -50f, 0f),
                startHeading = 0f,
                targetRadius = 30f,
                description = "Transition Chicane (R=30m, Width 22m). Left -> Right -> Left weight flick test."
            };
        }

        private static DriftTuningGroundManager.ZoneDefinition BuildZone10_WideHairpin(Transform parent, Material mat)
        {
            GameObject zoneGO = new GameObject("Zone_10_WideHairpin");
            zoneGO.transform.SetParent(parent);

            var pb = new RibbonPathBuilder(new Vector2(-28f, -70f), Vector2.up);
            pb.AddStraight(70f);
            pb.AddRightTurn(180f, 28f);
            pb.AddStraight(70f);

            Mesh mesh = CreateRoadMesh(pb.points, pb.normals, 22f, false);
            SaveAndAttachMesh(zoneGO, mesh, mat, "Mesh_Zone10_WideHairpin");
            AttachColliders(zoneGO, pb.points, pb.normals, 22f, false);

            return new DriftTuningGroundManager.ZoneDefinition
            {
                name = "10. WIDE HAIRPIN (R=28m)",
                zoneRoot = zoneGO,
                startPosition = new Vector3(-28f, -60f, 0f),
                startHeading = 0f,
                targetRadius = 28f,
                description = "Wide Hairpin (R=28m, Width 22m). Long Entry -> 180° Arc -> Long Exit."
            };
        }

        private static Mesh CreateFloorMesh(float width, float height)
        {
            Mesh mesh = new Mesh { name = "Mesh_TuningGroundFloor" };
            float hw = width * 0.5f;
            float hh = height * 0.5f;

            Vector3[] verts = new Vector3[]
            {
                new Vector3(-hw, -hh, 0f),
                new Vector3( hw, -hh, 0f),
                new Vector3( hw,  hh, 0f),
                new Vector3(-hw,  hh, 0f)
            };

            Color floorColor = new Color(0.045f, 0.050f, 0.065f, 1f); // Proving ground uniform dark tarmac
            Color[] colors = new Color[] { floorColor, floorColor, floorColor, floorColor };
            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            int[] tris = new int[] { 0, 1, 2, 0, 2, 3 };

            mesh.vertices = verts;
            mesh.colors = colors;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void SaveAndAttachFloorMesh(GameObject go, Mesh mesh, Material mat, string assetName)
        {
            string assetPath = TUNING_ART_ROOT + assetName + ".asset";
            AssetDatabase.CreateAsset(mesh, assetPath);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.sortingLayerName = "Default";
            mr.sortingOrder = -25; // Sits below road ribbons (-10) and skid marks (-5)
        }

        // =========================================================================
        // MESH GENERATION & ATTACHMENT
        // =========================================================================

        private static Mesh CreateRoadMesh(List<Vector2> points, List<Vector2> rightNormals, float roadWidth, bool isClosedLoop)
        {
            Mesh mesh = new Mesh();
            int count = points.Count;
            if (count < 2) return mesh;

            int vertCount = count * 10;
            Vector3[] vertices = new Vector3[vertCount];
            Color[] colors = new Color[vertCount];
            Vector2[] uvs = new Vector2[vertCount];

            Color whiteCurb = new Color(0.95f, 0.95f, 0.98f, 1f);
            Color darkAsphalt = new Color(0.12f, 0.13f, 0.16f, 1f);
            Color yellowCenter = new Color(0.95f, 0.80f, 0.15f, 1f);

            float halfW = roadWidth * 0.5f;        // 11.0f
            float curbInnerW = halfW - 0.6f;        // 10.4f
            float centerHalfW = 0.25f;             // 0.25f

            float[] offsets = new float[]
            {
                -halfW,       // 0: Left Curb Outer
                -curbInnerW,  // 1: Left Curb Inner
                -curbInnerW,  // 2: Left Asphalt Outer
                -centerHalfW, // 3: Left Asphalt Inner
                -centerHalfW, // 4: Centerline Left
                +centerHalfW, // 5: Centerline Right
                +centerHalfW, // 6: Right Asphalt Inner
                +curbInnerW,  // 7: Right Asphalt Outer
                +curbInnerW,  // 8: Right Curb Inner
                +halfW        // 9: Right Curb Outer
            };

            Color[] sliceColors = new Color[]
            {
                whiteCurb, whiteCurb,
                darkAsphalt, darkAsphalt,
                yellowCenter, yellowCenter,
                darkAsphalt, darkAsphalt,
                whiteCurb, whiteCurb
            };

            for (int i = 0; i < count; i++)
            {
                Vector2 p = points[i];
                Vector2 n = rightNormals[i];

                for (int v = 0; v < 10; v++)
                {
                    int idx = i * 10 + v;
                    Vector2 vertPos = p + n * offsets[v];
                    vertices[idx] = new Vector3(vertPos.x, vertPos.y, 0f);
                    colors[idx] = sliceColors[v];
                    uvs[idx] = new Vector2(v / 9f, (float)i / count);
                }
            }

            int segCount = isClosedLoop ? count : count - 1;
            int triCount = segCount * 5 * 6;
            int[] triangles = new int[triCount];
            int triIdx = 0;

            for (int i = 0; i < segCount; i++)
            {
                int next = (i + 1) % count;
                for (int q = 0; q < 5; q++)
                {
                    int c0 = i * 10 + q * 2;
                    int c1 = i * 10 + q * 2 + 1;
                    int n0 = next * 10 + q * 2;
                    int n1 = next * 10 + q * 2 + 1;

                    triangles[triIdx++] = c0;
                    triangles[triIdx++] = n0;
                    triangles[triIdx++] = n1;

                    triangles[triIdx++] = c0;
                    triangles[triIdx++] = n1;
                    triangles[triIdx++] = c1;
                }
            }

            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void SaveAndAttachMesh(GameObject go, Mesh mesh, Material mat, string assetName)
        {
            string assetPath = TUNING_ART_ROOT + assetName + ".asset";
            AssetDatabase.CreateAsset(mesh, assetPath);

            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.sortingLayerName = "Default";
            mr.sortingOrder = -10;
        }

        private static void AttachColliders(GameObject go, List<Vector2> points, List<Vector2> rightNormals, float roadWidth, bool isClosedLoop)
        {
            int count = points.Count;
            float halfW = roadWidth * 0.5f;

            // Left Wall
            GameObject wallL = new GameObject("LeftWall");
            wallL.transform.SetParent(go.transform);
            wallL.tag = "Wall";
            var edgeL = wallL.AddComponent<EdgeCollider2D>();
            Vector2[] ptsL = new Vector2[isClosedLoop ? count + 1 : count];
            for (int i = 0; i < count; i++) ptsL[i] = points[i] - rightNormals[i] * halfW;
            if (isClosedLoop) ptsL[count] = ptsL[0];
            edgeL.points = ptsL;

            // Right Wall
            GameObject wallR = new GameObject("RightWall");
            wallR.transform.SetParent(go.transform);
            wallR.tag = "Wall";
            var edgeR = wallR.AddComponent<EdgeCollider2D>();
            Vector2[] ptsR = new Vector2[isClosedLoop ? count + 1 : count];
            for (int i = 0; i < count; i++) ptsR[i] = points[i] + rightNormals[i] * halfW;
            if (isClosedLoop) ptsR[count] = ptsR[0];
            edgeR.points = ptsR;
        }

        // =========================================================================
        // UI HELPERS
        // =========================================================================

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

        private static MobileTouchButton CreateTouchButton(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size, string fallbackLabel)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.2f, 0.3f, 0.45f, 0.85f);
                CreateUIText("Label", go.transform, fallbackLabel, 34, Color.white, Vector2.zero, size);
            }
            img.raycastTarget = true;

            MobileTouchButton mtb = go.AddComponent<MobileTouchButton>();
            return mtb;
        }
    }
}

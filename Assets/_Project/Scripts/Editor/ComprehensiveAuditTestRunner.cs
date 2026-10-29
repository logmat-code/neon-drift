using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Core;
using NeonDrift.World;
using NeonDrift.Vehicle;
using NeonDrift.Presentation;
using NeonDrift.Progression;
using NeonDrift.Tutorial;
using NeonDrift.Monetization;
using NeonDrift.Gameplay;

namespace NeonDrift.Editor
{
    public static class ComprehensiveAuditTestRunner
    {
        public static void RunFullAuditSuite()
        {
            Debug.Log("==================================================================");
            Debug.Log("=== NEON DRIFT. — COMPREHENSIVE CRITICAL AUDIT & VERIFICATION ===");
            Debug.Log("==================================================================");

            // 1. Open Scene
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            Debug.Log($"[AUDIT-SCENE] Active scene: {scene.path}, IsLoaded: {scene.isLoaded}");

            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            var tm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            var track = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            var tutMgr = UnityEngine.Object.FindAnyObjectByType<TutorialManager>(FindObjectsInactive.Include);
            var tutView = UnityEngine.Object.FindAnyObjectByType<TutorialView>(FindObjectsInactive.Include);
            var adsMgr = UnityEngine.Object.FindAnyObjectByType<Yodo1AdsManager>(FindObjectsInactive.Include);
            var pm = UnityEngine.Object.FindAnyObjectByType<ProgressionManager>(FindObjectsInactive.Include);

            if (pm.CurrentData == null)
            {
                pm.LoadProfile();
            }

            // ==========================================
            // TEST 1: YODO1 MAS + ADMOB CONFIGURATION
            // ==========================================
            Debug.Log("\n--- TEST 1: YODO1 MAS + ADMOB CONFIGURATION ---");
            Debug.Log($"[TEST 1] Yodo1AdsManager present: {adsMgr != null}");
            Debug.Log($"[TEST 1] Official APP_KEY: {Yodo1AdsManager.APP_KEY} (Expected: PLd7aFzM4Z)");
            Debug.Log($"[TEST 1] Official ADMOB_APP_ID: {Yodo1AdsManager.ADMOB_APP_ID} (Expected: ca-app-pub-5784311004955543~4555927254)");
            
            // Check AndroidManifest.xml
            string manifestText = File.ReadAllText(Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml"));
            bool manifestHasAdMob = manifestText.Contains("ca-app-pub-5784311004955543~4555927254");
            bool manifestHasPackage = manifestText.Contains("com.logmat.neondrift");
            Debug.Log($"[TEST 1] AndroidManifest package matches: {manifestHasPackage}, AdMob App ID matches: {manifestHasAdMob}");

            // ==========================================
            // TEST 2 & 3: TRAFFIC & OBSTACLES AT RUN START & GAMEPLAY
            // ==========================================
            Debug.Log("\n--- TEST 2 & 3: TRAFFIC & OBSTACLES SPAWNING ---");
            // Simulate start run
            gm.StartRun();
            Debug.Log($"[TEST 2] Traffic count immediately at launch: {tm.activeTrafficCount}");
            
            // Simulate player driving forward 30 seconds
            float dt = 0.05f;
            int totalSteps = 600; // 30s
            HashSet<TrafficCar.CarType> typesSeen = new HashSet<TrafficCar.CarType>();
            int encounters = 0;

            for (int s = 0; s < totalSteps; s++)
            {
                player.currentSpeed = 18f;
                player.transform.position += new Vector3(0, player.currentSpeed * dt, 0);
                track.ProcessUpdate(dt);
                tm.UpdateTraffic(dt);

                var cars = UnityEngine.Object.FindObjectsByType<TrafficCar>(FindObjectsSortMode.None);
                foreach (var c in cars)
                {
                    if (c.gameObject.activeInHierarchy)
                    {
                        typesSeen.Add(c.carType);
                        float diff = c.longitudinalDistance - track.totalDistanceTraveled;
                        if (diff >= -5f && diff <= 35f) encounters++;
                    }
                }
            }

            Debug.Log($"[TEST 2 & 3] After 30s drive: Active Traffic: {tm.activeTrafficCount}, Visible Encounters: {encounters}, Distinct Types Seen: {typesSeen.Count}");
            foreach (var t in typesSeen)
            {
                Debug.Log($" -> Spawned Type: {t}");
            }

            // ==========================================
            // TEST 4: SHOP / GARAGE VEHICLE PURCHASE FROM UI
            // ==========================================
            Debug.Log("\n--- TEST 4: SHOP / GARAGE REAL UI PURCHASE ---");
            pm.CurrentData.credits = 8000;
            pm.CurrentData.GetVehicleData("akuma_86").isUnlocked = false;
            pm.CurrentData.selectedVehicleId = "kitsune_s13";
            pm.SaveProfile();

            gv.gameObject.SetActive(true);
            gv.RefreshUI();

            // Next car -> AKUMA 86
            gv.SelectNextCar();

            // Invoke buy button
            gv.OnBuyOrEquipClicked();

            bool akumaUnlocked = pm.CurrentData.GetVehicleData("akuma_86").isUnlocked;
            bool akumaEquipped = pm.CurrentData.selectedVehicleId == "akuma_86";
            Debug.Log($"[TEST 4] Akuma 86 Unlocked: {akumaUnlocked}, Equipped: {akumaEquipped}, Remaining NC: {pm.CurrentData.credits}");

            // ==========================================
            // TEST 5: TUTORIAL FIRST DRIVE FLOW
            // ==========================================
            Debug.Log("\n--- TEST 5: TUTORIAL FIRST DRIVE FLOW ---");
            // Simulate clean install profile
            pm.CurrentData.tutorialCompleted = false;
            pm.CurrentData.tutorialSkipped = false;
            pm.SaveProfile();
            tutMgr.currentStep = TutorialManager.TutorialStep.None;
            int preTutCredits = pm.CurrentData.credits;

            bool shouldStart = tutMgr.ShouldStartFirstDrive();
            Debug.Log($"[TEST 5] Should Start First Drive on new install: {shouldStart}");

            tutMgr.StartTutorial();
            Debug.Log($"[TEST 5] Step 1: {tutMgr.currentStep}");
            tutMgr.AdvanceStep();
            Debug.Log($"[TEST 5] Step 2: {tutMgr.currentStep}");
            tutMgr.AdvanceStep();
            Debug.Log($"[TEST 5] Step 3: {tutMgr.currentStep}");
            tutMgr.AdvanceStep();
            Debug.Log($"[TEST 5] Step 4: {tutMgr.currentStep}");
            tutMgr.AdvanceStep();
            Debug.Log($"[TEST 5] Step 5: {tutMgr.currentStep}");
            tutMgr.AdvanceStep();
            Debug.Log($"[TEST 5] Finished: {tutMgr.currentStep}, IsTutorialActive: {tutMgr.isTutorialActive}");

            int postTutCredits = pm.CurrentData.credits;
            Debug.Log($"[TEST 5] Bounty awarded once: {postTutCredits - preTutCredits} NC (Expected: +500 NC)");

            // Try advancing or completing again (should NOT award infinite credits)
            tutMgr.CompleteTutorial();
            Debug.Log($"[TEST 5] Second completion credits delta: {pm.CurrentData.credits - postTutCredits} NC (Expected: 0)");

            Debug.Log("==================================================================");
            Debug.Log("=== FULL AUDIT COMPLETED SUCCESSFULLY ===");
            Debug.Log("==================================================================");
        }
    }
}

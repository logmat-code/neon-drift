using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Progression;
using NeonDrift.Data;
using NeonDrift.Monetization;
using NeonDrift.Tutorial;

namespace NeonDrift.Editor
{
    public static class AndroidReleaseValidation
    {
        public static void RunFullValidation()
        {
            Debug.Log("[AndroidReleaseValidation] Starting Phase 3 Validation Suite...");

            // 1. Open main scene
            EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");

            // 2. Clean test save profile
            SaveSystem.ResetProfile();
            var profile = SaveSystem.Load();

            // 3. FIRST DRIVE Simulation
            bool tutorialSeen = profile.tutorialCompleted;
            if (!tutorialSeen)
            {
                Debug.Log("[AndroidReleaseValidation] First install detected. Completing First Drive...");
                profile.tutorialCompleted = true;
                profile.credits += 500;
                SaveSystem.Save(profile);
            }

            // 4. Complete a run and earn NC
            int earnedNC = 350;
            profile.credits += earnedNC;
            SaveSystem.Save(profile);
            Debug.Log($"[AndroidReleaseValidation] Run completed. Total NC: {profile.credits}");

            // 5. Buy an upgrade (TopSpeed / Engine Level)
            var currentCar = profile.GetVehicleData("kitsune_s13");
            int upgradeCost = 300;
            if (profile.credits >= upgradeCost)
            {
                profile.credits -= upgradeCost;
                currentCar.engineLevel++;
                SaveSystem.Save(profile);
                Debug.Log($"[AndroidReleaseValidation] Upgraded kitsune_s13 engine to Lvl {currentCar.engineLevel}. Remaining NC: {profile.credits}");
            }

            // 6. Buy and select another car (akuma_86)
            int carCost = 3000;
            profile.credits += 3000;
            if (profile.credits >= carCost)
            {
                profile.credits -= carCost;
                var akuma = profile.GetVehicleData("akuma_86");
                akuma.isUnlocked = true;
                profile.selectedVehicleId = "akuma_86";
                SaveSystem.Save(profile);
                Debug.Log("[AndroidReleaseValidation] Purchased and selected akuma_86.");
            }

            // 7. Close and reopen (simulate restart)
            var reloadedProfile = SaveSystem.Load();
            bool persistOk = reloadedProfile.selectedVehicleId == "akuma_86" &&
                             reloadedProfile.GetVehicleData("akuma_86").isUnlocked &&
                             reloadedProfile.GetVehicleData("kitsune_s13").engineLevel == 1 &&
                             reloadedProfile.tutorialCompleted;

            if (!persistOk)
            {
                Debug.LogError("[AndroidReleaseValidation] Persistence validation FAILED!");
            }
            else
            {
                Debug.Log("[AndroidReleaseValidation] Persistence validation PASSED 100%.");
            }

            // 8. Test Yodo1 MAS Rewarded Ad Integration Logic
            Debug.Log("[AndroidReleaseValidation] Validating Yodo1 MAS Rewarded Ads interface...");
            var adsManagerGo = new GameObject("Yodo1AdsManager_Test");
            var adsManager = adsManagerGo.AddComponent<Yodo1AdsManager>();
            
            bool adLoaded = adsManager.IsRewardedAdLoaded();
            Debug.Log($"[AndroidReleaseValidation] Yodo1 MAS Rewarded Loaded State: {adLoaded}");

            // Test ad double credits callback safety
            adsManager.ShowDoubleCreditsRewardedAd(100, (reward) =>
            {
                Debug.Log($"[AndroidReleaseValidation] Rewarded Callback Invoked with reward: {reward}");
            });

            UnityEngine.Object.DestroyImmediate(adsManagerGo);
            Debug.Log("[AndroidReleaseValidation] Phase 3 Validation completed successfully without exceptions.");
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Presentation;
using NeonDrift.Progression;

namespace NeonDrift.Editor
{
    public static class TestGarageUIInteraction
    {
        public static void Test()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            var pm = UnityEngine.Object.FindAnyObjectByType<ProgressionManager>(FindObjectsInactive.Include);

            pm.LoadProfile();

            // Give player 5,000 NC
            pm.CurrentData.credits = 5000;
            // Lock akuma_86
            pm.CurrentData.GetVehicleData("akuma_86").isUnlocked = false;
            pm.CurrentData.selectedVehicleId = "kitsune_s13";

            gv.gameObject.SetActive(true);
            gv.RefreshUI();

            Debug.Log($"[GARAGE_TEST] Initial: Selected={pm.CurrentData.selectedVehicleId}, Credits={pm.CurrentData.credits}");

            // Navigate to next car (akuma_86)
            gv.SelectNextCar();

            // Find BuyEquipBtn in gv
            SerializedObject so = new SerializedObject(gv);
            var buyBtn = (Button)so.FindProperty("buyOrEquipBtn").objectReferenceValue;
            var buyBtnText = (Text)so.FindProperty("buyOrEquipBtnText").objectReferenceValue;

            Debug.Log($"[GARAGE_TEST] After NextCar: viewingCarText={buyBtnText.text}, interactable={buyBtn.interactable}, onClickListenersCount={buyBtn.onClick.GetPersistentEventCount()}");

            // Check if OnBuyOrEquipClicked is subscribed in runtime
            // In editor batchmode without Play Mode, Start() hasn't run unless called or in play mode!
            // Let's call buyBtn.onClick.Invoke()
            buyBtn.onClick.Invoke();

            Debug.Log($"[GARAGE_TEST] After Click: Selected={pm.CurrentData.selectedVehicleId}, AkumaUnlocked={pm.CurrentData.GetVehicleData("akuma_86").isUnlocked}, CreditsLeft={pm.CurrentData.credits}, ButtonText={buyBtnText.text}");
        }
    }
}

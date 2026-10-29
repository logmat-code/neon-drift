using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Progression;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class TestDebugGarageClick
    {
        public static void Check()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var pm = UnityEngine.Object.FindAnyObjectByType<ProgressionManager>(FindObjectsInactive.Include);
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);

            Debug.Log($"ProgressionManager.Instance in scene: {ProgressionManager.Instance != null}");
            if (pm.CurrentData == null) pm.LoadProfile();

            pm.CurrentData.credits = 10000;
            pm.CurrentData.GetVehicleData("akuma_86").isUnlocked = false;
            pm.SaveProfile();

            gv.gameObject.SetActive(true);
            gv.RefreshUI();

            Debug.Log($"Before SelectNextCar: viewingCarIndex={typeof(GarageView).GetField("viewingCarIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(gv)}");

            gv.SelectNextCar();

            Debug.Log($"After SelectNextCar: viewingCarIndex={typeof(GarageView).GetField("viewingCarIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(gv)}");
            
            // Now click buy
            gv.OnBuyOrEquipClicked();

            Debug.Log($"After OnBuyOrEquipClicked: akuma_86 isUnlocked={pm.CurrentData.GetVehicleData("akuma_86").isUnlocked}, credits={pm.CurrentData.credits}");
        }
    }
}

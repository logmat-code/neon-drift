using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Progression;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class TestSingleBuyStep
    {
        public static void Check()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var pm = UnityEngine.Object.FindAnyObjectByType<ProgressionManager>(FindObjectsInactive.Include);
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);

            pm.LoadProfile();
            pm.CurrentData.credits = 8000;
            Debug.Log($"Initial Credits: {pm.CurrentData.credits}, CarCatalog: {pm.CarCatalog != null}");

            var carDef = pm.CarCatalog?.GetCar("akuma_86");
            Debug.Log($"CarDef akuma_86: {carDef?.displayName}, price={carDef?.priceNC}");

            var vData = pm.CurrentData.GetVehicleData("akuma_86");
            Debug.Log($"vData akuma_86: isUnlocked={vData.isUnlocked}");

            bool bought = pm.TryBuyCar("akuma_86");
            Debug.Log($"Result of TryBuyCar('akuma_86'): {bought}, newCredits: {pm.CurrentData.credits}, isUnlocked: {vData.isUnlocked}");
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class CheckBuyButtonDimensions
    {
        public static void Check()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            
            SerializedObject so = new SerializedObject(gv);
            var buyBtn = (Button)so.FindProperty("buyOrEquipBtn").objectReferenceValue;
            var carCard = gv.transform.Find("CarCard");
            var upgradesPanel = gv.transform.Find("UpgradesPanel");

            Debug.Log($"CarCard rect: {(carCard as RectTransform).rect}, anchoredPosition: {(carCard as RectTransform).anchoredPosition}");
            Debug.Log($"BuyEquipBtn rect: {(buyBtn.transform as RectTransform).rect}, anchoredPosition: {(buyBtn.transform as RectTransform).anchoredPosition}");
            Debug.Log($"UpgradesPanel rect: {(upgradesPanel as RectTransform).rect}, anchoredPosition: {(upgradesPanel as RectTransform).anchoredPosition}");
        }
    }
}

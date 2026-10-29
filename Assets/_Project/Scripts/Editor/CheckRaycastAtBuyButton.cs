using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class CheckRaycastAtBuyButton
    {
        public static void Check()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            gv.gameObject.SetActive(true);

            var carCard = gv.transform.Find("CarCard");
            var buyBtn = carCard.Find("BuyEquipBtn");

            RectTransform cardRT = carCard.GetComponent<RectTransform>();
            RectTransform buyRT = buyBtn.GetComponent<RectTransform>();
            RectTransform upRT = gv.transform.Find("UpgradesPanel").GetComponent<RectTransform>();

            Debug.Log($"CarCard - anchorMin: {cardRT.anchorMin}, anchorMax: {cardRT.anchorMax}, offsetMin: {cardRT.offsetMin}, offsetMax: {cardRT.offsetMax}, rect: {cardRT.rect}");
            Debug.Log($"UpgradesPanel - anchorMin: {upRT.anchorMin}, anchorMax: {upRT.anchorMax}, offsetMin: {upRT.offsetMin}, offsetMax: {upRT.offsetMax}, rect: {upRT.rect}");
            Debug.Log($"BuyBtn - anchorMin: {buyRT.anchorMin}, anchorMax: {buyRT.anchorMax}, anchoredPos: {buyRT.anchoredPosition}, rect: {buyRT.rect}");

            // UpgradesPanel Image raycastTarget:
            var upImg = gv.transform.Find("UpgradesPanel").GetComponent<Image>();
            Debug.Log($"UpgradesPanel Image raycastTarget: {upImg.raycastTarget}");
        }
    }
}

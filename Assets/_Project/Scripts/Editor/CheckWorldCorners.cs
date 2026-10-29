using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class CheckWorldCorners
    {
        public static void Check()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            gv.gameObject.SetActive(true);

            var carCard = gv.transform.Find("CarCard") as RectTransform;
            var buyBtn = carCard.Find("BuyEquipBtn") as RectTransform;
            var upPanel = gv.transform.Find("UpgradesPanel") as RectTransform;

            Vector3[] cardCorners = new Vector3[4];
            Vector3[] buyCorners = new Vector3[4];
            Vector3[] upCorners = new Vector3[4];

            cardCardCorners(carCard, cardCorners, "CarCard");
            cardCardCorners(buyBtn, buyCorners, "BuyEquipBtn");
            cardCardCorners(upPanel, upCorners, "UpgradesPanel");

            // Check if buyCorners are inside upPanel rect
            bool overlaps = RectTransformOverlaps(buyBtn, upPanel);
            Debug.Log($"OVERLAPS BuyEquipBtn and UpgradesPanel: {overlaps}");
        }

        static void cardCardCorners(RectTransform rt, Vector3[] corners, string name)
        {
            rt.GetWorldCorners(corners);
            Debug.Log($"{name} Corners: BottomLeft={corners[0]}, TopLeft={corners[1]}, TopRight={corners[2]}, BottomRight={corners[3]}");
        }

        static bool RectTransformOverlaps(RectTransform a, RectTransform b)
        {
            Vector3[] aCorners = new Vector3[4];
            Vector3[] bCorners = new Vector3[4];
            a.GetWorldCorners(aCorners);
            b.GetWorldCorners(bCorners);

            Rect aRect = new Rect(aCorners[0].x, aCorners[0].y, aCorners[2].x - aCorners[0].x, aCorners[2].y - aCorners[0].y);
            Rect bRect = new Rect(bCorners[0].x, bCorners[0].y, bCorners[2].x - bCorners[0].x, bCorners[2].y - bCorners[0].y);

            return aRect.Overlaps(bRect);
        }
    }
}

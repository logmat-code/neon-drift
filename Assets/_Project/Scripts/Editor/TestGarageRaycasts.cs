using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class TestGarageRaycasts
    {
        public static void Check()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            if (gv == null)
            {
                Debug.LogError("No GarageView!");
                return;
            }

            Debug.Log($"--- Inspecting all Graphic raycastTargets in GarageView ---");
            var graphics = gv.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                Debug.Log($"Graphic: '{g.gameObject.name}', Type: {g.GetType().Name}, raycastTarget: {g.raycastTarget}, siblingIndex: {g.transform.GetSiblingIndex()}, parent: {g.transform.parent.name}");
            }
        }
    }
}

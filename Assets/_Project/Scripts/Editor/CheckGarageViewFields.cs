using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Presentation;

namespace NeonDrift.Editor
{
    public static class CheckGarageViewFields
    {
        public static void Check()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gv = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            if (gv == null)
            {
                Debug.LogError("No GarageView found!");
                return;
            }

            SerializedObject so = new SerializedObject(gv);
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    Debug.Log($"GarageView Field '{prop.name}': {prop.objectReferenceValue}");
                }
            }
        }
    }
}

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Progression;

namespace NeonDrift.Editor
{
    public static class CheckProgressionManagerField
    {
        public static void Check()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var pm = UnityEngine.Object.FindAnyObjectByType<ProgressionManager>(FindObjectsInactive.Include);
            if (pm == null)
            {
                Debug.LogError("No ProgressionManager in scene!");
                return;
            }

            SerializedObject so = new SerializedObject(pm);
            var catProp = so.FindProperty("carCatalog");
            Debug.Log($"ProgressionManager carCatalog: {catProp?.objectReferenceValue}");
        }
    }
}

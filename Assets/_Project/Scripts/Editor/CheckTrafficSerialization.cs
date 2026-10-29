using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.World;
using NeonDrift.Presentation;
using NeonDrift.Core;

namespace NeonDrift.Editor
{
    public static class CheckTrafficSerialization
    {
        public static void Check()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var trafficMgr = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            if (trafficMgr == null)
            {
                Debug.LogError("No TrafficManager in scene!");
                return;
            }

            SerializedObject so = new SerializedObject(trafficMgr);
            var playerProp = so.FindProperty("playerVehicle");
            var trackProp = so.FindProperty("trackManager");
            var compactProp = so.FindProperty("compactSprite");
            var sedanProp = so.FindProperty("sedanSprite");

            Debug.Log($"TrafficManager Serialized Fields: playerVehicle={playerProp?.objectReferenceValue}, trackManager={trackProp?.objectReferenceValue}, compactSprite={compactProp?.objectReferenceValue}, sedanSprite={sedanProp?.objectReferenceValue}");

            // Also check GarageView
            var garageView = UnityEngine.Object.FindAnyObjectByType<GarageView>(FindObjectsInactive.Include);
            if (garageView != null)
            {
                SerializedObject gso = new SerializedObject(garageView);
                var buyBtn = gso.FindProperty("buyCarButton");
                var equipBtn = gso.FindProperty("equipCarButton");
                var carCatalog = gso.FindProperty("carCatalog");
                Debug.Log($"GarageView Serialized Fields: buyBtn={buyBtn?.objectReferenceValue}, equipBtn={equipBtn?.objectReferenceValue}, carCatalog={carCatalog?.objectReferenceValue}");
            }
        }
    }
}

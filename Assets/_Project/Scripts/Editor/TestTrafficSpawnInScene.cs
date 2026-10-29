using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.World;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class TestTrafficSpawnInScene
    {
        public static void Test()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var trafficMgr = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            var trackMgr = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();

            // Initialize track
            trackMgr.ResetTrack(42);

            // Spawn 3 cars directly through TrafficManager reflection or by triggering SpawnNextTraffic
            var spawnMethod = typeof(TrafficManager).GetMethod("SpawnNextTraffic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (spawnMethod != null)
            {
                spawnMethod.Invoke(trafficMgr, null);
                spawnMethod.Invoke(trafficMgr, null);
                spawnMethod.Invoke(trafficMgr, null);
            }

            Debug.Log($"Traffic active count after spawning: {trafficMgr.activeTrafficCount}");

            // Inspect the spawned cars
            var spawnedCars = UnityEngine.Object.FindObjectsByType<TrafficCar>(FindObjectsSortMode.None);
            Debug.Log($"Total TrafficCar objects found: {spawnedCars.Length}");
            foreach (var car in spawnedCars)
            {
                var sr = car.GetComponent<SpriteRenderer>();
                var col = car.GetComponent<BoxCollider2D>();
                Debug.Log($"Car: name='{car.name}', active={car.gameObject.activeInHierarchy}, pos={car.transform.position}, sprite={sr.sprite?.name}, sortingOrder={sr.sortingOrder}, color={sr.color}, collider={col.size}, tag={car.tag}");
            }
        }
    }
}

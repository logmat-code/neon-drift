using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Core;
using NeonDrift.World;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class TestTrafficSpawnOverTime
    {
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            var tm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            var track = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();

            gm.StartRun();

            // Simulate 20 seconds of gameplay Update calls
            float dt = 0.05f;
            for (float t = 0f; t < 20f; t += dt)
            {
                // Advance player
                track.totalDistanceTraveled += 20f * dt;
                player.transform.position += new Vector3(0, 20f * dt, 0);

                // Call TrafficManager.Update() via reflection
                var updateMethod = typeof(TrafficManager).GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                updateMethod.Invoke(tm, null);
            }

            Debug.Log($"[SPAWN_TIME_TEST] After 20s: activeTrafficCount={tm.activeTrafficCount}, pooledTrafficCount={tm.pooledTrafficCount}, totalDist={track.totalDistanceTraveled}");
            var cars = UnityEngine.Object.FindObjectsByType<TrafficCar>(FindObjectsSortMode.None);
            foreach (var c in cars)
            {
                Debug.Log($"Active Car: type={c.carType}, longDist={c.longitudinalDistance}, pos={c.transform.position}, active={c.gameObject.activeInHierarchy}");
            }
        }
    }
}

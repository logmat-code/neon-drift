using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Core;
using NeonDrift.World;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class TestTrafficDetailedLogs
    {
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            var tm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            var track = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();

            gm.StartRun();
            player.currentSpeed = 18f;

            // Step 1: Simulate 1 frame
            float dt = 0.05f;
            var updateMethod = typeof(TrafficManager).GetMethod("Update", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            for (int step = 0; step < 80; step++)
            {
                player.transform.position += new Vector3(0, player.currentSpeed * dt, 0);
                track.ProcessUpdate(dt);
                updateMethod.Invoke(tm, null);

                if (tm.activeTrafficCount > 0)
                {
                    Debug.Log($"[TRAFFIC_STEP {step}] playerDist={track.totalDistanceTraveled:F1}, activeCount={tm.activeTrafficCount}");
                    var cars = UnityEngine.Object.FindObjectsByType<TrafficCar>(FindObjectsSortMode.None);
                    foreach (var c in cars)
                    {
                        if (c.gameObject.activeInHierarchy)
                        {
                            Debug.Log($" -> Car '{c.name}': longDist={c.longitudinalDistance:F1}, diff={c.longitudinalDistance - track.totalDistanceTraveled:F1}, speed={c.speed:F1}, pos={c.transform.position}");
                        }
                    }
                }
            }
        }
    }
}

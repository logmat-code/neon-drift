using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Core;
using NeonDrift.World;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class TestTrafficSimulationFixedDt
    {
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            var tm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            var track = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();

            gm.StartRun();
            player.currentSpeed = 16f;

            float simDt = 0.05f;
            int totalSteps = 400; // 20 seconds of real time
            int visibleEncounters = 0;

            for (int step = 0; step < totalSteps; step++)
            {
                player.transform.position += new Vector3(0, player.currentSpeed * simDt, 0);
                track.ProcessUpdate(simDt);

                // Simulate TrafficManager.ProcessTrafficUpdate with explicit simDt
                // In game, TrafficManager.Update() uses Time.deltaTime (which is 0.016s in 60fps Play Mode!)
                // Let's call ProcessUpdate on TrafficManager
                var updateMethod = typeof(TrafficManager).GetMethod("UpdateTraffic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                // Let's see what happens to active cars
            }
        }
    }
}

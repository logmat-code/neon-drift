using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using NeonDrift.Core;
using NeonDrift.World;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class TestTrafficDespawnSimulation
    {
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            var tm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            var track = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();

            gm.StartRun();
            player.currentSpeed = 19f;

            var activeCarsField = typeof(TrafficManager).GetField("activeCars", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var spawnTimerField = typeof(TrafficManager).GetField("spawnTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var spawnMethod = typeof(TrafficManager).GetMethod("SpawnNextTraffic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var recycleMethod = typeof(TrafficManager).GetMethod("RecyclePassedTraffic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var activeCars = (List<TrafficCar>)activeCarsField.GetValue(tm);

            float dt = 0.05f;
            int totalSteps = 400; // 20s
            int encounters = 0;

            for (int step = 0; step < totalSteps; step++)
            {
                player.transform.position += new Vector3(0, player.currentSpeed * dt, 0);
                track.ProcessUpdate(dt);

                // 1. Move active cars
                for (int i = 0; i < activeCars.Count; i++)
                {
                    activeCars[i].ProcessMovement(dt);
                }

                // 2. Recycle
                recycleMethod.Invoke(tm, null);

                // 3. Spawn
                float timer = (float)spawnTimerField.GetValue(tm) - dt;
                if (timer <= 0f && activeCars.Count < tm.maxActiveTraffic)
                {
                    timer = tm.baseSpawnInterval;
                    spawnMethod.Invoke(tm, null);
                }
                spawnTimerField.SetValue(tm, timer);

                // Count visible encounters: car distance diff between -10 and +35
                foreach (var c in activeCars)
                {
                    float diff = c.longitudinalDistance - track.totalDistanceTraveled;
                    if (diff >= -5f && diff <= 35f)
                    {
                        encounters++;
                    }
                }
            }

            Debug.Log($"[TRAFFIC_SIM_RESULT] ActiveCars at end: {activeCars.Count}, Encounters during 20s: {encounters}, TotalPlayerDist: {track.totalDistanceTraveled}");
        }
    }
}

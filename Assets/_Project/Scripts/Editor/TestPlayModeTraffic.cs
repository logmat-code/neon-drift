using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Core;
using NeonDrift.World;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class TestPlayModeTraffic
    {
        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var gm = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            var tm = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            var track = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();

            Debug.Log($"[PLAY_TEST] Initial: gm={gm!=null}, tm={tm!=null}, track={track!=null}, player={player!=null}");
            
            // Player clicks DRIVE (calls gm.StartRun())
            gm.StartRun();

            Debug.Log($"[PLAY_TEST] After StartRun: trafficActiveCount={tm.activeTrafficCount}, totalDist={track.totalDistanceTraveled}");

            // Let's inspect spawned traffic cars right now
            var activeCars = UnityEngine.Object.FindObjectsByType<TrafficCar>(FindObjectsSortMode.None);
            Debug.Log($"[PLAY_TEST] TrafficCars in scene right after StartRun: {activeCars.Length}");
        }
    }
}

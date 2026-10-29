using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.World;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class TestLiveTrafficPlaymode
    {
        public static void SimulateRun()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var trackMgr = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var trafficMgr = UnityEngine.Object.FindAnyObjectByType<TrafficManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();

            Debug.Log($"[SIM] Starting test. TrackManager: {trackMgr != null}, TrafficManager: {trafficMgr != null}, Player: {player != null}");

            // Simulate Start
            trackMgr.ResetTrack(12345);
            Debug.Log($"[SIM] After ResetTrack: activeChunks={trackMgr.ActiveChunks.Count}, totalDistanceTraveled={trackMgr.totalDistanceTraveled}");

            // Check sampling for traffic
            float spawnDist = trackMgr.totalDistanceTraveled + 55f;
            bool sampleOk = trackMgr.GetRoadSampleAtDistance(spawnDist, 0f, out Vector2 pos, out Quaternion rot);
            Debug.Log($"[SIM] Sampling at spawnDist {spawnDist}: success={sampleOk}, pos={pos}, rot={rot.eulerAngles}");

            // Now simulate player driving forward 100 meters
            // In game: player moves up in Y (heading roughly north/upwards)
            // Let's see what happens to activeChunks and sampling as distance increases
            for (float dist = 0f; dist <= 300f; dist += 50f)
            {
                bool s = trackMgr.GetRoadSampleAtDistance(dist, 0f, out Vector2 p, out Quaternion r);
                Debug.Log($"[SIM] Sample at dist {dist}m: success={s}, pos={p}");
            }
        }
    }
}

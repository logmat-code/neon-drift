using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.World;
using NeonDrift.Vehicle;

namespace NeonDrift.Editor
{
    public static class TestRoadSampleDistance
    {
        public static void Test()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/NeonDistrictLoop.unity");
            var trackMgr = UnityEngine.Object.FindAnyObjectByType<TrackManager>();
            var player = UnityEngine.Object.FindAnyObjectByType<VehicleController>();

            Debug.Log($"TrackManager totalDistanceTraveled: {trackMgr.totalDistanceTraveled}, activeChunks: {trackMgr.ActiveChunks.Count}");
            
            float totalChunkLen = 0f;
            foreach (var chunk in trackMgr.ActiveChunks)
            {
                totalChunkLen += chunk.lengthMeters;
                Debug.Log($"Chunk: {chunk.name}, length: {chunk.lengthMeters}, entry: {chunk.entryPosition}, exit: {chunk.exitPosition}");
            }
            Debug.Log($"Total active chunks length: {totalChunkLen}");

            bool s0 = trackMgr.GetRoadSampleAtDistance(0f, 0f, out Vector2 p0, out Quaternion r0);
            bool s55 = trackMgr.GetRoadSampleAtDistance(55f, 0f, out Vector2 p55, out Quaternion r55);
            Debug.Log($"Sample at 0: success={s0}, pos={p0}; Sample at 55: success={s55}, pos={p55}");
            
            if (player != null)
            {
                Debug.Log($"Player pos: {player.transform.position}");
            }
        }
    }
}

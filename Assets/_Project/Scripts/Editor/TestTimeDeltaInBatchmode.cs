using UnityEditor;
using UnityEngine;

namespace NeonDrift.Editor
{
    public static class TestTimeDeltaInBatchmode
    {
        public static void Check()
        {
            Debug.Log($"Time.deltaTime in batchmode: {Time.deltaTime}, Time.time: {Time.time}");
        }
    }
}

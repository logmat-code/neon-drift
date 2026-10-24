using UnityEngine;

namespace NeonDrift.Data
{
    [CreateAssetMenu(fileName = "FlowConfig", menuName = "NeonDrift/Flow Config")]
    public class FlowConfig : ScriptableObject
    {
        [Header("Multipliers")]
        public int[] multipliers = { 1, 2, 3, 5, 8 };
        public float[] flowThresholdsForMultiplier = { 0f, 25f, 50f, 75f, 95f };
        public float maxFlow = 100f;
        public float flowDecayRate = 7.5f;

        [Header("Flow Gains")]
        public float flowGainDriftPerSec = 16f;
        public float flowGainLongDrift = 25f;
        public float flowGainPerfectExit = 20f;
        public float flowGainDriftChain = 15f;
        public float flowGainNearMiss = 18f;
        public float flowGainDriftNearMiss = 30f;
        public float flowGainTrafficGap = 25f;
        public float flowGainDriftGap = 40f;
        public float flowGainHighSpeedPerSec = 6f;

        [Header("Base Scores")]
        public int scorePerDriftSec = 300;
        public int scoreLongDrift = 1200;
        public int scorePerfectExit = 500;
        public int scoreDriftChain = 400;
        public int scoreNearMiss = 250;
        public int scoreDriftNearMiss = 650;
        public int scoreTrafficGap = 500;
        public int scoreDriftGap = 1000;
        public int scoreHighSpeedPerSec = 100;

        [Header("Penalties")]
        public float flowPenaltyWallTouch = 15f;
        public float flowPenaltyTrafficHit = 40f;
        public float flowPenaltyHeavyCrash = 80f;

        [Header("Boost Generation")]
        public float boostEnergyPerFlowGain = 0.6f;
    }
}

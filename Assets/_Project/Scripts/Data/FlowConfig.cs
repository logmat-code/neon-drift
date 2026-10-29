using UnityEngine;

namespace NeonDrift.Data
{
    [CreateAssetMenu(fileName = "FlowConfig", menuName = "NeonDrift/Flow Config")]
    public class FlowConfig : ScriptableObject
    {
        [Header("Multipliers")]
        public int[] multipliers = { 1, 2, 3 }; // Suggested limit: x3 maximum
        public float[] flowThresholdsForMultiplier = { 0f, 35f, 70f };
        public float maxFlow = 100f;
        public float flowDecayRate = 8.5f;

        [Header("Flow Gains")]
        public float flowGainDriftPerSec = 12f;
        public float flowGainLongDrift = 18f;
        public float flowGainPerfectExit = 15f;
        public float flowGainDriftChain = 12f;
        public float flowGainNearMiss = 14f;
        public float flowGainDriftNearMiss = 22f;
        public float flowGainTrafficGap = 16f;
        public float flowGainDriftGap = 26f;
        public float flowGainHighSpeedPerSec = 4f;

        [Header("Base Scores (Calibrated Points System)")]
        // Drift corto: 10-30 pts, Drift medio: 30-80 pts, Drift largo: 80-180 pts, Near Miss: 15-40 pts
        public int scorePerDriftSec = 25;
        public int scoreLongDrift = 80;
        public int scorePerfectExit = 45;
        public int scoreDriftChain = 35;
        public int scoreNearMiss = 25;
        public int scoreDriftNearMiss = 40;
        public int scoreTrafficGap = 30;
        public int scoreDriftGap = 50;
        public int scoreHighSpeedPerSec = 10;

        [Header("Penalties")]
        public float flowPenaltyWallTouch = 15f;
        public float flowPenaltyTrafficHit = 40f;
        public float flowPenaltyHeavyCrash = 80f;

        [Header("Boost Generation")]
        public float boostEnergyPerFlowGain = 0.6f;
    }
}

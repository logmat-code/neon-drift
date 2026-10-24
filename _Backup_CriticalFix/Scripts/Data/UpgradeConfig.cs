using UnityEngine;

namespace NeonDrift.Data
{
    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "NeonDrift/Upgrade Config")]
    public class UpgradeConfig : ScriptableObject
    {
        public int maxLevel = 5;

        [Header("Engine Upgrades (Top Speed & Acceleration)")]
        public int[] engineCost = { 400, 800, 1500, 2600, 4200 };
        public float[] engineSpeedBonus = { 0f, 1.8f, 3.8f, 6.0f, 8.5f };
        public float[] engineAccelBonus = { 0f, 1.5f, 3.2f, 5.0f, 7.2f };

        [Header("Tires Upgrades (Grip & Drift Recovery)")]
        public int[] tiresCost = { 350, 750, 1400, 2400, 3900 };
        public float[] tiresGripBonus = { 0f, 1.2f, 2.5f, 4.0f, 5.8f };
        public float[] tiresRecoveryBonus = { 0f, 0.6f, 1.4f, 2.3f, 3.5f };

        [Header("Turbo Upgrades (Boost Power & Duration)")]
        public int[] turboCost = { 500, 1000, 1800, 3000, 4800 };
        public float[] turboPowerBonus = { 0f, 0.08f, 0.18f, 0.30f, 0.45f };
        public float[] turboDurationBonus = { 0f, 0.4f, 0.9f, 1.5f, 2.2f };

        public int GetEngineCost(int currentLevel) => currentLevel < maxLevel ? engineCost[currentLevel] : -1;
        public int GetTiresCost(int currentLevel) => currentLevel < maxLevel ? tiresCost[currentLevel] : -1;
        public int GetTurboCost(int currentLevel) => currentLevel < maxLevel ? turboCost[currentLevel] : -1;
    }
}

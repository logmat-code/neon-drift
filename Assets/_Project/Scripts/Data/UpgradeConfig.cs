using UnityEngine;

namespace NeonDrift.Data
{
    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "NeonDrift/Upgrade Config")]
    public class UpgradeConfig : ScriptableObject
    {
        public int maxLevel = 5;

        [Header("Engine Upgrades (Top Speed & Acceleration)")]
        public int[] engineCost = { 200, 450, 850, 1500, 2500 };
        public float[] engineSpeedBonus = { 0f, 1.2f, 2.6f, 4.2f, 6.0f };
        public float[] engineAccelBonus = { 0f, 1.0f, 2.2f, 3.6f, 5.2f };

        [Header("Tires Upgrades (Grip & Drift Recovery)")]
        public int[] tiresCost = { 200, 450, 850, 1500, 2500 };
        public float[] tiresGripBonus = { 0f, 1.0f, 2.2f, 3.5f, 5.0f };
        public float[] tiresRecoveryBonus = { 0f, 0.5f, 1.1f, 1.8f, 2.8f };

        [Header("Turbo Upgrades (Boost Power & Duration)")]
        public int[] turboCost = { 200, 450, 850, 1500, 2500 };
        public float[] turboPowerBonus = { 0f, 0.08f, 0.18f, 0.30f, 0.45f };
        public float[] turboDurationBonus = { 0f, 0.4f, 0.9f, 1.5f, 2.2f };

        public int GetEngineCost(int currentLevel) => currentLevel < maxLevel ? engineCost[currentLevel] : -1;
        public int GetTiresCost(int currentLevel) => currentLevel < maxLevel ? tiresCost[currentLevel] : -1;
        public int GetTurboCost(int currentLevel) => currentLevel < maxLevel ? turboCost[currentLevel] : -1;
        public int GetUpgradeCost(int currentLevel) => currentLevel < maxLevel ? engineCost[Mathf.Clamp(currentLevel, 0, engineCost.Length - 1)] : -1;
    }
}

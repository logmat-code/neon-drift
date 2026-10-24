using System;
using UnityEngine;
using NeonDrift.Data;
using NeonDrift.Vehicle;

namespace NeonDrift.Progression
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        [SerializeField] private UpgradeConfig upgradeConfig;

        public SaveData CurrentData { get; private set; }

        public event Action OnProfileUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadProfile();
        }

        public void LoadProfile()
        {
            CurrentData = SaveSystem.Load();
            OnProfileUpdated?.Invoke();
        }

        public void SaveProfile()
        {
            SaveSystem.Save(CurrentData);
            OnProfileUpdated?.Invoke();
        }

        public void ApplyUpgradesToVehicle(VehicleController vehicle)
        {
            if (vehicle == null || upgradeConfig == null) return;

            int engLvl = Mathf.Clamp(CurrentData.engineLevel, 0, upgradeConfig.maxLevel);
            int tireLvl = Mathf.Clamp(CurrentData.tiresLevel, 0, upgradeConfig.maxLevel);
            int turboLvl = Mathf.Clamp(CurrentData.turboLevel, 0, upgradeConfig.maxLevel);

            float speedBonus = upgradeConfig.engineSpeedBonus[engLvl];
            float accelBonus = upgradeConfig.engineAccelBonus[engLvl];

            float gripBonus = upgradeConfig.tiresGripBonus[tireLvl];
            float recoveryBonus = upgradeConfig.tiresRecoveryBonus[tireLvl];

            float turboPower = upgradeConfig.turboPowerBonus[turboLvl];
            float turboDuration = upgradeConfig.turboDurationBonus[turboLvl];

            vehicle.ApplyUpgradeBonuses(speedBonus, accelBonus, gripBonus, recoveryBonus, turboPower, turboDuration);
        }

        public bool TryUpgradeEngine()
        {
            int cost = upgradeConfig.GetEngineCost(CurrentData.engineLevel);
            if (cost > 0 && CurrentData.credits >= cost)
            {
                CurrentData.credits -= cost;
                CurrentData.engineLevel++;
                SaveProfile();
                return true;
            }
            return false;
        }

        public bool TryUpgradeTires()
        {
            int cost = upgradeConfig.GetTiresCost(CurrentData.tiresLevel);
            if (cost > 0 && CurrentData.credits >= cost)
            {
                CurrentData.credits -= cost;
                CurrentData.tiresLevel++;
                SaveProfile();
                return true;
            }
            return false;
        }

        public bool TryUpgradeTurbo()
        {
            int cost = upgradeConfig.GetTurboCost(CurrentData.turboLevel);
            if (cost > 0 && CurrentData.credits >= cost)
            {
                CurrentData.credits -= cost;
                CurrentData.turboLevel++;
                SaveProfile();
                return true;
            }
            return false;
        }

        public int CalculateCreditsEarned(int score, bool completedTrack, int maxCombo, int nearMisses)
        {
            int baseReward = score / 15;
            int completionBonus = completedTrack ? 600 : 150;
            int comboBonus = maxCombo * 20;
            int skillBonus = nearMisses * 40;

            int totalCredits = baseReward + completionBonus + comboBonus + skillBonus;
            return Mathf.Max(50, totalCredits);
        }

        public void RecordRunResults(int score, float longestDrift, int maxCombo, int nearMisses, int creditsEarned)
        {
            CurrentData.totalRuns++;
            CurrentData.credits += creditsEarned;

            if (score > CurrentData.bestScore)
            {
                CurrentData.bestScore = score;
            }
            if (longestDrift > CurrentData.longestDriftRecord)
            {
                CurrentData.longestDriftRecord = longestDrift;
            }
            if (maxCombo > CurrentData.maxComboRecord)
            {
                CurrentData.maxComboRecord = maxCombo;
            }
            if (nearMisses > CurrentData.nearMissesRecord)
            {
                CurrentData.nearMissesRecord = nearMisses;
            }

            SaveProfile();
        }
    }
}

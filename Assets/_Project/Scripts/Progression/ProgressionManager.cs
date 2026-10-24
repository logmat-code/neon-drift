using System;
using UnityEngine;
using NeonDrift.Data;
using NeonDrift.Vehicle;
using NeonDrift.Gameplay;

namespace NeonDrift.Progression
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        [SerializeField] private CarCatalog carCatalog;
        [SerializeField] private CustomizationCatalog customCatalog;
        [SerializeField] private UpgradeConfig upgradeConfig;

        public bool TryUpgradeEngine()
        {
            return TryUpgradeCategory(CurrentData.selectedVehicleId, "engine");
        }

        public bool TryUpgradeTires()
        {
            return TryUpgradeCategory(CurrentData.selectedVehicleId, "drift");
        }

        public bool TryUpgradeTurbo()
        {
            return TryUpgradeCategory(CurrentData.selectedVehicleId, "turbo");
        }

        public int CalculateCreditsEarned(int score, bool completedTrack, int maxCombo, int nearMisses)
        {
            return CalculateCreditsEarned(score, completedTrack ? 1000f : 200f, maxCombo, nearMisses, nearMisses / 2);
        }

        public void RecordRunResults(int score, float longestDrift, int maxCombo, int nearMisses, int creditsEarned)
        {
            SettleRunIdempotent(Guid.NewGuid().ToString(), score, 500f, maxCombo, nearMisses, nearMisses / 2, out _, out _);
        }

        public CarCatalog CarCatalog => carCatalog;
        public CustomizationCatalog CustomCatalog => customCatalog;
        public UpgradeConfig UpgradeConfig => upgradeConfig;

        public SaveData CurrentData { get; private set; }

        public event Action OnProfileUpdated;
        public event Action<string> OnCarEquipped;

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

        public CarDefinition GetEquippedCar()
        {
            if (carCatalog != null)
            {
                return carCatalog.GetCar(CurrentData.selectedVehicleId);
            }
            return null;
        }

        public VehicleSaveData GetEquippedCarData()
        {
            return CurrentData.GetVehicleData(CurrentData.selectedVehicleId);
        }

        public bool TryEquipCar(string carId)
        {
            var data = CurrentData.GetVehicleData(carId);
            if (data != null && data.isUnlocked)
            {
                CurrentData.selectedVehicleId = carId;
                SaveProfile();
                OnCarEquipped?.Invoke(carId);
                return true;
            }
            return false;
        }

        public bool TryBuyCar(string carId)
        {
            var def = carCatalog?.GetCar(carId);
            if (def == null) return false;

            var data = CurrentData.GetVehicleData(carId);
            if (data.isUnlocked) return true; // Already owned

            if (CurrentData.credits >= def.priceNC)
            {
                CurrentData.credits -= def.priceNC;
                data.isUnlocked = true;
                CurrentData.selectedVehicleId = carId;
                SaveProfile();
                OnCarEquipped?.Invoke(carId);
                return true;
            }
            return false;
        }

        // Upgrades: 6 Categories (Engine, Turbo, Steering, Drift Control, Brakes, Durability)
        public int GetUpgradeCost(int currentLevel)
        {
            return currentLevel switch
            {
                0 => 300,
                1 => 700,
                2 => 1400,
                3 => 2500,
                4 => 4000,
                _ => -1 // Maxed
            };
        }

        public bool TryUpgradeCategory(string carId, string category)
        {
            var vData = CurrentData.GetVehicleData(carId);
            int currentLevel = category.ToLower() switch
            {
                "engine" => vData.engineLevel,
                "turbo" => vData.turboLevel,
                "steering" => vData.steeringLevel,
                "drift" => vData.driftControlLevel,
                "brakes" => vData.brakesLevel,
                "durability" => vData.durabilityLevel,
                _ => -1
            };

            if (currentLevel < 0 || currentLevel >= 5) return false;
            int cost = GetUpgradeCost(currentLevel);
            if (cost > 0 && CurrentData.credits >= cost)
            {
                CurrentData.credits -= cost;
                switch (category.ToLower())
                {
                    case "engine": vData.engineLevel++; break;
                    case "turbo": vData.turboLevel++; break;
                    case "steering": vData.steeringLevel++; break;
                    case "drift": vData.driftControlLevel++; break;
                    case "brakes": vData.brakesLevel++; break;
                    case "durability": vData.durabilityLevel++; break;
                }
                SaveProfile();
                return true;
            }
            return false;
        }

        // Customization
        public bool TryBuyCustomization(string itemId, int price)
        {
            if (CurrentData.unlockedCustomizations.Contains(itemId)) return true;
            if (CurrentData.credits >= price)
            {
                CurrentData.credits -= price;
                CurrentData.unlockedCustomizations.Add(itemId);
                SaveProfile();
                return true;
            }
            return false;
        }

        public void EquipPaint(string carId, int index)
        {
            CurrentData.GetVehicleData(carId).paintColorIndex = index;
            SaveProfile();
        }

        public void EquipUnderglow(string carId, int index)
        {
            CurrentData.GetVehicleData(carId).underglowIndex = index;
            SaveProfile();
        }

        public void EquipSmoke(string carId, int index)
        {
            CurrentData.GetVehicleData(carId).smokeIndex = index;
            SaveProfile();
        }

        public void ApplyUpgradesToVehicle(VehicleController vehicle)
        {
            if (vehicle == null) return;

            string carId = CurrentData.selectedVehicleId;
            var carDef = carCatalog?.GetCar(carId);
            var vData = CurrentData.GetVehicleData(carId);

            // Base stats from selected car definition
            if (carDef != null && carDef.carSprite != null)
            {
                var sr = vehicle.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = carDef.carSprite;
            }

            // Apply 5-tier upgrade bonuses
            float speedBonus = vData.engineLevel * 0.8f;
            float accelBonus = vData.engineLevel * 0.6f;
            float gripBonus = vData.steeringLevel * 0.05f;
            float recoveryBonus = vData.brakesLevel * 0.08f;
            float turboPower = vData.turboLevel * 0.06f;
            float turboDuration = vData.turboLevel * 0.25f;

            vehicle.ApplyUpgradeBonuses(speedBonus, accelBonus, gripBonus, recoveryBonus, turboPower, turboDuration);

            // Apply Durability upgrade to CollisionHandler
            var colHandler = vehicle.GetComponent<CollisionHandler>();
            if (colHandler != null)
            {
                int baseHits = carDef != null ? carDef.maxDurabilityHits : 3;
                colHandler.maxDurability = baseHits + vData.durabilityLevel;
                colHandler.ResetDurability();
            }

            // Apply Visual Customization (Paint & Underglow)
            ApplyVisualCustomization(vehicle, vData);
        }

        public void ApplyVisualCustomization(VehicleController vehicle, VehicleSaveData vData)
        {
            if (vehicle == null || customCatalog == null) return;

            var sr = vehicle.GetComponent<SpriteRenderer>();
            if (sr != null && customCatalog.paints != null && vData.paintColorIndex < customCatalog.paints.Count)
            {
                sr.color = customCatalog.paints[vData.paintColorIndex].primaryColor;
            }

            // Apply underglow light / sprite if present
            Transform underglowT = vehicle.transform.Find("NeonUnderglow");
            if (underglowT != null)
            {
                var ugSr = underglowT.GetComponent<SpriteRenderer>();
                if (ugSr != null && customCatalog.underglows != null && vData.underglowIndex < customCatalog.underglows.Count)
                {
                    ugSr.color = customCatalog.underglows[vData.underglowIndex].emissionColor;
                }
            }
        }

        // Idempotent Run Earnings Settlement
        public int CalculateCreditsEarned(int score, float distanceMeters, int maxCombo, int nearMisses, int trafficGaps)
        {
            int distNC = (int)(distanceMeters / 100f) * 5;
            int scoreNC = (score / 1000) * 10;
            int comboNC = (maxCombo >= 3) ? (maxCombo * 25) : (maxCombo * 10);
            int nearMissNC = nearMisses * 15;
            int gapNC = trafficGaps * 20;

            int totalNC = distNC + scoreNC + comboNC + nearMissNC + gapNC;
            return Mathf.Max(30, totalNC);
        }

        public bool SettleRunIdempotent(string runId, int score, float distanceMeters, int maxCombo, int nearMisses, int trafficGaps, out int ncEarned, out bool isNewRecord)
        {
            ncEarned = 0;
            isNewRecord = false;

            if (string.IsNullOrEmpty(runId)) runId = Guid.NewGuid().ToString();
            if (CurrentData.settledRunIds.Contains(runId))
            {
                Debug.LogWarning($"[Progression] Run {runId} was already settled. Avoiding duplicate payout.");
                return false;
            }

            ncEarned = CalculateCreditsEarned(score, distanceMeters, maxCombo, nearMisses, trafficGaps);
            if (score > CurrentData.bestScore)
            {
                isNewRecord = true;
                ncEarned += 100; // New Personal Best bonus
                CurrentData.bestScore = score;
            }

            CurrentData.credits += ncEarned;
            CurrentData.totalRuns++;
            CurrentData.settledRunIds.Add(runId);
            // Cap history
            if (CurrentData.settledRunIds.Count > 50) CurrentData.settledRunIds.RemoveAt(0);

            if (distanceMeters > CurrentData.totalDistanceRecord) CurrentData.totalDistanceRecord = distanceMeters;
            if (maxCombo > CurrentData.maxComboRecord) CurrentData.maxComboRecord = maxCombo;
            if (nearMisses > CurrentData.nearMissesRecord) CurrentData.nearMissesRecord = nearMisses;

            SaveProfile();
            return true;
        }

        public void RewardRewardedAdNC(int bonusNC)
        {
            CurrentData.credits += bonusNC;
            SaveProfile();
        }
    }
}

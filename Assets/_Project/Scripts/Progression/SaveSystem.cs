using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonDrift.Progression
{
    [Serializable]
    public class VehicleSaveData
    {
        public string vehicleId = "kitsune_s13";
        public bool isUnlocked = true;
        // Upgrades (0 to 5)
        public int engineLevel = 0;
        public int turboLevel = 0;
        public int steeringLevel = 0;
        public int driftControlLevel = 0;
        public int brakesLevel = 0;
        public int durabilityLevel = 0;

        // Customization selections
        public int paintColorIndex = 0;
        public int liveryIndex = 0;
        public int rimsIndex = 0;
        public int underglowIndex = 0;
        public int smokeIndex = 0;
    }

    [Serializable]
    public class SaveData
    {
        public int credits = 1000; // Neon Credits (NC)
        public int bestScore = 0;
        public int totalRuns = 0;
        public float longestDriftRecord = 0f;
        public int maxComboRecord = 0;
        public int nearMissesRecord = 0;
        public float totalDistanceRecord = 0f;

        // Active Equipped Vehicle
        public string selectedVehicleId = "kitsune_s13";

        // Vehicle inventories and upgrades
        public List<VehicleSaveData> vehicles = new List<VehicleSaveData>();

        // Purchased Customization Item IDs
        public List<string> unlockedCustomizations = new List<string>();

        // Audio & Settings
        public float masterVolume = 1f;
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
        public float engineVolume = 1f;

        // Tutorial flags
        public bool tutorialCompleted = false;
        public bool tutorialSkipped = false;

        // Processed Run IDs for Idempotent settlement (protects against double payouts)
        public List<string> settledRunIds = new List<string>();

        // Legacy compatibility
        public int engineLevel => GetVehicleData(selectedVehicleId).engineLevel;
        public int tiresLevel => GetVehicleData(selectedVehicleId).driftControlLevel;
        public int turboLevel => GetVehicleData(selectedVehicleId).turboLevel;

        public VehicleSaveData GetVehicleData(string vehicleId)
        {
            if (vehicles == null) vehicles = new List<VehicleSaveData>();
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i].vehicleId == vehicleId) return vehicles[i];
            }
            var fresh = new VehicleSaveData { vehicleId = vehicleId, isUnlocked = (vehicleId == "kitsune_s13") };
            vehicles.Add(fresh);
            return fresh;
        }
    }

    public static class SaveSystem
    {
        private const string SAVE_KEY = "NeonDrift_SaveProfile_v2";

        public static SaveData Load()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                SaveData fresh = new SaveData();
                fresh.GetVehicleData("kitsune_s13").isUnlocked = true;
                fresh.unlockedCustomizations.Add("paint_0");
                fresh.unlockedCustomizations.Add("underglow_0");
                fresh.unlockedCustomizations.Add("smoke_0");
                Save(fresh);
                return fresh;
            }

            try
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) data = new SaveData();
                if (data.vehicles == null || data.vehicles.Count == 0)
                {
                    data.GetVehicleData("kitsune_s13").isUnlocked = true;
                }
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Failed to parse save data: {e.Message}. Generating default.");
                return new SaveData();
            }
        }

        public static void Save(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Error saving data: {e.Message}");
            }
        }

        public static void ResetProfile()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
        }
    }
}

using System;
using UnityEngine;

namespace NeonDrift.Progression
{
    [Serializable]
    public class SaveData
    {
        public int credits = 1000; // Starting credit stash for initial experimentation
        public int bestScore = 0;
        public int engineLevel = 0;
        public int tiresLevel = 0;
        public int turboLevel = 0;
        public int totalRuns = 0;
        public float longestDriftRecord = 0f;
        public int maxComboRecord = 0;
        public int nearMissesRecord = 0;
        public float sfxVolume = 1f;
        public float musicVolume = 1f;
    }

    public static class SaveSystem
    {
        private const string SAVE_KEY = "NeonDrift_SaveProfile_v1";

        public static SaveData Load()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                SaveData fresh = new SaveData();
                Save(fresh);
                return fresh;
            }

            try
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                return data ?? new SaveData();
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

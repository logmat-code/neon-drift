using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NeonDrift.Progression;

namespace NeonDrift.Editor
{
    public static class InspectVehicleSaveData
    {
        public static void Inspect()
        {
            var data = SaveSystem.Load();
            Debug.Log($"Loaded SaveData: selectedVehicleId={data.selectedVehicleId}, credits={data.credits}, vehiclesCount={data.vehicles.Count}");
            foreach (var v in data.vehicles)
            {
                Debug.Log($"Vehicle in SaveData: id='{v.vehicleId}', isUnlocked={v.isUnlocked}");
            }

            var akumaData = data.GetVehicleData("akuma_86");
            Debug.Log($"data.GetVehicleData('akuma_86'): id='{akumaData.vehicleId}', isUnlocked={akumaData.isUnlocked}");
        }
    }
}

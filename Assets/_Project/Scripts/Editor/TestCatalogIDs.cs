using UnityEditor;
using UnityEngine;
using NeonDrift.Data;

namespace NeonDrift.Editor
{
    public static class TestCatalogIDs
    {
        public static void Check()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<CarCatalog>("Assets/_Project/Data/CarCatalog.asset");
            if (catalog == null)
            {
                Debug.LogError("CarCatalog.asset NOT found!");
                return;
            }

            Debug.Log($"CarCatalog entries count: {catalog.cars.Count}");
            foreach (var c in catalog.cars)
            {
                Debug.Log($"Catalog Car: id='{c.id}', displayName='{c.displayName}', price={c.priceNC}");
            }

            Debug.Log($"GetCar('akuma_86'): {catalog.GetCar("akuma_86")?.displayName}");
            Debug.Log($"GetCar('AKUMA_86'): {catalog.GetCar("AKUMA_86")?.displayName}");
        }
    }
}

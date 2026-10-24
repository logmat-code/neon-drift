using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using NeonDrift.Data;

namespace NeonDrift.Editor
{
    public static class BuildAssetsIntegrator
    {
        private const string DATA_DIR = "Assets/_Project/Data/";
        private const string ART_VEHICLES = "Assets/_Project/Art/Vehicles/";
        private const string ART_TRAFFIC = "Assets/_Project/Art/Traffic/";

        public static void EnsureCatalogs()
        {
            if (!Directory.Exists(DATA_DIR)) Directory.CreateDirectory(DATA_DIR);

            // 1. Car Catalog
            string carCatPath = DATA_DIR + "CarCatalog.asset";
            var carCatalog = AssetDatabase.LoadAssetAtPath<CarCatalog>(carCatPath);
            if (carCatalog == null)
            {
                carCatalog = ScriptableObject.CreateInstance<CarCatalog>();
                AssetDatabase.CreateAsset(carCatalog, carCatPath);
            }

            carCatalog.cars.Clear();

            // KITSUNE S13
            carCatalog.cars.Add(new CarDefinition
            {
                id = "kitsune_s13",
                displayName = "KITSUNE S13",
                archetype = "BALANCED STREET DRIFTER",
                priceNC = 0,
                carSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_VEHICLES + "kitsune_s13.png"),
                baseTopSpeed = 19f,
                baseAcceleration = 15f,
                turnRate = 130f,
                driftBaseTurnRate = 58f,
                maxDurabilityHits = 3,
                engineBasePitch = 1.0f
            });

            // AKUMA 86
            carCatalog.cars.Add(new CarDefinition
            {
                id = "akuma_86",
                displayName = "AKUMA 86",
                archetype = "AGILE & LIGHTWEIGHT TOUGE",
                priceNC = 3000,
                carSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_VEHICLES + "car_akuma_86.png"),
                baseTopSpeed = 18.5f,
                baseAcceleration = 16f,
                turnRate = 145f,
                driftBaseTurnRate = 65f,
                maxDurabilityHits = 3,
                engineBasePitch = 1.08f
            });

            // RYUJIN RX
            carCatalog.cars.Add(new CarDefinition
            {
                id = "ryujin_rx",
                displayName = "RYUJIN RX",
                archetype = "ROTARY DRIFT SPECIALIST",
                priceNC = 6000,
                carSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_VEHICLES + "car_ryujin_rx.png"),
                baseTopSpeed = 20f,
                baseAcceleration = 15.5f,
                turnRate = 135f,
                driftBaseTurnRate = 72f,
                maxDurabilityHits = 3,
                engineBasePitch = 1.15f
            });

            // KAGE R34
            carCatalog.cars.Add(new CarDefinition
            {
                id = "kage_r34",
                displayName = "KAGE R34",
                archetype = "HIGHWAY KING (HIGH STABILITY)",
                priceNC = 9000,
                carSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_VEHICLES + "car_kage_r34.png"),
                baseTopSpeed = 21f,
                baseAcceleration = 16.5f,
                turnRate = 125f,
                driftBaseTurnRate = 54f,
                maxDurabilityHits = 4,
                engineBasePitch = 0.95f
            });

            // RAIDEN GT
            carCatalog.cars.Add(new CarDefinition
            {
                id = "raiden_gt",
                displayName = "RAIDEN GT",
                archetype = "TURBO CHARGED ACCEL & BOOST",
                priceNC = 12000,
                carSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_VEHICLES + "car_raiden_gt.png"),
                baseTopSpeed = 21.5f,
                baseAcceleration = 18f,
                turnRate = 128f,
                driftBaseTurnRate = 56f,
                boostMultiplier = 1.6f,
                maxDurabilityHits = 4,
                engineBasePitch = 1.05f
            });

            // ONI X
            carCatalog.cars.Add(new CarDefinition
            {
                id = "oni_x",
                displayName = "ONI X",
                archetype = "DEMON HYPERCAR (MAX SPEED)",
                priceNC = 15000,
                carSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ART_VEHICLES + "car_oni_x.png"),
                baseTopSpeed = 23.5f,
                baseAcceleration = 18.5f,
                turnRate = 120f,
                driftBaseTurnRate = 60f,
                maxDurabilityHits = 5,
                engineBasePitch = 1.25f
            });

            EditorUtility.SetDirty(carCatalog);

            // 2. Customization Catalog
            string custCatPath = DATA_DIR + "CustomizationCatalog.asset";
            var custCatalog = AssetDatabase.LoadAssetAtPath<CustomizationCatalog>(custCatPath);
            if (custCatalog == null)
            {
                custCatalog = ScriptableObject.CreateInstance<CustomizationCatalog>();
                AssetDatabase.CreateAsset(custCatalog, custCatPath);
            }

            custCatalog.paints.Clear();
            custCatalog.paints.Add(new CustomizationItem { id = "paint_0", displayName = "Factory White", category = CustomizationCategory.PaintColor, priceNC = 0, primaryColor = Color.white });
            custCatalog.paints.Add(new CustomizationItem { id = "paint_1", displayName = "Neon Magenta", category = CustomizationCategory.PaintColor, priceNC = 350, primaryColor = new Color(1f, 0.1f, 0.6f) });
            custCatalog.paints.Add(new CustomizationItem { id = "paint_2", displayName = "Cyber Cyan", category = CustomizationCategory.PaintColor, priceNC = 350, primaryColor = new Color(0.1f, 0.9f, 1f) });
            custCatalog.paints.Add(new CustomizationItem { id = "paint_3", displayName = "Midnight Gold", category = CustomizationCategory.PaintColor, priceNC = 500, primaryColor = new Color(1f, 0.8f, 0.1f) });

            custCatalog.underglows.Clear();
            custCatalog.underglows.Add(new CustomizationItem { id = "underglow_0", displayName = "Cyan Neon", category = CustomizationCategory.UnderglowNeon, priceNC = 0, emissionColor = Color.cyan });
            custCatalog.underglows.Add(new CustomizationItem { id = "underglow_1", displayName = "Magenta Glow", category = CustomizationCategory.UnderglowNeon, priceNC = 800, emissionColor = Color.magenta });
            custCatalog.underglows.Add(new CustomizationItem { id = "underglow_2", displayName = "Acid Lime", category = CustomizationCategory.UnderglowNeon, priceNC = 1000, emissionColor = Color.green });

            EditorUtility.SetDirty(custCatalog);
            AssetDatabase.SaveAssets();

            // Set PlayerSettings Product Name
            PlayerSettings.productName = "NEON DRIFT.";
            Debug.Log("[BuildAssetsIntegrator] Product Name set to 'NEON DRIFT.'. Car and Customization Catalogs verified.");
        }
    }
}

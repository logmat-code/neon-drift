using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonDrift.Data
{
    [Serializable]
    public class CarDefinition
    {
        public string id;
        public string displayName;
        public string archetype; // e.g. "Agile / Accessible", "Drift Master", "Balanced Highway King"
        public int priceNC;
        public Sprite carSprite;

        [Header("Base Performance Specs")]
        public float baseTopSpeed = 19f;
        public float baseAcceleration = 15f;
        public float brakeDeceleration = 24f;
        public float turnRate = 130f;
        public float driftBaseTurnRate = 58f;
        public float maxDriftSlipAngle = 48f;
        public float minDriftSlipAngle = 14f;
        public float driftSpeedRetention = 0.95f;
        public float boostMultiplier = 1.4f;
        public float boostDuration = 2.8f;
        public int maxDurabilityHits = 3;

        [Header("Audio Customization")]
        public float engineBasePitch = 1.0f;
    }

    [CreateAssetMenu(fileName = "CarCatalog", menuName = "NeonDrift/Car Catalog")]
    public class CarCatalog : ScriptableObject
    {
        public List<CarDefinition> cars = new List<CarDefinition>();

        public CarDefinition GetCar(string id)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                if (cars[i].id == id) return cars[i];
            }
            return cars.Count > 0 ? cars[0] : null;
        }
    }
}

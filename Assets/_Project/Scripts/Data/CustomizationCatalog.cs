using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonDrift.Data
{
    public enum CustomizationCategory
    {
        PaintColor,
        UnderglowNeon,
        TireSmoke,
        Rims
    }

    [Serializable]
    public class CustomizationItem
    {
        public string id;
        public string displayName;
        public CustomizationCategory category;
        public int priceNC;
        public Color primaryColor = Color.white;
        public Color emissionColor = Color.cyan;
    }

    [CreateAssetMenu(fileName = "CustomizationCatalog", menuName = "NeonDrift/Customization Catalog")]
    public class CustomizationCatalog : ScriptableObject
    {
        public List<CustomizationItem> paints = new List<CustomizationItem>();
        public List<CustomizationItem> underglows = new List<CustomizationItem>();
        public List<CustomizationItem> smokes = new List<CustomizationItem>();

        public CustomizationItem GetItem(string id)
        {
            foreach (var item in paints) if (item.id == id) return item;
            foreach (var item in underglows) if (item.id == id) return item;
            foreach (var item in smokes) if (item.id == id) return item;
            return null;
        }
    }
}

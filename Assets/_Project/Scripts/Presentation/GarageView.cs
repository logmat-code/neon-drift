using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Progression;
using NeonDrift.Data;

namespace NeonDrift.Presentation
{
    public class GarageView : MonoBehaviour
    {
        [Header("Header Info")]
        [SerializeField] private Text creditsText;
        [SerializeField] private Text carNameText;
        [SerializeField] private Text carArchetypeText;
        [SerializeField] private Image carPreviewImage;

        [Header("Vehicle Selector")]
        [SerializeField] private Button prevCarBtn;
        [SerializeField] private Button nextCarBtn;
        [SerializeField] private Button buyOrEquipBtn;
        [SerializeField] private Text buyOrEquipBtnText;

        [Header("Upgrade Buttons & Levels (6 Categories)")]
        [SerializeField] private Button upgradeEngineBtn;
        [SerializeField] private Text engineLvlText;
        [SerializeField] private Text engineCostText;

        [SerializeField] private Button upgradeTurboBtn;
        [SerializeField] private Text turboLvlText;
        [SerializeField] private Text turboCostText;

        [SerializeField] private Button upgradeSteerBtn;
        [SerializeField] private Text steerLvlText;
        [SerializeField] private Text steerCostText;

        [SerializeField] private Button upgradeDriftBtn;
        [SerializeField] private Text driftLvlText;
        [SerializeField] private Text driftCostText;

        [SerializeField] private Button upgradeBrakesBtn;
        [SerializeField] private Text brakesLvlText;
        [SerializeField] private Text brakesCostText;

        [SerializeField] private Button upgradeDurabilityBtn;
        [SerializeField] private Text durabilityLvlText;
        [SerializeField] private Text durabilityCostText;

        [Header("Monetization & Action Buttons")]
        [SerializeField] private Button watchAdBonusBtn;
        [SerializeField] private Button backBtn;
        [SerializeField] private Button driveBtn;

        public event Action OnBackClicked;
        public event Action OnDriveClicked;

        private int viewingCarIndex = 0;
        private List<string> availableCarIds = new List<string> { "kitsune_s13", "akuma_86", "ryujin_rx", "kage_r34", "raiden_gt", "oni_x" };

        private void Start()
        {
            if (prevCarBtn != null) prevCarBtn.onClick.AddListener(SelectPreviousCar);
            if (nextCarBtn != null) nextCarBtn.onClick.AddListener(SelectNextCar);
            if (buyOrEquipBtn != null) buyOrEquipBtn.onClick.AddListener(OnBuyOrEquipClicked);

            if (upgradeEngineBtn != null) upgradeEngineBtn.onClick.AddListener(() => TryUpgrade("engine"));
            if (upgradeTurboBtn != null) upgradeTurboBtn.onClick.AddListener(() => TryUpgrade("turbo"));
            if (upgradeSteerBtn != null) upgradeSteerBtn.onClick.AddListener(() => TryUpgrade("steering"));
            if (upgradeDriftBtn != null) upgradeDriftBtn.onClick.AddListener(() => TryUpgrade("drift"));
            if (upgradeBrakesBtn != null) upgradeBrakesBtn.onClick.AddListener(() => TryUpgrade("brakes"));
            if (upgradeDurabilityBtn != null) upgradeDurabilityBtn.onClick.AddListener(() => TryUpgrade("durability"));

            if (watchAdBonusBtn != null) watchAdBonusBtn.onClick.AddListener(OnWatchAdClicked);
            if (backBtn != null) backBtn.onClick.AddListener(() => OnBackClicked?.Invoke());
            if (driveBtn != null) driveBtn.onClick.AddListener(() => OnDriveClicked?.Invoke());

            RefreshUI();
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        public void SelectPreviousCar()
        {
            viewingCarIndex = (viewingCarIndex - 1 + availableCarIds.Count) % availableCarIds.Count;
            RefreshUI();
        }

        public void SelectNextCar()
        {
            viewingCarIndex = (viewingCarIndex + 1) % availableCarIds.Count;
            RefreshUI();
        }

        private void OnBuyOrEquipClicked()
        {
            string carId = availableCarIds[viewingCarIndex];
            var pm = ProgressionManager.Instance;
            if (pm == null) return;

            var vData = pm.CurrentData.GetVehicleData(carId);
            if (vData.isUnlocked)
            {
                pm.TryEquipCar(carId);
            }
            else
            {
                pm.TryBuyCar(carId);
            }
            RefreshUI();
        }

        private void TryUpgrade(string category)
        {
            string carId = availableCarIds[viewingCarIndex];
            var pm = ProgressionManager.Instance;
            if (pm != null && pm.TryUpgradeCategory(carId, category))
            {
                AudioManager.Instance?.PlayUIClick();
                RefreshUI();
            }
        }

        private void OnWatchAdClicked()
        {
            Monetization.Yodo1AdsManager.Instance?.ShowGarageBonusRewardedAd((bonus) =>
            {
                AudioManager.Instance?.PlayUIClick();
                RefreshUI();
            });
        }

        public void RefreshUI()
        {
            var pm = ProgressionManager.Instance;
            if (pm == null || pm.CurrentData == null) return;

            if (creditsText != null) creditsText.text = $"{pm.CurrentData.credits:N0} NC";

            string currentId = availableCarIds[Mathf.Clamp(viewingCarIndex, 0, availableCarIds.Count - 1)];
            var carDef = pm.CarCatalog?.GetCar(currentId);
            var vData = pm.CurrentData.GetVehicleData(currentId);

            if (carNameText != null) carNameText.text = carDef != null ? carDef.displayName.ToUpper() : currentId.ToUpper();
            if (carArchetypeText != null) carArchetypeText.text = carDef != null ? carDef.archetype : "STREET DRIFTER";
            if (carPreviewImage != null && carDef != null && carDef.carSprite != null) carPreviewImage.sprite = carDef.carSprite;

            // Buy or Equip button state
            if (buyOrEquipBtnText != null)
            {
                if (currentId == pm.CurrentData.selectedVehicleId)
                {
                    buyOrEquipBtnText.text = "EQUIPPED";
                    if (buyOrEquipBtn != null) buyOrEquipBtn.interactable = false;
                }
                else if (vData.isUnlocked)
                {
                    buyOrEquipBtnText.text = "EQUIP CAR";
                    if (buyOrEquipBtn != null) buyOrEquipBtn.interactable = true;
                }
                else
                {
                    int price = carDef != null ? carDef.priceNC : 3000;
                    buyOrEquipBtnText.text = $"UNLOCK ({price:N0} NC)";
                    if (buyOrEquipBtn != null) buyOrEquipBtn.interactable = pm.CurrentData.credits >= price;
                }
            }

            // Upgrades stats display
            UpdateUpgradeSlot(vData.engineLevel, engineLvlText, engineCostText, upgradeEngineBtn, pm);
            UpdateUpgradeSlot(vData.turboLevel, turboLvlText, turboCostText, upgradeTurboBtn, pm);
            UpdateUpgradeSlot(vData.steeringLevel, steerLvlText, steerCostText, upgradeSteerBtn, pm);
            UpdateUpgradeSlot(vData.driftControlLevel, driftLvlText, driftCostText, upgradeDriftBtn, pm);
            UpdateUpgradeSlot(vData.brakesLevel, brakesLvlText, brakesCostText, upgradeBrakesBtn, pm);
            UpdateUpgradeSlot(vData.durabilityLevel, durabilityLvlText, durabilityCostText, upgradeDurabilityBtn, pm);
        }

        private void UpdateUpgradeSlot(int level, Text lvlText, Text costText, Button btn, ProgressionManager pm)
        {
            if (lvlText != null) lvlText.text = $"LVL {level}/5";
            int cost = pm.GetUpgradeCost(level);
            if (costText != null) costText.text = cost > 0 ? $"{cost} NC" : "MAX";
            if (btn != null) btn.interactable = cost > 0 && pm.CurrentData.credits >= cost;
        }
    }
}

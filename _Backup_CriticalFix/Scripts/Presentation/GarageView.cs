using System;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Progression;
using NeonDrift.Data;

namespace NeonDrift.Presentation
{
    public class GarageView : MonoBehaviour
    {
        [Header("Profile & Credits")]
        [SerializeField] private Text creditsText;

        [Header("Stat Sliders / Bars")]
        [SerializeField] private Image speedBar;
        [SerializeField] private Image accelBar;
        [SerializeField] private Image gripBar;
        [SerializeField] private Image driftBar;
        [SerializeField] private Image boostBar;

        [Header("Upgrade Buttons & Labels")]
        [SerializeField] private Button upgradeEngineBtn;
        [SerializeField] private Text engineLvlText;
        [SerializeField] private Text engineCostText;

        [SerializeField] private Button upgradeTiresBtn;
        [SerializeField] private Text tiresLvlText;
        [SerializeField] private Text tiresCostText;

        [SerializeField] private Button upgradeTurboBtn;
        [SerializeField] private Text turboLvlText;
        [SerializeField] private Text turboCostText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button backBtn;
        [SerializeField] private Button driveBtn;

        [Header("Config")]
        [SerializeField] private UpgradeConfig upgradeConfig;

        public event Action OnBackClicked;
        public event Action OnDriveClicked;

        private void Start()
        {
            if (backBtn != null) backBtn.onClick.AddListener(() => OnBackClicked?.Invoke());
            if (driveBtn != null) driveBtn.onClick.AddListener(() => OnDriveClicked?.Invoke());

            if (upgradeEngineBtn != null) upgradeEngineBtn.onClick.AddListener(HandleUpgradeEngine);
            if (upgradeTiresBtn != null) upgradeTiresBtn.onClick.AddListener(HandleUpgradeTires);
            if (upgradeTurboBtn != null) upgradeTurboBtn.onClick.AddListener(HandleUpgradeTurbo);

            RefreshUI();
        }

        public void RefreshUI()
        {
            if (ProgressionManager.Instance == null || ProgressionManager.Instance.CurrentData == null) return;
            var data = ProgressionManager.Instance.CurrentData;

            if (creditsText != null) creditsText.text = $"CREDITS: {data.credits:N0}";

            // Stat bars (normalized 0..1)
            if (speedBar != null) speedBar.fillAmount = Mathf.Clamp01(0.4f + data.engineLevel * 0.12f);
            if (accelBar != null) accelBar.fillAmount = Mathf.Clamp01(0.45f + data.engineLevel * 0.11f);
            if (gripBar != null) gripBar.fillAmount = Mathf.Clamp01(0.5f + data.tiresLevel * 0.10f);
            if (driftBar != null) driftBar.fillAmount = Mathf.Clamp01(0.55f + data.tiresLevel * 0.09f);
            if (boostBar != null) boostBar.fillAmount = Mathf.Clamp01(0.35f + data.turboLevel * 0.13f);

            // Engine
            if (engineLvlText != null) engineLvlText.text = $"ENGINE LV.{data.engineLevel}";
            int eCost = upgradeConfig != null ? upgradeConfig.GetEngineCost(data.engineLevel) : -1;
            if (engineCostText != null) engineCostText.text = eCost > 0 ? $"CR {eCost}" : "MAX";
            if (upgradeEngineBtn != null) upgradeEngineBtn.interactable = (eCost > 0 && data.credits >= eCost);

            // Tires
            if (tiresLvlText != null) tiresLvlText.text = $"TIRES LV.{data.tiresLevel}";
            int tCost = upgradeConfig != null ? upgradeConfig.GetTiresCost(data.tiresLevel) : -1;
            if (tiresCostText != null) tiresCostText.text = tCost > 0 ? $"CR {tCost}" : "MAX";
            if (upgradeTiresBtn != null) upgradeTiresBtn.interactable = (tCost > 0 && data.credits >= tCost);

            // Turbo
            if (turboLvlText != null) turboLvlText.text = $"TURBO LV.{data.turboLevel}";
            int tbCost = upgradeConfig != null ? upgradeConfig.GetTurboCost(data.turboLevel) : -1;
            if (turboCostText != null) turboCostText.text = tbCost > 0 ? $"CR {tbCost}" : "MAX";
            if (upgradeTurboBtn != null) upgradeTurboBtn.interactable = (tbCost > 0 && data.credits >= tbCost);
        }

        private void HandleUpgradeEngine()
        {
            if (ProgressionManager.Instance.TryUpgradeEngine())
            {
                AudioManager.Instance?.PlayUiClick();
                RefreshUI();
            }
        }

        private void HandleUpgradeTires()
        {
            if (ProgressionManager.Instance.TryUpgradeTires())
            {
                AudioManager.Instance?.PlayUiClick();
                RefreshUI();
            }
        }

        private void HandleUpgradeTurbo()
        {
            if (ProgressionManager.Instance.TryUpgradeTurbo())
            {
                AudioManager.Instance?.PlayUiClick();
                RefreshUI();
            }
        }
    }
}

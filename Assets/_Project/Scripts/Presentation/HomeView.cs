using System;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Progression;

namespace NeonDrift.Presentation
{
    public class HomeView : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button driveButton;
        [SerializeField] private Button garageButton;
        [SerializeField] private Button settingsButton;

        [Header("Profile Labels")]
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text creditsText;

        public event Action OnDriveClicked;
        public event Action OnGarageClicked;
        public event Action OnSettingsClicked;

        private void Start()
        {
            if (driveButton != null) driveButton.onClick.AddListener(() => OnDriveClicked?.Invoke());
            if (garageButton != null) garageButton.onClick.AddListener(() => OnGarageClicked?.Invoke());
            if (settingsButton != null) settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());

            RefreshProfile();
        }

        public void RefreshProfile()
        {
            if (ProgressionManager.Instance != null && ProgressionManager.Instance.CurrentData != null)
            {
                var data = ProgressionManager.Instance.CurrentData;
                if (bestScoreText != null) bestScoreText.text = $"BEST: {data.bestScore:N0}";
                if (creditsText != null) creditsText.text = $"CR: {data.credits:N0}";
            }
        }
    }
}

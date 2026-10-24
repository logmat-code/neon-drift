using System;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Monetization;
using NeonDrift.Social;
using NeonDrift.Progression;

namespace NeonDrift.Presentation
{
    public class ResultsView : MonoBehaviour
    {
        [Header("Stat Labels")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text longestDriftText;
        [SerializeField] private Text maxComboText;
        [SerializeField] private Text nearMissesText;
        [SerializeField] private Text creditsEarnedText;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button garageButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button doubleCreditsButton;
        [SerializeField] private Button shareButton;

        public event Action OnRetryClicked;
        public event Action OnGarageClicked;
        public event Action OnHomeClicked;

        private int lastEarnedNC = 0;
        private int lastScore = 0;
        private float lastDist = 0f;
        private int lastCombo = 0;
        private bool doubled = false;

        private void Start()
        {
            if (retryButton != null) retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
            if (garageButton != null) garageButton.onClick.AddListener(() => OnGarageClicked?.Invoke());
            if (homeButton != null) homeButton.onClick.AddListener(() => OnHomeClicked?.Invoke());
            if (doubleCreditsButton != null) doubleCreditsButton.onClick.AddListener(OnDoubleCreditsClicked);
            if (shareButton != null) shareButton.onClick.AddListener(OnShareClicked);
        }

        public void DisplayResults(string title, int finalScore, int bestScore, float longestDrift, int maxCombo, int nearMisses, int creditsEarned, float distance = 0f)
        {
            lastEarnedNC = creditsEarned;
            lastScore = finalScore;
            lastDist = distance;
            lastCombo = maxCombo;
            doubled = false;

            if (titleText != null) titleText.text = title;
            if (finalScoreText != null) finalScoreText.text = $"SCORE: {finalScore:N0}";
            if (bestScoreText != null) bestScoreText.text = $"BEST: {bestScore:N0}";
            if (longestDriftText != null) longestDriftText.text = $"LONGEST DRIFT: {longestDrift:F1}s";
            if (maxComboText != null) maxComboText.text = $"MAX COMBO: x{maxCombo}";
            if (nearMissesText != null) nearMissesText.text = $"NEAR MISSES: {nearMisses}";
            if (creditsEarnedText != null) creditsEarnedText.text = $"+{creditsEarned:N0} NC";

            if (doubleCreditsButton != null)
            {
                doubleCreditsButton.gameObject.SetActive(true);
                doubleCreditsButton.interactable = true;
            }
        }

        private void OnDoubleCreditsClicked()
        {
            if (doubled) return;
            Yodo1AdsManager.Instance?.ShowDoubleCreditsRewardedAd(lastEarnedNC, (bonus) =>
            {
                doubled = true;
                if (creditsEarnedText != null) creditsEarnedText.text = $"+{lastEarnedNC * 2:N0} NC (DOUBLED!)";
                if (doubleCreditsButton != null) doubleCreditsButton.interactable = false;
            });
        }

        private void OnShareClicked()
        {
            string car = ProgressionManager.Instance?.GetEquippedCar()?.displayName ?? "KITSUNE S13";
            SocialShareManager.Instance?.ShareRunScore(car, lastScore, lastDist, lastCombo, lastEarnedNC);
        }
    }
}

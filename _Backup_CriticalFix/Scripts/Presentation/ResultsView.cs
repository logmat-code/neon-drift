using System;
using UnityEngine;
using UnityEngine.UI;

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

        public event Action OnRetryClicked;
        public event Action OnGarageClicked;

        private void Start()
        {
            if (retryButton != null) retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
            if (garageButton != null) garageButton.onClick.AddListener(() => OnGarageClicked?.Invoke());
        }

        public void DisplayResults(string title, int finalScore, int bestScore, float longestDrift, int maxCombo, int nearMisses, int creditsEarned)
        {
            if (titleText != null) titleText.text = title;
            if (finalScoreText != null) finalScoreText.text = $"SCORE: {finalScore:N0}";
            if (bestScoreText != null) bestScoreText.text = $"BEST: {bestScore:N0}";
            if (longestDriftText != null) longestDriftText.text = $"LONGEST DRIFT: {longestDrift:F1}s";
            if (maxComboText != null) maxComboText.text = $"MAX COMBO: x{maxCombo}";
            if (nearMissesText != null) nearMissesText.text = $"NEAR MISSES: {nearMisses}";
            if (creditsEarnedText != null) creditsEarnedText.text = $"+{creditsEarned:N0} CR";
        }
    }
}

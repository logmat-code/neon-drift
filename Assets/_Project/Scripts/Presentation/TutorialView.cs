using System;
using UnityEngine;
using UnityEngine.UI;
using NeonDrift.Tutorial;

namespace NeonDrift.Presentation
{
    public class TutorialView : MonoBehaviour
    {
        [Header("Tutorial Card")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text instructionText;
        [SerializeField] private Text hintText;

        [Header("Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;

        public event Action OnTutorialCompletedOrSkipped;

        private void Start()
        {
            if (nextButton != null) nextButton.onClick.AddListener(OnNextClicked);
            if (skipButton != null) skipButton.onClick.AddListener(OnSkipClicked);

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnStepChanged += UpdateStepUI;
                UpdateStepUI(TutorialManager.Instance.currentStep);
            }
        }

        private void OnDestroy()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnStepChanged -= UpdateStepUI;
            }
        }

        public void DisplayStep(TutorialManager.TutorialStep step)
        {
            UpdateStepUI(step);
        }

        private void UpdateStepUI(TutorialManager.TutorialStep step)
        {
            if (step == TutorialManager.TutorialStep.None || step == TutorialManager.TutorialStep.Completed)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            switch (step)
            {
                case TutorialManager.TutorialStep.Steering:
                    if (titleText != null) titleText.text = "PHASE 1: STEERING";
                    if (instructionText != null) instructionText.text = "Tap LEFT or RIGHT to navigate through highway lanes.";
                    if (hintText != null) hintText.text = "Keep smooth rhythm between curbs to avoid speed penalties.";
                    break;

                case TutorialManager.TutorialStep.Drift:
                    if (titleText != null) titleText.text = "PHASE 2: ARCADE DRIFT";
                    if (instructionText != null) instructionText.text = "Hold DRIFT while steering into a turn to break traction.";
                    if (hintText != null) hintText.text = "Release DRIFT to straighten out and cash in your Flow combo!";
                    break;

                case TutorialManager.TutorialStep.Boost:
                    if (titleText != null) titleText.text = "PHASE 3: CYBER BOOST";
                    if (instructionText != null) instructionText.text = "Press BOOST to surge forward at supersonic velocity.";
                    if (hintText != null) hintText.text = "Build boost meter by executing clean, continuous high-angle drifts.";
                    break;

                case TutorialManager.TutorialStep.TrafficAndNearMiss:
                    if (titleText != null) titleText.text = "PHASE 4: TRAFFIC & DANGER";
                    if (instructionText != null) instructionText.text = "Weave closely past oncoming civilian cars for NEAR MISS bonuses.";
                    if (hintText != null) hintText.text = "Thread through tight gaps between vehicles to earn extra NC!";
                    break;

                case TutorialManager.TutorialStep.NeonCreditsAndGarage:
                    if (titleText != null) titleText.text = "PHASE 5: REWARDS & SHOWROOM";
                    if (instructionText != null) instructionText.text = "Earn NEON CREDITS (NC) to unlock new cars and 5-tier upgrades.";
                    if (hintText != null) hintText.text = "Complete FIRST DRIVE now to claim your +500 NC starter bounty!";
                    break;
            }
        }

        private void OnNextClicked()
        {
            TutorialManager.Instance?.AdvanceStep();
            if (TutorialManager.Instance != null && !TutorialManager.Instance.isTutorialActive)
            {
                OnTutorialCompletedOrSkipped?.Invoke();
            }
        }

        private void OnSkipClicked()
        {
            TutorialManager.Instance?.SkipTutorial();
            OnTutorialCompletedOrSkipped?.Invoke();
        }
    }
}

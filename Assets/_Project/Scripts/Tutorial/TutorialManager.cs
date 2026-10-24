using System;
using UnityEngine;
using NeonDrift.Progression;

namespace NeonDrift.Tutorial
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        public enum TutorialStep
        {
            None,
            Steering,
            Drift,
            Boost,
            TrafficAndNearMiss,
            NeonCreditsAndGarage,
            Completed
        }

        public TutorialStep currentStep = TutorialStep.None;
        public bool isTutorialActive => currentStep != TutorialStep.None && currentStep != TutorialStep.Completed;

        public event Action<TutorialStep> OnStepChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool ShouldStartFirstDrive()
        {
            var data = ProgressionManager.Instance?.CurrentData;
            if (data == null) return false;
            return !data.tutorialCompleted && !data.tutorialSkipped;
        }

        public void StartTutorial()
        {
            currentStep = TutorialStep.Steering;
            OnStepChanged?.Invoke(currentStep);
            Debug.Log("[Tutorial] FIRST DRIVE started: Step 1 - Steering (Left / Right)");
        }

        public void AdvanceStep()
        {
            currentStep = currentStep switch
            {
                TutorialStep.Steering => TutorialStep.Drift,
                TutorialStep.Drift => TutorialStep.Boost,
                TutorialStep.Boost => TutorialStep.TrafficAndNearMiss,
                TutorialStep.TrafficAndNearMiss => TutorialStep.NeonCreditsAndGarage,
                TutorialStep.NeonCreditsAndGarage => TutorialStep.Completed,
                _ => TutorialStep.Completed
            };

            if (currentStep == TutorialStep.Completed)
            {
                CompleteTutorial();
            }
            else
            {
                OnStepChanged?.Invoke(currentStep);
            }
        }

        public void SkipTutorial()
        {
            currentStep = TutorialStep.Completed;
            var data = ProgressionManager.Instance?.CurrentData;
            if (data != null)
            {
                data.tutorialSkipped = true;
                ProgressionManager.Instance.SaveProfile();
            }
            OnStepChanged?.Invoke(currentStep);
            Debug.Log("[Tutorial] Tutorial skipped by player.");
        }

        public void CompleteTutorial()
        {
            currentStep = TutorialStep.Completed;
            var data = ProgressionManager.Instance?.CurrentData;
            if (data != null && !data.tutorialCompleted)
            {
                data.tutorialCompleted = true;
                ProgressionManager.Instance.RewardRewardedAdNC(500); // 500 NC First Drive bonus
                ProgressionManager.Instance.SaveProfile();
                Debug.Log("[Tutorial] FIRST DRIVE Completed! +500 NC awarded.");
            }
            OnStepChanged?.Invoke(currentStep);
        }
    }
}

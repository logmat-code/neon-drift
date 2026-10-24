using System;
using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.Gameplay
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private FlowManager flowManager;
        [SerializeField] private VehicleController vehicle;

        public int currentScore = 0;
        public float longestDrift = 0f;
        public int maxCombo = 0;
        public int nearMissCount = 0;
        public int gapsCount = 0;

        public event Action<int, int, string> OnScoreAdded; // currentTotal, addedScore, actionLabel

        private void Start()
        {
            if (flowManager != null)
                Subscribe(flowManager);
        }

        public void Initialize(FlowManager fm, VehicleController vc)
        {
            flowManager = fm;
            vehicle = vc;
            Subscribe(flowManager);
        }

        private void Subscribe(FlowManager fm)
        {
            fm.OnSkillAction += HandleSkillAction;
        }

        private void Update()
        {
            if (vehicle != null && vehicle.currentState == DriftState.Drifting)
            {
                if (vehicle.currentDriftDuration > longestDrift)
                {
                    longestDrift = vehicle.currentDriftDuration;
                }
            }

            if (flowManager != null && flowManager.currentCombo > maxCombo)
            {
                maxCombo = flowManager.currentCombo;
            }
        }

        private void HandleSkillAction(string label, int baseScore, int multiplier)
        {
            int points = baseScore * multiplier;
            currentScore += points;

            if (label.Contains("NEAR MISS"))
                nearMissCount++;
            if (label.Contains("GAP"))
                gapsCount++;

            OnScoreAdded?.Invoke(currentScore, points, label);
        }

        public void ResetSession()
        {
            currentScore = 0;
            longestDrift = 0f;
            maxCombo = 0;
            nearMissCount = 0;
            gapsCount = 0;
        }
    }
}

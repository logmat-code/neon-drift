using System;
using UnityEngine;
using NeonDrift.Data;
using NeonDrift.Vehicle;

namespace NeonDrift.Gameplay
{
    public class FlowManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private FlowConfig config;
        [SerializeField] private VehicleController vehicle;

        [Header("Runtime State")]
        public float currentFlow = 0f;
        public int currentMultiplier = 1;
        public int currentMultiplierIndex = 0;
        public int currentCombo = 0;

        public event Action<float, int> OnFlowChanged; // param: currentFlow, currentMultiplier
        public event Action<int> OnMultiplierChanged; // param: new multiplier (1, 2, 3, 5, 8)
        public event Action<string, int, int> OnSkillAction; // param: actionLabel, baseScore, multiplier

        [Header("Drift Distance & Milestones")]
        public float currentDriftDistance = 0f;
        private int driftTierAwarded = 0;
        private bool highSpeedDriftAwarded = false;

        private float driftTimeAccumulator = 0f;

        private void Start()
        {
            if (vehicle != null)
                SubscribeToVehicle(vehicle);
        }

        public void Initialize(FlowConfig flowConfig, VehicleController vehicleController)
        {
            config = flowConfig;
            vehicle = vehicleController;
            SubscribeToVehicle(vehicle);
        }

        private void SubscribeToVehicle(VehicleController vc)
        {
            vc.OnDriftTick += HandleDriftTick;
            vc.OnLongDriftAchieved += HandleLongDrift;
            vc.OnPerfectExit += HandlePerfectExit;
            vc.OnDriftChain += HandleDriftChain;
            vc.OnSpinout += HandleSpinout;
            vc.OnImpact += HandleImpact;
        }

        private void Update()
        {
            // Reset drift distance metrics when not actively drifting
            if (vehicle != null && vehicle.currentState != DriftState.Drifting)
            {
                currentDriftDistance = 0f;
                driftTierAwarded = 0;
                highSpeedDriftAwarded = false;
            }

            // Flow decays naturally when no drift or high speed action is sustaining it
            if (vehicle != null && vehicle.currentState != DriftState.Drifting && !vehicle.isBoosting)
            {
                if (currentFlow > 0f)
                {
                    currentFlow = Mathf.Max(0f, currentFlow - config.flowDecayRate * Time.deltaTime);
                    UpdateMultiplier();
                    OnFlowChanged?.Invoke(currentFlow, currentMultiplier);
                }
            }

            // High speed flow generation
            if (vehicle != null && vehicle.currentSpeed > vehicle.Config.baseTopSpeed * 0.9f)
            {
                AddFlow(config.flowGainHighSpeedPerSec * Time.deltaTime, false);
            }
        }

        public void AddFlow(float amount, bool notifyAction = false, string actionLabel = "", int baseScore = 0)
        {
            currentFlow = Mathf.Clamp(currentFlow + amount, 0f, config.maxFlow);
            if (vehicle != null)
            {
                vehicle.AddBoostEnergy(amount * config.boostEnergyPerFlowGain);
            }

            UpdateMultiplier();
            OnFlowChanged?.Invoke(currentFlow, currentMultiplier);

            if (notifyAction && !string.IsNullOrEmpty(actionLabel))
            {
                currentCombo++;
                OnSkillAction?.Invoke(actionLabel, baseScore, currentMultiplier);
            }
        }

        private void UpdateMultiplier()
        {
            int oldMultiplier = currentMultiplier;
            int newIndex = 0;

            for (int i = config.flowThresholdsForMultiplier.Length - 1; i >= 0; i--)
            {
                if (currentFlow >= config.flowThresholdsForMultiplier[i])
                {
                    newIndex = i;
                    break;
                }
            }

            currentMultiplierIndex = newIndex;
            currentMultiplier = config.multipliers[newIndex];

            if (currentMultiplier != oldMultiplier)
            {
                OnMultiplierChanged?.Invoke(currentMultiplier);
            }
        }

        private void HandleDriftTick(float duration)
        {
            if (vehicle == null) return;

            float stepDist = vehicle.currentSpeed * Time.fixedDeltaTime;
            currentDriftDistance += stepDist;

            // Tiered drift distance milestones:
            // Tier 1 (25m) -> DRIFT
            // Tier 2 (60m) -> LONG DRIFT
            // Tier 3 (110m) -> GREAT DRIFT
            // Tier 4 (175m) -> INSANE DRIFT
            if (currentDriftDistance >= 175f && driftTierAwarded < 4)
            {
                driftTierAwarded = 4;
                AddFlow(35f, true, "INSANE DRIFT", 1500);
            }
            else if (currentDriftDistance >= 110f && driftTierAwarded < 3)
            {
                driftTierAwarded = 3;
                AddFlow(25f, true, "GREAT DRIFT", 700);
            }
            else if (currentDriftDistance >= 60f && driftTierAwarded < 2)
            {
                driftTierAwarded = 2;
                AddFlow(20f, true, "LONG DRIFT", 350);
            }
            else if (currentDriftDistance >= 25f && driftTierAwarded < 1)
            {
                driftTierAwarded = 1;
                AddFlow(12f, true, "DRIFT", 150);
            }

            // High speed drift bonus
            if (!highSpeedDriftAwarded && vehicle.currentSpeed > vehicle.Config.baseTopSpeed * 0.90f)
            {
                highSpeedDriftAwarded = true;
                AddFlow(15f, true, "HIGH SPEED DRIFT", 250);
            }

            driftTimeAccumulator += Time.fixedDeltaTime;
            if (driftTimeAccumulator >= 0.35f)
            {
                driftTimeAccumulator = 0f;
                AddFlow(config.flowGainDriftPerSec * 0.35f, false);
            }
        }

        private void HandleLongDrift()
        {
            // Handled via distance milestones
        }

        private void HandlePerfectExit()
        {
            AddFlow(config.flowGainPerfectExit, true, "PERFECT EXIT", config.scorePerfectExit);
        }

        private void HandleDriftChain(int chainCount)
        {
            if (chainCount > 1)
            {
                // Heavily reward chained drifts: escalating flow gains propel multipliers rapidly
                float chainFlowBonus = config.flowGainDriftChain * chainCount * 1.3f;
                int chainScoreBonus = config.scoreDriftChain * chainCount;
                AddFlow(chainFlowBonus, true, $"CHAIN x{chainCount}", chainScoreBonus);
            }
        }

        private void HandleSpinout()
        {
            currentFlow = Mathf.Max(0f, currentFlow * 0.4f);
            currentCombo = 0;
            UpdateMultiplier();
            OnFlowChanged?.Invoke(currentFlow, currentMultiplier);
        }

        private void HandleImpact(VehicleController.CollisionSeverity severity)
        {
            float penalty = severity switch
            {
                VehicleController.CollisionSeverity.WallTouch => config.flowPenaltyWallTouch,
                VehicleController.CollisionSeverity.TrafficHit => config.flowPenaltyTrafficHit,
                VehicleController.CollisionSeverity.HeavyCrash => config.flowPenaltyHeavyCrash,
                _ => 15f
            };

            currentFlow = Mathf.Max(0f, currentFlow - penalty);
            currentCombo = 0;
            UpdateMultiplier();
            OnFlowChanged?.Invoke(currentFlow, currentMultiplier);
        }

        // External hooks for Near Miss and Gap
        public void RegisterNearMiss(bool duringDrift)
        {
            if (duringDrift)
            {
                AddFlow(config.flowGainDriftNearMiss, true, "DRIFT NEAR MISS", config.scoreDriftNearMiss);
            }
            else
            {
                AddFlow(config.flowGainNearMiss, true, "NEAR MISS", config.scoreNearMiss);
            }
        }

        public void RegisterTrafficGap(bool duringDrift)
        {
            if (duringDrift)
            {
                AddFlow(config.flowGainDriftGap, true, "DRIFT GAP", config.scoreDriftGap);
            }
            else
            {
                AddFlow(config.flowGainTrafficGap, true, "TRAFFIC GAP", config.scoreTrafficGap);
            }
        }
    }
}

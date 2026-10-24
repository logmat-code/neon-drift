using UnityEngine;

namespace NeonDrift.Data
{
    [CreateAssetMenu(fileName = "CarConfig", menuName = "NeonDrift/Car Config")]
    public class CarConfig : ScriptableObject
    {
        [Header("Speed & Throttle")]
        public float baseTopSpeed = 19f;
        public float baseAcceleration = 15f;
        public float brakeDeceleration = 24f;

        [Header("Steering (Grip Mode)")]
        [Tooltip("Max turning rate in degrees per second at standard speed")]
        public float turnRate = 130f;
        [Tooltip("Steering boost at low speeds (< 6 m/s)")]
        public float lowSpeedSteerMultiplier = 1.25f;
        [Tooltip("Steering dampening at high speeds (> 16 m/s) for stability")]
        public float highSpeedSteerMultiplier = 0.85f;

        [Header("Drift Dynamics (Arcade)")]
        public float minDriftSpeed = 6.0f;
        [Tooltip("Base turning rate of trajectory in neutral drift (deg/sec)")]
        public float driftBaseTurnRate = 58f;
        [Tooltip("Minimum turning rate of trajectory when countersteering to open line (deg/sec)")]
        public float driftMinTurnRate = 28f;
        [Tooltip("Maximum turning rate of trajectory when steering in to close radius (deg/sec)")]
        public float driftMaxTurnRate = 100f;
        [Tooltip("Legacy cornering rate")]
        public float driftCorneringRate = 72f;
        [Tooltip("Base visual drift slip angle in degrees")]
        public float baseDriftSlipAngle = 28f;
        [Tooltip("Max slip angle when steering into the drift")]
        public float maxDriftSlipAngle = 48f;
        [Tooltip("Min slip angle when countersteering opposite to drift")]
        public float minDriftSlipAngle = 14f;
        [Tooltip("Rate at which slip angle transitions (deg/sec)")]
        public float slipAngleRate = 95f;
        [Tooltip("Speed retention ratio during clean neutral drift")]
        public float driftSpeedRetention = 0.95f;
        [Tooltip("Speed retention ratio during heavy / forced drift")]
        public float hardDriftSpeedRetention = 0.88f;
        [Tooltip("Time to smoothly realign heading to trajectory upon exit")]
        public float driftExitDuration = 0.20f;
        [Tooltip("Threshold of slip reduction to trigger a transition flick")]
        public float transitionFlickThreshold = 16f;

        [Header("Perfect Exit & Combos")]
        public float perfectExitMinAngle = 18f;
        public float perfectExitMaxAngle = 46f;
        public float perfectExitImpulse = 3.5f;
        public float driftChainWindow = 1.2f;

        [Header("Boost Tuning")]
        public float boostSpeedMultiplier = 1.4f;
        public float boostAccelerationMultiplier = 2.0f;
        public float boostDuration = 2.8f;
        public float maxBoostEnergy = 100f;

        // Legacy compatibility properties
        public float gripLateralFriction => 20f;
        public float driftLateralFriction => 2.8f;
        public float driftSteerRate => turnRate;
        public float driftSelfAlignStrength => 2.5f;
        public float driftEntryDuration => 0.15f;
        public float turnAcceleration => 600f;
        public float steerInputResponse => 12f;
        public float minDriftAngle => minDriftSlipAngle;
        public float maxDriftAngle => maxDriftSlipAngle;
        public float spinoutAngle => 90f;
    }
}

using System;
using UnityEngine;
using NeonDrift.Data;

namespace NeonDrift.Vehicle
{
    public enum DriftState
    {
        Grip,
        DriftEntry,
        Drifting,
        DriftExit,
        Spinout
    }

    /// <summary>
    /// Pure Arcade Vehicle Controller for NEON DRIFT.
    /// Provides deterministic, responsive, and predictable handling designed for mobile touch and keyboard.
    /// Single authority over heading, speed, and lateral slip — eliminates physics solver fights.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class VehicleController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CarConfig config;

        [Header("Input State (Digital / Filtered)")]
        public float steerInput; // -1 (Left), 0 (None), +1 (Right)
        public bool driftHeld;
        public bool boostRequested;
        public bool autoAccelerate = true;

        [Header("Runtime State")]
        public DriftState currentState = DriftState.Grip;
        public float currentSpeed;
        public float currentDriftAngle;
        public float currentDriftDuration;
        public bool isBoosting;
        public float boostEnergy = 100f;
        public float currentBoostDuration = 0f;

        // Upgrade modifiers
        private float upgradeSpeedBonus = 0f;
        private float upgradeAccelBonus = 0f;
        private float upgradeGripBonus = 0f;
        private float upgradeRecoveryBonus = 0f;
        private float upgradeBoostMultiplierBonus = 0f;
        private float upgradeBoostDurationBonus = 0f;

        // Deterministic Angles & Drift Dynamics
        public float headingAngle = 0f;       // Visual car body rotation in degrees (0 = Up)
        public float trajectoryAngle = 0f;    // Movement travel direction in degrees (0 = Up)
        public float driftDirection = 0f;     // +1 = Drifting Right, -1 = Drifting Left, 0 = None
        public float currentSlipAngle = 0f;   // Difference between trajectory and heading
        private float exitTimer = 0f;
        private float chainTimer = 0f;
        private int comboCount = 0;
        private bool longDriftAwarded = false;

        // Telemetry & Diagnostics
        public float yawRate { get; private set; } = 0f;
        public float measuredTurnRadius { get; private set; } = 999f;
        public float currentTrajectoryTurnRate { get; private set; } = 0f;

        // Mobile touch cached inputs
        private float touchSteer = 0f;
        private bool touchDrift = false;

        private Rigidbody2D rb;

        // Events
        public event Action<float> OnDriftTick;
        public event Action OnLongDriftAchieved;
        public event Action OnPerfectExit;
        public event Action<int> OnDriftChain;
        public event Action OnSpinout;
        public event Action OnBoostStarted;
        public event Action OnBoostEnded;
        public event Action<CollisionSeverity> OnImpact;

        public Rigidbody2D Rigidbody => rb;
        public CarConfig Config => config;

        public enum CollisionSeverity
        {
            WallTouch,
            TrafficHit,
            HeavyCrash
        }

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 0f;
                rb.angularDamping = 0f;
                rb.freezeRotation = true;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            boostEnergy = config != null ? config.maxBoostEnergy : 100f;
            headingAngle = transform.eulerAngles.z;
            trajectoryAngle = headingAngle;
            ResetInputs();
        }

        public void Initialize(CarConfig carConfig)
        {
            config = carConfig;
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            boostEnergy = config != null ? config.maxBoostEnergy : 100f;
            headingAngle = transform.eulerAngles.z;
            trajectoryAngle = headingAngle;
            currentState = DriftState.Grip;
            ResetInputs();
        }

        public void ApplyUpgradeBonuses(float speed, float accel, float grip, float recovery, float boostPower, float boostDuration)
        {
            upgradeSpeedBonus = speed;
            upgradeAccelBonus = accel;
            upgradeGripBonus = grip;
            upgradeRecoveryBonus = recovery;
            upgradeBoostMultiplierBonus = boostPower;
            upgradeBoostDurationBonus = boostDuration;
        }

        private void Update()
        {
            SyncInputs();
            if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                boostRequested = true;
            }
            UpdateTimers();
            HandleBoost();
        }

        private void FixedUpdate()
        {
            if (rb == null || config == null) return;

            // Zero residual angular velocity to ensure 100% deterministic authority
            rb.angularVelocity = 0f;

            float maxSpeed = (config.baseTopSpeed + upgradeSpeedBonus) * (isBoosting ? (config.boostSpeedMultiplier + upgradeBoostMultiplierBonus) : 1f);
            float accel = (config.baseAcceleration + upgradeAccelBonus) * (isBoosting ? config.boostAccelerationMultiplier : 1f);

            // Auto-acceleration forward
            if (autoAccelerate && currentSpeed < maxSpeed)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, accel * Time.fixedDeltaTime);
            }

            // Update Vehicle State Machine
            switch (currentState)
            {
                case DriftState.Grip:
                    UpdateGripState();
                    break;

                case DriftState.DriftEntry:
                case DriftState.Drifting:
                    UpdateDriftingState();
                    break;

                case DriftState.DriftExit:
                    UpdateDriftExitState();
                    break;

                case DriftState.Spinout:
                    // Spinout is disabled in this arcade iteration: automatically recover
                    currentState = DriftState.Grip;
                    UpdateGripState();
                    break;
            }
        }

        public void SyncInputs()
        {
            // Keyboard polling (fallback / Editor testing)
            float keyboardSteer = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) keyboardSteer -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) keyboardSteer += 1f;

            bool keyboardDrift = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift);

            // Active input selection: Keyboard takes priority if pressed, otherwise mobile touch
            if (Mathf.Abs(keyboardSteer) > 0.01f)
            {
                steerInput = keyboardSteer;
            }
            else
            {
                steerInput = touchSteer;
            }

            driftHeld = keyboardDrift || touchDrift;
        }

        private float GetSpeedSteerMultiplier()
        {
            if (config == null) return 1f;

            if (currentSpeed < 6.0f)
            {
                float t = Mathf.Clamp01(currentSpeed / 6.0f);
                return Mathf.Lerp(config.lowSpeedSteerMultiplier, 1.0f, t);
            }
            else if (currentSpeed > 16.0f)
            {
                float t = Mathf.Clamp01((currentSpeed - 16.0f) / 10.0f);
                return Mathf.Lerp(1.0f, config.highSpeedSteerMultiplier, t);
            }
            return 1.0f;
        }

        private void UpdateGripState()
        {
            if (Mathf.Abs(steerInput) > 0.01f)
            {
                float turnSpeed = config.turnRate * GetSpeedSteerMultiplier();
                float prevHeading = headingAngle;
                headingAngle += -steerInput * turnSpeed * Time.fixedDeltaTime;
                yawRate = Mathf.DeltaAngle(prevHeading, headingAngle) / Time.fixedDeltaTime;
                trajectoryAngle = headingAngle;
                currentTrajectoryTurnRate = yawRate;
                measuredTurnRadius = (Mathf.Abs(yawRate) > 1f) ? (currentSpeed / (Mathf.Abs(yawRate) * Mathf.Deg2Rad)) : 999f;
            }
            else
            {
                trajectoryAngle = headingAngle;
                yawRate = 0f;
                currentTrajectoryTurnRate = 0f;
                measuredTurnRadius = 999f;
            }

            currentDriftAngle = 0f;
            currentSlipAngle = 0f;
            driftDirection = 0f;

            // In Grip, velocity vector strictly follows heading
            Vector2 forwardDir = AngleToDirection(headingAngle);
            rb.linearVelocity = forwardDir * currentSpeed;
            rb.MoveRotation(headingAngle);

            // Drift Entry condition: speed threshold + handbrake held + steering input (pre-arm supported)
            if (driftHeld && currentSpeed >= config.minDriftSpeed && Mathf.Abs(steerInput) > 0.05f)
            {
                driftDirection = Mathf.Sign(steerInput);
                currentState = DriftState.Drifting;
                currentDriftDuration = 0f;
                longDriftAwarded = false;
                currentSlipAngle = config.baseDriftSlipAngle;
            }
        }

        private void UpdateDriftingState()
        {
            // Exit condition check: handbrake released or speed too low
            if (!driftHeld || currentSpeed < config.minDriftSpeed * 0.4f)
            {
                currentState = DriftState.DriftExit;
                exitTimer = config.driftExitDuration;
                return;
            }

            currentDriftDuration += Time.fixedDeltaTime;
            OnDriftTick?.Invoke(currentDriftDuration);

            if (currentDriftDuration >= 2.0f && !longDriftAwarded)
            {
                longDriftAwarded = true;
                OnLongDriftAchieved?.Invoke();
            }

            // Relative steering:
            // S_rel = steerInput * driftDirection
            // > 0 => steering into turn (closing arc, higher slip)
            // == 0 => neutral (holding steady arc)
            // < 0 => countersteering (opening arc, reducing slip, preparing flick)
            float steerInDrift = steerInput * driftDirection;
            float targetSlip;
            float targetTurnRate;
            float currentRetention;

            // 1. Dynamic Weight Transfer Flick Check (Figure-8 transition):
            // If the player countersteers opposite to current drift (steerInDrift < -0.45f),
            // and the slip angle has reduced significantly (<= transitionFlickThreshold),
            // the inertia transfers across neutral and flicks into an opposite drift!
            if (steerInDrift < -0.45f && currentSlipAngle <= config.transitionFlickThreshold)
            {
                driftDirection = -driftDirection;
                currentSlipAngle = config.baseDriftSlipAngle;
                comboCount++;
                OnDriftChain?.Invoke(comboCount);

                // Recompute relative steering under new drift direction
                steerInDrift = steerInput * driftDirection;
            }

            // 2. Modulate Turn Rate, Slip Angle, and Speed Retention:
            if (steerInDrift > 0.05f)
            {
                // Steering INTO drift: tighten radius, deepen slip
                float t = Mathf.Clamp01((steerInDrift - 0.05f) / 0.95f);
                targetSlip = Mathf.Lerp(config.baseDriftSlipAngle, config.maxDriftSlipAngle, t);
                targetTurnRate = Mathf.Lerp(config.driftBaseTurnRate, config.driftMaxTurnRate, t);
                currentRetention = Mathf.Lerp(config.driftSpeedRetention, config.hardDriftSpeedRetention, t);
            }
            else if (steerInDrift < -0.05f)
            {
                // Countersteering: open radius, reduce slip
                float t = Mathf.Clamp01((-steerInDrift - 0.05f) / 0.95f);
                targetSlip = Mathf.Lerp(config.baseDriftSlipAngle, config.minDriftSlipAngle, t);
                targetTurnRate = Mathf.Lerp(config.driftBaseTurnRate, config.driftMinTurnRate, t);
                currentRetention = config.driftSpeedRetention;
            }
            else
            {
                // Neutral: sweet spot balanced drift
                targetSlip = config.baseDriftSlipAngle;
                targetTurnRate = config.driftBaseTurnRate;
                currentRetention = config.driftSpeedRetention;
            }

            // 3. Smooth Slip Angle Update
            currentSlipAngle = Mathf.MoveTowards(currentSlipAngle, targetSlip, config.slipAngleRate * Time.fixedDeltaTime);
            currentDriftAngle = currentSlipAngle;

            // 4. Update Trajectory Curve
            float trajDelta = -driftDirection * targetTurnRate * Time.fixedDeltaTime;
            trajectoryAngle += trajDelta;
            currentTrajectoryTurnRate = targetTurnRate;

            // Compute measured turning radius: R = v / (omega in rad/s)
            float omegaRad = Mathf.Max(0.01f, targetTurnRate * Mathf.Deg2Rad);
            measuredTurnRadius = currentSpeed / omegaRad;

            // 5. Update Body Heading (Tail steps out, nose points into turn)
            float prevHeading = headingAngle;
            headingAngle = trajectoryAngle - (driftDirection * currentSlipAngle);
            yawRate = Mathf.DeltaAngle(prevHeading, headingAngle) / Time.fixedDeltaTime;

            // 6. Velocity Vector along trajectory
            float slideSpeed = currentSpeed * currentRetention;
            Vector2 trajDir = AngleToDirection(trajectoryAngle);
            rb.linearVelocity = trajDir * slideSpeed;
            rb.MoveRotation(headingAngle);
        }

        private void UpdateDriftExitState()
        {
            exitTimer -= Time.fixedDeltaTime;

            // Smoothly align body heading with travel trajectory (no snap, no rewind)
            float recoveryRate = config.maxDriftSlipAngle / Mathf.Max(0.05f, config.driftExitDuration);
            currentSlipAngle = Mathf.MoveTowards(currentSlipAngle, 0f, recoveryRate * Time.fixedDeltaTime);
            currentDriftAngle = currentSlipAngle;

            float prevHeading = headingAngle;
            headingAngle = Mathf.MoveTowardsAngle(headingAngle, trajectoryAngle, recoveryRate * Time.fixedDeltaTime);
            yawRate = Mathf.DeltaAngle(prevHeading, headingAngle) / Time.fixedDeltaTime;

            // Maintain forward trajectory motion
            Vector2 moveDir = AngleToDirection(trajectoryAngle);
            rb.linearVelocity = moveDir * currentSpeed;
            rb.MoveRotation(headingAngle);

            if (exitTimer <= 0f || currentSlipAngle <= 0.5f)
            {
                // Perfect Exit check
                bool isPerfect = (currentDriftAngle >= config.perfectExitMinAngle && currentDriftAngle <= config.perfectExitMaxAngle && currentDriftDuration >= 0.35f);
                if (isPerfect)
                {
                    currentSpeed = Mathf.Min(currentSpeed + config.perfectExitImpulse, (config.baseTopSpeed + upgradeSpeedBonus) * 1.15f);
                    OnPerfectExit?.Invoke();
                }

                chainTimer = config.driftChainWindow;
                comboCount++;
                OnDriftChain?.Invoke(comboCount);

                trajectoryAngle = headingAngle;
                currentState = DriftState.Grip;
                driftDirection = 0f;
            }
        }

        private Vector2 AngleToDirection(float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        }

        private void UpdateTimers()
        {
            if (chainTimer > 0f)
            {
                chainTimer -= Time.deltaTime;
                if (chainTimer <= 0f && currentState == DriftState.Grip)
                {
                    comboCount = 0;
                }
            }
        }

        private void HandleBoost()
        {
            if (boostRequested && !isBoosting && boostEnergy >= 30f)
            {
                boostRequested = false;
                isBoosting = true;
                currentBoostDuration = config.boostDuration + upgradeBoostDurationBonus;
                OnBoostStarted?.Invoke();
            }

            if (isBoosting)
            {
                currentBoostDuration -= Time.deltaTime;
                boostEnergy = Mathf.Max(0f, boostEnergy - (config.maxBoostEnergy / (config.boostDuration + upgradeBoostDurationBonus)) * Time.deltaTime);

                if (currentBoostDuration <= 0f || boostEnergy <= 0f)
                {
                    isBoosting = false;
                    OnBoostEnded?.Invoke();
                }
            }
        }

        public void AddBoostEnergy(float amount)
        {
            if (!isBoosting)
            {
                boostEnergy = Mathf.Clamp(boostEnergy + amount, 0f, config.maxBoostEnergy);
            }
        }

        public void TriggerCollisionPenalty(CollisionSeverity severity)
        {
            OnImpact?.Invoke(severity);
            comboCount = 0;

            switch (severity)
            {
                case CollisionSeverity.WallTouch:
                    currentSpeed *= 0.88f;
                    break;
                case CollisionSeverity.TrafficHit:
                    currentSpeed *= 0.60f;
                    if (currentState == DriftState.Drifting || currentState == DriftState.DriftEntry)
                    {
                        currentState = DriftState.Grip;
                        currentSlipAngle = 0f;
                    }
                    break;
                case CollisionSeverity.HeavyCrash:
                    currentSpeed *= 0.35f;
                    if (currentState == DriftState.Drifting || currentState == DriftState.DriftEntry)
                    {
                        currentState = DriftState.Grip;
                        currentSlipAngle = 0f;
                    }
                    break;
            }
        }

        // Public Mobile Input Hooks
        public void SetSteer(float value)
        {
            touchSteer = Mathf.Clamp(value, -1f, 1f);
            SyncInputs();
        }

        public void SetDrift(bool held)
        {
            touchDrift = held;
            SyncInputs();
        }

        public void RequestBoost()
        {
            boostRequested = true;
        }

        public void ResetInputs()
        {
            touchSteer = 0f;
            touchDrift = false;
            steerInput = 0f;
            driftHeld = false;
            boostRequested = false;
        }

        // Telemetry Instrumentation Properties
        public bool TelemetryDriftButtonHeld => driftHeld;
        public bool TelemetryDriftRequested => driftHeld && currentSpeed >= (config != null ? config.minDriftSpeed : 6f) && Mathf.Abs(steerInput) > 0.05f;
        public bool TelemetryDriftAssistActive => currentState == DriftState.Drifting;
        public bool TelemetryDynamicFlickActive => currentState == DriftState.Drifting && (steerInput * driftDirection < -0.45f);
    }
}

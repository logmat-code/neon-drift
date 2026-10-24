using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.Presentation
{
    /// <summary>
    /// Smooth, rock-solid portrait camera for NEON DRIFT.
    /// Strictly locks camera rotation to Quaternion.identity to eliminate disorienting camera sway.
    /// Provides consistent forward framing so the player always has ample track visibility ahead.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target & Tracking")]
        [SerializeField] private VehicleController target;
        public float smoothSpeed = 6.0f;

        public enum FramingMode
        {
            FixedForwardY,  // Standard endless forward highway (fixed vertical +Y lead)
            Omnidirectional // Velocity-oriented dynamic lead for circuits, closed loops & tuning grounds
        }

        [Header("Framing Mode")]
        public FramingMode framingMode = FramingMode.FixedForwardY;
        [Tooltip("Lead time in seconds based on linear velocity (Omnidirectional mode)")]
        public float velocityLeadTime = 0.16f;
        [Tooltip("Maximum distance the camera can lead ahead of the vehicle in meters (Omnidirectional mode)")]
        public float maxLeadDistance = 4.0f;

        [Header("Portrait Forward Framing (FixedForwardY Mode)")]
        [Tooltip("Fixed vertical forward lead keeping vehicle in lower 25% of portrait view")]
        public float forwardFramingOffset = 7.0f;
        [Tooltip("Subtle lateral anticipation following horizontal drift motion")]
        public float lateralAnticipation = 0.20f;

        [Header("Orthographic Zoom (Portrait 9:16)")]
        public float baseOrthoSize = 12.5f;
        public float boostOrthoSize = 14.0f;
        public float zoomSpeed = 3.0f;

        [Header("Camera Shake (Positional Only)")]
        public float traumaDecay = 2.5f;
        public float maxShakeOffset = 0.25f;

        private Camera cam;
        private float currentTrauma = 0f;
        private Vector3 currentVelocity = Vector3.zero;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = baseOrthoSize;
                cam.backgroundColor = new Color(0.045f, 0.050f, 0.065f, 1f); // Canonical uniform dark tarmac
            }
        }

        public void SetTarget(VehicleController vehicle)
        {
            target = vehicle;
            if (target != null)
            {
                target.OnImpact += HandleImpact;
            }
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            Vector3 offset = CalculateFramingOffset();
            transform.position = target.transform.position + offset;
            currentVelocity = Vector3.zero;
            currentTrauma = 0f;
        }

        public Vector3 CalculateFramingOffset()
        {
            if (framingMode == FramingMode.Omnidirectional)
            {
                if (target != null && target.Rigidbody != null)
                {
                    Vector2 vel = target.Rigidbody.linearVelocity;
                    Vector2 lead = Vector2.ClampMagnitude(vel * velocityLeadTime, maxLeadDistance);
                    return new Vector3(lead.x, lead.y, -10f);
                }
                return new Vector3(0f, 0f, -10f);
            }
            else
            {
                float lateralLead = (target != null && target.Rigidbody != null) ? target.Rigidbody.linearVelocity.x * lateralAnticipation : 0f;
                return new Vector3(lateralLead, forwardFramingOffset, -10f);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Strict camera rotation lock: zero tilting or angular wobbling
            transform.rotation = Quaternion.identity;

            Vector3 desiredPosition = target.transform.position + CalculateFramingOffset();

            // Smooth tracking
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

            // Handle Positional Camera Shake (No rotation shake)
            if (currentTrauma > 0f)
            {
                currentTrauma = Mathf.Max(0f, currentTrauma - traumaDecay * Time.deltaTime);
                float shakeIntensity = currentTrauma * currentTrauma;
                float offsetX = (Mathf.PerlinNoise(Time.time * 30f, 0f) - 0.5f) * 2f * maxShakeOffset * shakeIntensity;
                float offsetY = (Mathf.PerlinNoise(0f, Time.time * 30f) - 0.5f) * 2f * maxShakeOffset * shakeIntensity;
                transform.position += new Vector3(offsetX, offsetY, 0f);
            }

            // Smooth FOV zoom on Boost
            float targetOrtho = target.isBoosting ? boostOrthoSize : baseOrthoSize;
            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrtho, zoomSpeed * Time.deltaTime);
            }
        }

        private void HandleImpact(VehicleController.CollisionSeverity severity)
        {
            float addedTrauma = severity switch
            {
                VehicleController.CollisionSeverity.WallTouch => 0.15f,
                VehicleController.CollisionSeverity.TrafficHit => 0.40f,
                VehicleController.CollisionSeverity.HeavyCrash => 0.70f,
                _ => 0.20f
            };
            AddTrauma(addedTrauma);
        }

        public void AddTrauma(float amount)
        {
            currentTrauma = Mathf.Clamp01(currentTrauma + amount);
        }
    }
}

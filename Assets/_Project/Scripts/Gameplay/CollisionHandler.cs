using System;
using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.Gameplay
{
    public class CollisionHandler : MonoBehaviour
    {
        [SerializeField] private VehicleController vehicle;

        public event Action<VehicleController.CollisionSeverity, Vector2> OnCollisionOccurred;
        public event Action<int, int> OnDurabilityChanged; // currentDurability, maxDurability

        [Header("Vehicle Durability (Endless Defeat System)")]
        public int maxDurability = 3;
        public int currentDurability = 3;

        private float collisionCooldown = 0f;

        private void Awake()
        {
            if (vehicle == null)
                vehicle = GetComponent<VehicleController>();
            currentDurability = maxDurability;
        }

        public void ResetDurability()
        {
            currentDurability = maxDurability;
            collisionCooldown = 0f;
            OnDurabilityChanged?.Invoke(currentDurability, maxDurability);
        }

        private void Update()
        {
            if (collisionCooldown > 0f)
                collisionCooldown -= Time.deltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collisionCooldown > 0f) return;

            Vector2 contactPoint = collision.GetContact(0).point;
            float relativeImpactSpeed = collision.relativeVelocity.magnitude;

            VehicleController.CollisionSeverity severity;

            bool isWall = collision.gameObject.name.Contains("Curb") || 
                          collision.gameObject.name.Contains("Wall") || 
                          collision.gameObject.name.Contains("Barrier") || 
                          collision.gameObject.name.Contains("Guardrail");

            if (isWall)
            {
                severity = relativeImpactSpeed > 14f 
                    ? VehicleController.CollisionSeverity.TrafficHit 
                    : VehicleController.CollisionSeverity.WallTouch;
            }
            else // Traffic or obstacle
            {
                if (relativeImpactSpeed > 16f || vehicle.currentSpeed > 18f)
                {
                    severity = VehicleController.CollisionSeverity.HeavyCrash;
                }
                else
                {
                    severity = VehicleController.CollisionSeverity.TrafficHit;
                }

                // Momentarily ignore collision so the player doesn't get physically wedged against kinematic traffic
                if (collision.collider != null && collision.otherCollider != null)
                {
                    StartCoroutine(TemporarilyIgnoreCollision(collision.collider, collision.otherCollider, 0.45f));
                }
            }

            collisionCooldown = 0.4f;
            vehicle.TriggerCollisionPenalty(severity);
            OnCollisionOccurred?.Invoke(severity, contactPoint);

            // Play collision audio feedback
            if (severity == VehicleController.CollisionSeverity.HeavyCrash)
            {
                NeonDrift.Presentation.AudioManager.Instance?.PlayHeavyCrash();
            }
            else
            {
                if (collision.gameObject.name.Contains("Cone") || collision.gameObject.name.Contains("Barrier"))
                {
                    NeonDrift.Presentation.AudioManager.Instance?.PlayObstacleHit();
                }
                else
                {
                    NeonDrift.Presentation.AudioManager.Instance?.PlayCrash();
                }
            }

            // Fair defeat evaluation:
            // 1. Catastrophic high-speed impact (> 20 m/s or HeavyCrash at high speed) results in fatal crash
            // 2. Otherwise each significant collision (TrafficHit or HeavyCrash) depletes 1 durability point.
            if (severity == VehicleController.CollisionSeverity.HeavyCrash && vehicle.currentSpeed > 20f)
            {
                currentDurability = 0;
                OnDurabilityChanged?.Invoke(currentDurability, maxDurability);
                NeonDrift.Core.GameManager.Instance?.EndRun("TOTAL WRECK");
            }
            else if (severity == VehicleController.CollisionSeverity.TrafficHit || severity == VehicleController.CollisionSeverity.HeavyCrash)
            {
                currentDurability = Mathf.Max(0, currentDurability - 1);
                OnDurabilityChanged?.Invoke(currentDurability, maxDurability);

                if (currentDurability <= 0)
                {
                    NeonDrift.Core.GameManager.Instance?.EndRun("VEHICLE DESTROYED");
                }
            }
        }

        private System.Collections.IEnumerator TemporarilyIgnoreCollision(Collider2D c1, Collider2D c2, float duration)
        {
            if (c1 != null && c2 != null)
            {
                Physics2D.IgnoreCollision(c1, c2, true);
            }
            yield return new WaitForSeconds(duration);
            if (c1 != null && c2 != null)
            {
                Physics2D.IgnoreCollision(c1, c2, false);
            }
        }
    }
}

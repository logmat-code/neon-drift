using System;
using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.Gameplay
{
    public class CollisionHandler : MonoBehaviour
    {
        [SerializeField] private VehicleController vehicle;

        public event Action<VehicleController.CollisionSeverity, Vector2> OnCollisionOccurred;

        private float collisionCooldown = 0f;

        private void Awake()
        {
            if (vehicle == null)
                vehicle = GetComponent<VehicleController>();
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
            }

            collisionCooldown = 0.4f;
            vehicle.TriggerCollisionPenalty(severity);
            OnCollisionOccurred?.Invoke(severity, contactPoint);
        }
    }
}

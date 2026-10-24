using UnityEngine;
using NeonDrift.Gameplay;
using NeonDrift.Vehicle;

namespace NeonDrift.World
{
    public class TrafficCar : MonoBehaviour
    {
        public enum CarType
        {
            Compact,
            Sedan,
            Van
        }

        public CarType carType;
        public float forwardSpeed = 12f;
        public int currentLane = 0;
        public bool hasAwardedNearMiss = false;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void FixedUpdate()
        {
            // Traffic flows along the road heading (upwards in track coordinates)
            Vector2 nextPos = rb.position + (Vector2)transform.up * (forwardSpeed * Time.fixedDeltaTime);
            rb.MovePosition(nextPos);
        }

        public void ResetCar(CarType type, float speed, int lane)
        {
            carType = type;
            forwardSpeed = speed;
            currentLane = lane;
            hasAwardedNearMiss = false;
        }
    }
}

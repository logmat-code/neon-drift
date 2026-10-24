using UnityEngine;

namespace NeonDrift.World
{
    public class TrafficCar : MonoBehaviour
    {
        public enum CarType
        {
            Compact,
            Sedan,
            Sports,
            Taxi,
            Van,
            Pickup,
            SlowHazardTruck,
            Bus,
            RoadBarrier
        }

        public CarType carType = CarType.Sedan;
        public float speed = 12f;
        public float forwardSpeed => speed;
        public bool hasAwardedNearMiss = false;
        public int currentLane = 0;
        public float longitudinalDistance = 0f;
        public float lateralOffset = 0f;

        private TrackManager trackManager;

        public void Configure(TrackManager track, CarType type, float carSpeed, int lane, float startDistance, float laneOffset)
        {
            trackManager = track;
            carType = type;
            speed = carSpeed;
            hasAwardedNearMiss = false;
            currentLane = lane;
            longitudinalDistance = startDistance;
            lateralOffset = laneOffset;
        }

        public void ResetCar(CarType type, float carSpeed, int lane)
        {
            carType = type;
            speed = carSpeed;
            currentLane = lane;
            hasAwardedNearMiss = false;
        }

        public void ProcessMovement(float dt)
        {
            if (carType != CarType.RoadBarrier)
            {
                longitudinalDistance += speed * dt;
            }

            if (trackManager != null)
            {
                if (trackManager.GetRoadSampleAtDistance(longitudinalDistance, lateralOffset, out Vector2 pos, out Quaternion rot))
                {
                    transform.position = new Vector3(pos.x, pos.y, 0f);
                    transform.rotation = rot;
                }
            }
        }
    }
}

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
            TrafficCone,
            RoadBarrier,
            StalledWreck
        }

        public CarType carType = CarType.Sedan;
        public float speed = 12f;
        public float forwardSpeed => speed;
        public bool hasAwardedNearMiss = false;
        public int currentLane = 0;
        public float longitudinalDistance = 0f;
        public float lateralOffset = 0f;

        private TrackManager trackManager;
        private float laneChangeTimer = 0f;
        private float targetLateralOffset = 0f;
        private bool isChangingLane = false;

        public void Configure(TrackManager track, CarType type, float carSpeed, int lane, float startDistance, float laneOffset)
        {
            trackManager = track;
            carType = type;
            speed = carSpeed;
            hasAwardedNearMiss = false;
            currentLane = lane;
            longitudinalDistance = startDistance;
            lateralOffset = laneOffset;
            targetLateralOffset = laneOffset;
            isChangingLane = false;
            // Occasional dynamic lane changing for mid/high tier traffic
            laneChangeTimer = (type == CarType.Sports || type == CarType.Taxi) ? Random.Range(3.0f, 6.0f) : 9999f;
        }

        public void ResetCar(CarType type, float carSpeed, int lane)
        {
            carType = type;
            speed = carSpeed;
            currentLane = lane;
            hasAwardedNearMiss = false;
            isChangingLane = false;
        }

        public void ProcessMovement(float dt)
        {
            // Stationary obstacles remain fixed on longitudinal highway track
            bool isStationary = (carType == CarType.RoadBarrier || carType == CarType.TrafficCone || carType == CarType.StalledWreck);
            if (!isStationary)
            {
                longitudinalDistance += speed * dt;

                // Subtle dynamic lane change for lively arcade feel
                if (trackManager != null && (carType == CarType.Sports || carType == CarType.Taxi))
                {
                    laneChangeTimer -= dt;
                    if (laneChangeTimer <= 0f && !isChangingLane)
                    {
                        isChangingLane = true;
                        int newLane = Mathf.Clamp(currentLane + (Random.value < 0.5f ? -1 : 1), 0, 3);
                        float totalW = trackManager.roadWidth;
                        float laneWidth = totalW / 4f;
                        targetLateralOffset = -totalW * 0.5f + laneWidth * 0.5f + newLane * laneWidth;
                        currentLane = newLane;
                    }

                    if (isChangingLane)
                    {
                        lateralOffset = Mathf.MoveTowards(lateralOffset, targetLateralOffset, 3.2f * dt);
                        if (Mathf.Abs(lateralOffset - targetLateralOffset) < 0.05f)
                        {
                            lateralOffset = targetLateralOffset;
                            isChangingLane = false;
                            laneChangeTimer = Random.Range(4.0f, 8.0f);
                        }
                    }
                }
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

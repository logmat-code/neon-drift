using System.Collections.Generic;
using UnityEngine;
using NeonDrift.Vehicle;
using NeonDrift.World;

namespace NeonDrift.Gameplay
{
    public class NearMissDetector : MonoBehaviour
    {
        [SerializeField] private FlowManager flowManager;
        [SerializeField] private VehicleController vehicle;

        [Header("Detection Thresholds")]
        public float nearMissMaxLateralDist = 2.4f;
        public float nearMissMinLateralDist = 0.6f;
        public float longitudinalPassBand = 1.6f;
        public float minRelativeSpeed = 3f;

        private readonly Collider2D[] queryResults = new Collider2D[16];
        private ContactFilter2D contactFilter;

        private void Start()
        {
            contactFilter = new ContactFilter2D();
            contactFilter.useTriggers = true;
            contactFilter.SetLayerMask(Physics2D.AllLayers);
        }

        public void Initialize(FlowManager fm, VehicleController vc)
        {
            flowManager = fm;
            vehicle = vc;
        }

        private void Update()
        {
            if (vehicle == null || flowManager == null) return;

            Vector2 playerPos = vehicle.transform.position;
            int count = Physics2D.OverlapCircle(playerPos, 5f, contactFilter, queryResults);

            List<TrafficCar> nearbyCars = new List<TrafficCar>();

            for (int i = 0; i < count; i++)
            {
                TrafficCar tc = queryResults[i].GetComponentInParent<TrafficCar>();
                if (tc != null && !nearbyCars.Contains(tc))
                {
                    nearbyCars.Add(tc);
                }
            }

            CheckNearMissesAndGaps(playerPos, nearbyCars);
        }

        private void CheckNearMissesAndGaps(Vector2 playerPos, List<TrafficCar> nearbyCars)
        {
            TrafficCar leftCar = null;
            TrafficCar rightCar = null;

            foreach (var tc in nearbyCars)
            {
                Vector2 toCar = (Vector2)tc.transform.position - playerPos;
                float forwardDist = Vector2.Dot(vehicle.transform.up, toCar);
                float lateralDist = Vector2.Dot(vehicle.transform.right, toCar);

                // Check longitudinal passing alignment
                if (Mathf.Abs(forwardDist) <= longitudinalPassBand)
                {
                    float absLateral = Mathf.Abs(lateralDist);

                    // Near Miss single evaluation
                    if (!tc.hasAwardedNearMiss && absLateral >= nearMissMinLateralDist && absLateral <= nearMissMaxLateralDist)
                    {
                        float relSpeed = Mathf.Abs(vehicle.currentSpeed - tc.forwardSpeed);
                        if (relSpeed >= minRelativeSpeed)
                        {
                            tc.hasAwardedNearMiss = true;
                            bool isDrifting = vehicle.currentState == DriftState.Drifting;
                            flowManager.RegisterNearMiss(isDrifting);
                            NeonDrift.Presentation.AudioManager.Instance?.PlayNearMiss();
                        }
                    }

                    // Gap identification (cars on both flanks)
                    if (lateralDist < -nearMissMinLateralDist && lateralDist > -3.5f)
                        leftCar = tc;
                    else if (lateralDist > nearMissMinLateralDist && lateralDist < 3.5f)
                        rightCar = tc;
                }
            }

            // Check if player threaded between two cars simultaneously
            if (leftCar != null && rightCar != null)
            {
                if (!leftCar.hasAwardedNearMiss || !rightCar.hasAwardedNearMiss)
                {
                    leftCar.hasAwardedNearMiss = true;
                    rightCar.hasAwardedNearMiss = true;
                    bool isDrifting = vehicle.currentState == DriftState.Drifting;
                    flowManager.RegisterTrafficGap(isDrifting);
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.World
{
    public class TrafficManager : MonoBehaviour
    {
        [Header("Traffic Sprites")]
        [SerializeField] private Sprite compactSprite;
        [SerializeField] private Sprite sedanSprite;
        [SerializeField] private Sprite sportsSprite;
        [SerializeField] private Sprite taxiSprite;
        [SerializeField] private Sprite vanSprite;
        [SerializeField] private Sprite pickupSprite;
        [SerializeField] private Sprite busSprite;

        [Header("Obstacle & Hazard Sprites")]
        [SerializeField] private Sprite coneSprite;
        [SerializeField] private Sprite barrierSprite;
        [SerializeField] private Sprite stalledCarSprite;

        [Header("Target References")]
        [SerializeField] private VehicleController playerVehicle;
        [SerializeField] private TrackManager trackManager;

        [Header("Traffic Toggle")]
        public bool trafficEnabled = true;

        [Header("Spawning Tuning")]
        public float spawnDistanceAhead = 65f;
        public float despawnDistanceBehind = 35f;
        public float baseSpawnInterval = 1.25f;
        public int maxActiveTraffic = 20;

        private readonly List<TrafficCar> activeCars = new List<TrafficCar>();
        private readonly Queue<TrafficCar> pool = new Queue<TrafficCar>();

        private float spawnTimer = 0f;
        private int lastSpawnLane = -1;

        public int activeTrafficCount => activeCars.Count;
        public int pooledTrafficCount => pool.Count;

        private void Start()
        {
            if (trackManager != null)
            {
                trackManager.OnFloatingOriginShift += HandleFloatingOriginShift;
            }
        }

        private void OnDestroy()
        {
            if (trackManager != null)
            {
                trackManager.OnFloatingOriginShift -= HandleFloatingOriginShift;
            }
        }

        public void Initialize(VehicleController player, TrackManager track, 
            Sprite compact, Sprite sedan, Sprite sports, Sprite taxi, Sprite van, Sprite pickup, Sprite bus,
            Sprite cone = null, Sprite barrier = null, Sprite stalled = null)
        {
            if (trackManager != null)
            {
                trackManager.OnFloatingOriginShift -= HandleFloatingOriginShift;
            }

            playerVehicle = player;
            trackManager = track;
            compactSprite = compact;
            sedanSprite = sedan;
            sportsSprite = sports;
            taxiSprite = taxi;
            vanSprite = van;
            pickupSprite = pickup;
            busSprite = bus;
            coneSprite = cone;
            barrierSprite = barrier;
            stalledCarSprite = stalled;

            if (trackManager != null)
            {
                trackManager.OnFloatingOriginShift += HandleFloatingOriginShift;
            }
        }

        private void Update()
        {
            UpdateTraffic(Time.deltaTime);
        }

        public void UpdateTraffic(float dt)
        {
            if (playerVehicle == null || !trafficEnabled || trackManager == null) return;

            // 1. Move all traffic vehicles and obstacles
            for (int i = 0; i < activeCars.Count; i++)
            {
                activeCars[i].ProcessMovement(dt);
            }

            // 2. Recycle cars outside the active road window
            RecyclePassedTraffic();

            // 3. Progressive Spawning (Density & Variety evolves with distance)
            spawnTimer -= dt;
            float playerDist = trackManager.totalDistanceTraveled;
            // Progressive interval: starts at 1.4s, drops to 0.75s in high intensity
            float currentInterval = Mathf.Lerp(1.4f, 0.75f, Mathf.Clamp01(playerDist / 2500f));

            if (spawnTimer <= 0f && activeCars.Count < maxActiveTraffic)
            {
                spawnTimer = currentInterval;
                SpawnNextTraffic();
            }
        }

        public void SpawnNextTraffic()
        {
            if (trackManager == null) return;

            float playerDist = trackManager.totalDistanceTraveled;
            float spawnDist = playerDist + spawnDistanceAhead;

            // Lane selection (0..3) with fairness check: never block identical lane back-to-back
            int lane = Random.Range(0, 4);
            if (lane == lastSpawnLane && Random.value < 0.65f)
            {
                lane = (lane + 1) % 4;
            }
            lastSpawnLane = lane;

            float totalW = trackManager.roadWidth;
            float laneWidth = totalW / 4f;
            float laneOffset = -totalW * 0.5f + laneWidth * 0.5f + lane * laneWidth;

            // Progressive Variety & Obstacle selection based on run progress
            float roll = Random.value;
            TrafficCar.CarType type;

            if (playerDist < 300f) // Early run: mostly compacts, sedans, gentle cones
            {
                if (roll < 0.45f) type = TrafficCar.CarType.Compact;
                else if (roll < 0.85f) type = TrafficCar.CarType.Sedan;
                else type = TrafficCar.CarType.TrafficCone;
            }
            else if (playerDist < 900f) // Mid run: Taxis, Sports cars, Vans, Cones, Occasional Barrier
            {
                if (roll < 0.25f) type = TrafficCar.CarType.Taxi;
                else if (roll < 0.50f) type = TrafficCar.CarType.Sports;
                else if (roll < 0.70f) type = TrafficCar.CarType.Van;
                else if (roll < 0.85f) type = TrafficCar.CarType.TrafficCone;
                else type = TrafficCar.CarType.RoadBarrier;
            }
            else // Late run: Pickups, Heavy Trucks, Transit Buses, Stalled Wrecks & Highway Barriers
            {
                if (roll < 0.18f) type = TrafficCar.CarType.Pickup;
                else if (roll < 0.36f) type = TrafficCar.CarType.Bus;
                else if (roll < 0.52f) type = TrafficCar.CarType.SlowHazardTruck;
                else if (roll < 0.68f) type = TrafficCar.CarType.Sports;
                else if (roll < 0.80f) type = TrafficCar.CarType.StalledWreck;
                else if (roll < 0.90f) type = TrafficCar.CarType.RoadBarrier;
                else type = TrafficCar.CarType.TrafficCone;
            }

            // Speed differentiation
            float speed = type switch
            {
                TrafficCar.CarType.Compact => Random.Range(10f, 13f),
                TrafficCar.CarType.Sedan => Random.Range(11f, 14f),
                TrafficCar.CarType.Taxi => Random.Range(13f, 16f),
                TrafficCar.CarType.Sports => Random.Range(15f, 18f),
                TrafficCar.CarType.Van => Random.Range(9f, 12f),
                TrafficCar.CarType.Pickup => Random.Range(10f, 13f),
                TrafficCar.CarType.Bus => Random.Range(7f, 10f),
                TrafficCar.CarType.SlowHazardTruck => Random.Range(6f, 8f),
                TrafficCar.CarType.TrafficCone => 0f,
                TrafficCar.CarType.RoadBarrier => 0f,
                TrafficCar.CarType.StalledWreck => 0f,
                _ => 11f
            };

            TrafficCar car = GetFromPool();
            car.Configure(trackManager, type, speed, lane, spawnDist, laneOffset);

            SpriteRenderer sr = car.GetComponent<SpriteRenderer>();
            sr.sprite = type switch
            {
                TrafficCar.CarType.Compact => compactSprite != null ? compactSprite : sedanSprite,
                TrafficCar.CarType.Sedan => sedanSprite,
                TrafficCar.CarType.Taxi => taxiSprite != null ? taxiSprite : sedanSprite,
                TrafficCar.CarType.Sports => sportsSprite != null ? sportsSprite : sedanSprite,
                TrafficCar.CarType.Van => vanSprite != null ? vanSprite : sedanSprite,
                TrafficCar.CarType.Pickup => pickupSprite != null ? pickupSprite : vanSprite,
                TrafficCar.CarType.Bus => busSprite != null ? busSprite : vanSprite,
                TrafficCar.CarType.SlowHazardTruck => vanSprite != null ? vanSprite : sedanSprite,
                TrafficCar.CarType.TrafficCone => coneSprite != null ? coneSprite : compactSprite,
                TrafficCar.CarType.RoadBarrier => barrierSprite != null ? barrierSprite : compactSprite,
                TrafficCar.CarType.StalledWreck => stalledCarSprite != null ? stalledCarSprite : sportsSprite,
                _ => sedanSprite
            };

            sr.color = type switch
            {
                TrafficCar.CarType.SlowHazardTruck => new Color(1f, 0.65f, 0.15f, 1f),
                TrafficCar.CarType.TrafficCone => Color.white,
                TrafficCar.CarType.RoadBarrier => Color.white,
                TrafficCar.CarType.StalledWreck => new Color(0.9f, 0.9f, 0.9f, 1f),
                _ => Color.white
            };

            BoxCollider2D col = car.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = type switch
                {
                    TrafficCar.CarType.Compact => new Vector2(2.0f, 3.4f),
                    TrafficCar.CarType.Sedan => new Vector2(2.2f, 4.0f),
                    TrafficCar.CarType.Taxi => new Vector2(2.2f, 4.0f),
                    TrafficCar.CarType.Sports => new Vector2(2.2f, 4.0f),
                    TrafficCar.CarType.Van => new Vector2(2.4f, 4.8f),
                    TrafficCar.CarType.Pickup => new Vector2(2.4f, 4.6f),
                    TrafficCar.CarType.Bus => new Vector2(2.6f, 6.4f),
                    TrafficCar.CarType.SlowHazardTruck => new Vector2(2.6f, 5.2f),
                    TrafficCar.CarType.TrafficCone => new Vector2(1.0f, 1.0f),
                    TrafficCar.CarType.RoadBarrier => new Vector2(2.2f, 1.2f),
                    TrafficCar.CarType.StalledWreck => new Vector2(2.2f, 4.0f),
                    _ => new Vector2(2.2f, 4.2f)
                };
            }

            car.transform.localScale = type switch
            {
                TrafficCar.CarType.Bus => new Vector3(0.85f, 0.85f, 1f),
                TrafficCar.CarType.TrafficCone => new Vector3(0.80f, 0.80f, 1f),
                TrafficCar.CarType.RoadBarrier => new Vector3(0.85f, 0.85f, 1f),
                _ => new Vector3(0.72f, 0.72f, 1f)
            };

            car.gameObject.SetActive(true);
            activeCars.Add(car);

            // Position immediately on curved highway frame
            car.ProcessMovement(0f);
        }

        public void PreSeedInitialTraffic()
        {
            if (trackManager == null) return;
            // Spawn 3 immediate traffic cars ahead so highway isn't empty at launch
            float[] initialOffsets = new float[] { 28f, 48f, 68f };
            int[] initialLanes = new int[] { 0, 2, 1 };

            for (int i = 0; i < initialOffsets.Length; i++)
            {
                float dist = trackManager.totalDistanceTraveled + initialOffsets[i];
                int lane = initialLanes[i];
                float totalW = trackManager.roadWidth;
                float laneWidth = totalW / 4f;
                float laneOffset = -totalW * 0.5f + laneWidth * 0.5f + lane * laneWidth;

                TrafficCar.CarType type = (i == 0) ? TrafficCar.CarType.Sedan : (i == 1 ? TrafficCar.CarType.Taxi : TrafficCar.CarType.Compact);
                float speed = (i == 0) ? 11f : (i == 1 ? 13f : 12f);

                TrafficCar car = GetFromPool();
                car.Configure(trackManager, type, speed, lane, dist, laneOffset);

                SpriteRenderer sr = car.GetComponent<SpriteRenderer>();
                sr.sprite = type switch
                {
                    TrafficCar.CarType.Compact => compactSprite != null ? compactSprite : sedanSprite,
                    TrafficCar.CarType.Taxi => taxiSprite != null ? taxiSprite : sedanSprite,
                    _ => sedanSprite
                };
                sr.color = Color.white;

                BoxCollider2D col = car.GetComponent<BoxCollider2D>();
                if (col != null) col.size = new Vector2(2.2f, 4.2f);
                car.transform.localScale = new Vector3(0.72f, 0.72f, 1f);

                car.gameObject.SetActive(true);
                activeCars.Add(car);
                car.ProcessMovement(0f);
            }
        }

        private TrafficCar GetFromPool()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            GameObject go = new GameObject("TrafficCar_Pooled");
            try { go.tag = "Traffic"; } catch {}
            go.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = false;

            TrafficCar tc = go.AddComponent<TrafficCar>();
            return tc;
        }

        private void RecyclePassedTraffic()
        {
            float playerDist = trackManager.totalDistanceTraveled;

            for (int i = activeCars.Count - 1; i >= 0; i--)
            {
                TrafficCar car = activeCars[i];
                if (car == null)
                {
                    activeCars.RemoveAt(i);
                    continue;
                }

                float diff = car.longitudinalDistance - playerDist;
                // Despawn if passed behind player view (> despawnDistanceBehind) or extremely far ahead (> 95m)
                if (diff < -despawnDistanceBehind || diff > spawnDistanceAhead + 35f)
                {
                    car.gameObject.SetActive(false);
                    pool.Enqueue(car);
                    activeCars.RemoveAt(i);
                }
            }
        }

        private void HandleFloatingOriginShift(Vector3 shift)
        {
            foreach (var car in activeCars)
            {
                if (car != null)
                {
                    car.transform.position += shift;
                }
            }
        }

        public void ClearAllTraffic()
        {
            for (int i = activeCars.Count - 1; i >= 0; i--)
            {
                if (activeCars[i] != null)
                {
                    activeCars[i].gameObject.SetActive(false);
                    pool.Enqueue(activeCars[i]);
                }
            }
            activeCars.Clear();
            spawnTimer = 0f;
        }
    }
}

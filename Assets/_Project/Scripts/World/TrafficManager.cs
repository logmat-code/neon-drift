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

        [Header("Target References")]
        [SerializeField] private VehicleController playerVehicle;
        [SerializeField] private TrackManager trackManager;

        [Header("Traffic Toggle")]
        public bool trafficEnabled = true;

        [Header("Spawning Tuning")]
        public float spawnDistanceAhead = 55f;
        public float despawnDistanceBehind = 25f;
        public float baseSpawnInterval = 1.6f;
        public int maxActiveTraffic = 16;

        private readonly List<TrafficCar> activeCars = new List<TrafficCar>();
        private readonly Queue<TrafficCar> pool = new Queue<TrafficCar>();

        private float spawnTimer = 0f;
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
            Sprite compact, Sprite sedan, Sprite sports, Sprite taxi, Sprite van, Sprite pickup, Sprite bus)
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

            if (trackManager != null)
            {
                trackManager.OnFloatingOriginShift += HandleFloatingOriginShift;
            }
        }

        private void Update()
        {
            if (playerVehicle == null || !trafficEnabled || trackManager == null) return;

            float dt = Time.deltaTime;

            // 1. Move all traffic vehicles along real highway curves
            for (int i = 0; i < activeCars.Count; i++)
            {
                activeCars[i].ProcessMovement(dt);
            }

            // 2. Recycle cars outside the active road window
            RecyclePassedTraffic();

            // 3. Progressive Spawning (Variety evolves with distance)
            spawnTimer -= dt;
            if (spawnTimer <= 0f && activeCars.Count < maxActiveTraffic)
            {
                spawnTimer = baseSpawnInterval;
                SpawnNextTraffic();
            }
        }

        private void SpawnNextTraffic()
        {
            float playerDist = trackManager.totalDistanceTraveled;
            float spawnDist = playerDist + spawnDistanceAhead;

            // Lane selection: 0, 1, 2, 3
            int lane = Random.Range(0, 4);
            float totalW = trackManager.roadWidth;
            float laneWidth = totalW / 4f;
            float laneOffset = -totalW * 0.5f + laneWidth * 0.5f + lane * laneWidth;

            // Progressive Variety selection based on distance
            float roll = Random.value;
            TrafficCar.CarType type;
            if (playerDist < 400f) // Early run: mostly compacts & sedans
            {
                type = roll < 0.6f ? TrafficCar.CarType.Compact : TrafficCar.CarType.Sedan;
            }
            else if (playerDist < 1200f) // Mid run: Taxis, Sports cars & Vans
            {
                if (roll < 0.35f) type = TrafficCar.CarType.Taxi;
                else if (roll < 0.65f) type = TrafficCar.CarType.Sports;
                else if (roll < 0.85f) type = TrafficCar.CarType.Van;
                else type = TrafficCar.CarType.Sedan;
            }
            else // Late run: Pickups, Trucks, Transit Buses & Road Obstacles
            {
                if (roll < 0.20f) type = TrafficCar.CarType.Pickup;
                else if (roll < 0.40f) type = TrafficCar.CarType.Bus;
                else if (roll < 0.60f) type = TrafficCar.CarType.SlowHazardTruck;
                else if (roll < 0.75f) type = TrafficCar.CarType.Sports;
                else if (roll < 0.90f) type = TrafficCar.CarType.Taxi;
                else type = TrafficCar.CarType.RoadBarrier;
            }

            float speed = type switch
            {
                TrafficCar.CarType.Compact => Random.Range(13f, 15f),
                TrafficCar.CarType.Sedan => Random.Range(14f, 17f),
                TrafficCar.CarType.Taxi => Random.Range(15f, 18f),
                TrafficCar.CarType.Sports => Random.Range(17f, 21f),
                TrafficCar.CarType.Van => Random.Range(12f, 14f),
                TrafficCar.CarType.Pickup => Random.Range(13f, 15f),
                TrafficCar.CarType.Bus => Random.Range(10f, 12f),
                TrafficCar.CarType.SlowHazardTruck => Random.Range(8f, 10f),
                TrafficCar.CarType.RoadBarrier => 0f,
                _ => 14f
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
                TrafficCar.CarType.RoadBarrier => compactSprite != null ? compactSprite : sedanSprite,
                _ => sedanSprite
            };

            sr.color = type switch
            {
                TrafficCar.CarType.SlowHazardTruck => new Color(1f, 0.65f, 0.15f, 1f),
                TrafficCar.CarType.RoadBarrier => new Color(1f, 0.25f, 0.15f, 1f),
                _ => Color.white
            };

            BoxCollider2D col = car.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = type switch
                {
                    TrafficCar.CarType.Compact => new Vector2(2.0f, 3.5f),
                    TrafficCar.CarType.Sedan => new Vector2(2.2f, 4.2f),
                    TrafficCar.CarType.Taxi => new Vector2(2.2f, 4.2f),
                    TrafficCar.CarType.Sports => new Vector2(2.3f, 4.4f),
                    TrafficCar.CarType.Van => new Vector2(2.5f, 5.2f),
                    TrafficCar.CarType.Pickup => new Vector2(2.5f, 5.0f),
                    TrafficCar.CarType.Bus => new Vector2(2.7f, 7.8f),
                    TrafficCar.CarType.SlowHazardTruck => new Vector2(2.8f, 5.8f),
                    TrafficCar.CarType.RoadBarrier => new Vector2(2.2f, 2.0f),
                    _ => new Vector2(2.2f, 4.2f)
                };
            }

            car.transform.localScale = (type == TrafficCar.CarType.Bus) ? new Vector3(0.85f, 0.85f, 1f) : new Vector3(0.72f, 0.72f, 1f);
            car.gameObject.SetActive(true);
            activeCars.Add(car);

            // Position immediately
            car.ProcessMovement(0f);
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
                if (diff < -despawnDistanceBehind || diff > spawnDistanceAhead + 60f)
                {
                    car.gameObject.SetActive(false);
                    pool.Enqueue(car);
                    activeCars.RemoveAt(i);
                }
            }
        }

        private void HandleFloatingOriginShift(Vector3 shift)
        {
            // Traffic positions are calculated relative to TrackManager distance on curve,
            // but for immediate visual continuity in this frame:
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
        }
    }
}

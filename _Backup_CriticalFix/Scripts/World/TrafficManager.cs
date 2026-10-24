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
        [SerializeField] private Sprite vanSprite;

        [Header("Target References")]
        [SerializeField] private VehicleController playerVehicle;
        [SerializeField] private TrackManager trackManager;

        [Header("Traffic Toggle")]
        public bool trafficEnabled = true;

        [Header("Spawning Tuning")]
        public float spawnDistanceAhead = 42f;
        public float despawnDistanceBehind = 25f;
        public float baseSpawnInterval = 1.8f;
        public int maxActiveTraffic = 14;

        private readonly List<TrafficCar> activeCars = new List<TrafficCar>();
        private readonly Queue<TrafficCar> pool = new Queue<TrafficCar>();

        private float spawnTimer = 0f;
        private readonly float[] laneOffsets = new float[] { -5.25f, -1.75f, 1.75f, 5.25f };

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

        public void Initialize(VehicleController player, TrackManager track, Sprite compact, Sprite sedan, Sprite van)
        {
            if (trackManager != null)
            {
                trackManager.OnFloatingOriginShift -= HandleFloatingOriginShift;
            }

            playerVehicle = player;
            trackManager = track;
            compactSprite = compact;
            sedanSprite = sedan;
            vanSprite = van;

            if (trackManager != null)
            {
                trackManager.OnFloatingOriginShift += HandleFloatingOriginShift;
            }
        }

        private void Update()
        {
            if (playerVehicle == null || !trafficEnabled) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && activeCars.Count < maxActiveTraffic)
            {
                // Query road context ahead of player from TrackManager
                Vector2 roadCenter = (Vector2)playerVehicle.transform.position + (Vector2)playerVehicle.transform.up * spawnDistanceAhead;
                Quaternion roadRot = playerVehicle.transform.rotation;
                float density = 0.4f;
                int driftDir = 0;
                ChunkType chunkType = ChunkType.StraightShort;

                if (trackManager != null)
                {
                    trackManager.GetRoadFrameAhead(spawnDistanceAhead, out roadCenter, out roadRot, out density, out driftDir, out chunkType);
                }

                // Dynamic difficulty based on total distance
                float distanceTraveled = trackManager != null ? trackManager.totalDistanceTraveled : 0f;
                float difficultyFactor = Mathf.Clamp01(distanceTraveled / 2500f);
                float effectiveDensity = Mathf.Clamp(density + difficultyFactor * 0.25f, 0.15f, 0.75f);

                if (Random.value <= effectiveDensity + 0.15f)
                {
                    TrySpawnContextualTraffic(roadCenter, roadRot, driftDir, chunkType, effectiveDensity);
                }

                float currentInterval = Mathf.Lerp(baseSpawnInterval, 1.1f, difficultyFactor);
                spawnTimer = Mathf.Max(0.7f, currentInterval / Mathf.Max(0.4f, effectiveDensity * 1.4f));
            }

            RecyclePassedTraffic();
        }

        private void TrySpawnContextualTraffic(Vector2 roadCenter, Quaternion roadRot, int driftDir, ChunkType chunkType, float density)
        {
            // Determine lane candidates based on chunk type to enable clean racing lines and avoid unavoidable walls
            List<int> candidateLanes = new List<int> { 0, 1, 2, 3 };

            if (chunkType == ChunkType.HairpinLeft || chunkType == ChunkType.HairpinRight)
            {
                // Hairpin: Spawns at most 1 car on the very outside lane, leaving the apex and racing line wide open
                int safeLane = (chunkType == ChunkType.HairpinLeft) ? 3 : 0;
                if (Random.value < 0.5f)
                {
                    SpawnCarAt(roadCenter, roadRot, safeLane);
                }
                return;
            }

            if (chunkType == ChunkType.TechnicalLeft || chunkType == ChunkType.TechnicalRight)
            {
                // Technical curve: Leave inside apex completely open, at most 1 car on the outer lane
                int outerLane = (chunkType == ChunkType.TechnicalLeft) ? 3 : 0;
                if (Random.value < 0.65f)
                {
                    SpawnCarAt(roadCenter, roadRot, outerLane);
                }
                return;
            }

            if (chunkType == ChunkType.StraightTraffic)
            {
                // Intentional Traffic Gap: Spawn in lane 0 and lane 3, leaving 1 & 2 open to thread between
                SpawnCarAt(roadCenter, roadRot, 0);
                if (density > 0.4f && Random.value < 0.8f)
                {
                    SpawnCarAt(roadCenter, roadRot, 3);
                }
                return;
            }

            if (driftDir == -1) // Sweeper or curve turning Left
            {
                // Leave inside apex (lane 0) clear; place traffic on outer lanes for Near Miss clipping
                candidateLanes.Remove(0);
            }
            else if (driftDir == 1) // Sweeper or curve turning Right
            {
                // Leave inside apex (lane 3) clear; place traffic on outer lanes
                candidateLanes.Remove(3);
            }

            // Never spawn in all 4 lanes simultaneously (always at least 1-2 open escape corridors)
            int carsToSpawn = (density > 0.55f && candidateLanes.Count > 2 && Random.value < 0.55f) ? 2 : 1;

            for (int i = 0; i < carsToSpawn; i++)
            {
                if (candidateLanes.Count <= 1) break;

                int pickIdx = Random.Range(0, candidateLanes.Count);
                int chosenLane = candidateLanes[pickIdx];
                candidateLanes.RemoveAt(pickIdx);

                SpawnCarAt(roadCenter, roadRot, chosenLane);
            }
        }

        private void SpawnCarAt(Vector2 roadCenter, Quaternion roadRot, int lane)
        {
            TrafficCar.CarType type = (TrafficCar.CarType)Random.Range(0, 3);
            float speed = type switch
            {
                TrafficCar.CarType.Compact => Random.Range(12f, 15f),
                TrafficCar.CarType.Sedan => Random.Range(14f, 17f),
                TrafficCar.CarType.Van => Random.Range(11f, 13f),
                _ => 13f
            };

            Vector2 rightDir = roadRot * Vector2.right;
            float totalW = trackManager != null ? trackManager.roadWidth : 22f;
            float laneWidth = totalW / 4f;
            float laneOffset = -totalW * 0.5f + laneWidth * 0.5f + lane * laneWidth;
            Vector2 spawnPos = roadCenter + rightDir * laneOffset;

            TrafficCar car = GetFromPool();
            car.transform.position = spawnPos;
            car.transform.rotation = roadRot;
            car.ResetCar(type, speed, lane);

            SpriteRenderer sr = car.GetComponent<SpriteRenderer>();
            sr.sprite = type switch
            {
                TrafficCar.CarType.Compact => compactSprite,
                TrafficCar.CarType.Sedan => sedanSprite,
                TrafficCar.CarType.Van => vanSprite,
                _ => sedanSprite
            };

            BoxCollider2D col = car.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.size = type switch
                {
                    TrafficCar.CarType.Compact => new Vector2(2.0f, 3.5f),
                    TrafficCar.CarType.Sedan => new Vector2(2.2f, 4.2f),
                    TrafficCar.CarType.Van => new Vector2(2.5f, 5.2f),
                    _ => new Vector2(2.2f, 4.2f)
                };
            }

            car.transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            car.gameObject.SetActive(true);
            activeCars.Add(car);
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
            Vector2 playerPos = playerVehicle.transform.position;
            Vector2 playerForward = playerVehicle.transform.up;

            for (int i = activeCars.Count - 1; i >= 0; i--)
            {
                TrafficCar car = activeCars[i];
                if (car == null)
                {
                    activeCars.RemoveAt(i);
                    continue;
                }

                Vector2 toCar = (Vector2)car.transform.position - playerPos;
                float forwardProj = Vector2.Dot(playerForward, toCar);

                // Despawn if behind the player or excessively far ahead
                if (forwardProj < -despawnDistanceBehind || forwardProj > spawnDistanceAhead + 45f)
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
                    var rb = car.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.position += (Vector2)shift;
                    }
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

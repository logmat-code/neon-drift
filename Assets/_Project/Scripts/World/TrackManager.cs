using System;
using System.Collections.Generic;
using UnityEngine;
using NeonDrift.Vehicle;

namespace NeonDrift.World
{
    /// <summary>
    /// Central Endless Procedural Track Manager for NEON DRIFT.
    /// Manages continuous multi-chunk highway generation, self-balancing drift patterns,
    /// seamless object pooling, floating origin shifts, and dynamic microzone theming.
    /// Eliminates physical world limits — run continues indefinitely.
    /// </summary>
    public class TrackManager : MonoBehaviour
    {
        [Header("Target & References")]
        [SerializeField] private VehicleController playerVehicle;

        [Header("Track Configuration")]
        [SerializeField] private Material roadMaterial;
        [SerializeField] private Sprite roadSprite;
        [SerializeField] private Sprite finishSprite;
        [SerializeField] private Sprite buildingTowerSprite;
        [SerializeField] private Sprite buildingShopSprite;
        [SerializeField] private Sprite lampSprite;
        [SerializeField] private Sprite vendingSprite;

        [Header("Mountain Touge Environment")]
        [SerializeField] private Sprite mountainPineSprite;
        [SerializeField] private Sprite rockyCliffSprite;
        [SerializeField] private Sprite mountainGuardrailSprite;
        [SerializeField] private Sprite chevronSignSprite;

        public Material RoadMaterial { get => roadMaterial; set => roadMaterial = value; }

        public float roadWidth = 22f;
        public float generationAheadDistance = 380f;
        public float despawnBehindDistance = 65f;
        public float floatingOriginThreshold = 1000f;

        [Header("Runtime Endless State")]
        public int activeSeed = 0;
        public float totalDistanceTraveled = 0f;
        public MicroZone currentZone = MicroZone.NeonCore;
        public int activeChunkCount => activeChunks.Count;
        public int pooledChunkCount => chunkPool.Count;
        public float totalTrackDistance => float.PositiveInfinity;
        public IReadOnlyList<RoadChunk> ActiveChunks => activeChunks;
        public Queue<RoadChunk> ChunkPool => chunkPool;

        // Events
        public event Action<Vector3> OnFloatingOriginShift;
        public event Action<MicroZone> OnZoneChanged;
        public event Action OnFinishCrossed; // Kept for interface backward compatibility

        private System.Random rng;
        private Vector2 currentHeadPos = Vector2.zero;
        private float currentHeadHeading = 0f;
        private float zoneDistanceCounter = 0f;
        private int lastPatternIndex = -1;

        private readonly List<RoadChunk> activeChunks = new List<RoadChunk>();
        private readonly Queue<RoadChunk> chunkPool = new Queue<RoadChunk>();
        private readonly Queue<ChunkDefinition> pendingChunks = new Queue<ChunkDefinition>();

        private Transform chunksRoot;

        private void Awake()
        {
            // Clean up any orphan pre-serialized chunks children from edit mode saves
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name.Contains("EndlessChunksRoot") || child.name.Contains("Chunk"))
                {
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }
            }

            GameObject rootGO = new GameObject("EndlessChunksRoot");
            rootGO.transform.SetParent(transform, false);
            chunksRoot = rootGO.transform;
        }

        private void Start()
        {
            if (playerVehicle == null)
            {
                playerVehicle = UnityEngine.Object.FindFirstObjectByType<VehicleController>();
            }

            if (activeChunks.Count == 0)
            {
                ResetTrack(activeSeed);
            }
        }

        public void Initialize(
            VehicleController player,
            Sprite road,
            Sprite finish,
            Sprite tower,
            Sprite shop,
            Sprite lamp,
            Sprite vm,
            int seed = 0,
            Material roadMat = null)
        {
            playerVehicle = player;
            roadSprite = road;
            finishSprite = finish;
            buildingTowerSprite = tower;
            buildingShopSprite = shop;
            lampSprite = lamp;
            vendingSprite = vm;
            activeSeed = seed;
            if (roadMat != null) roadMaterial = roadMat;

            ResetTrack(seed);
        }

        public void BuildTrack(Sprite road, Sprite finish, Sprite tower, Sprite shop, Sprite lamp, Sprite vm)
        {
            roadSprite = road;
            finishSprite = finish;
            buildingTowerSprite = tower;
            buildingShopSprite = shop;
            lampSprite = lamp;
            vendingSprite = vm;

            if (playerVehicle == null)
            {
                playerVehicle = UnityEngine.Object.FindFirstObjectByType<VehicleController>();
            }

            ResetTrack(activeSeed);
        }

        public void ResetTrack(int seed = 0)
        {
            if (chunksRoot == null)
            {
                GameObject rootGO = new GameObject("EndlessChunksRoot");
                rootGO.transform.SetParent(transform, false);
                chunksRoot = rootGO.transform;
            }
            chunksRoot.localPosition = Vector3.zero;

            activeSeed = (seed != 0) ? seed : UnityEngine.Random.Range(100000, 999999);
            rng = new System.Random(activeSeed);
            Debug.Log($"[EndlessTrack] Run started with Seed: {activeSeed}");

            // Recycle all active chunks into pool
            for (int i = activeChunks.Count - 1; i >= 0; i--)
            {
                if (activeChunks[i] != null)
                {
                    activeChunks[i].Recycle();
                    chunkPool.Enqueue(activeChunks[i]);
                }
            }
            activeChunks.Clear();
            pendingChunks.Clear();

            totalDistanceTraveled = 0f;
            currentHeadPos = Vector2.zero;
            currentHeadHeading = 0f;
            currentZone = MicroZone.NeonCore;
            zoneDistanceCounter = 0f;
            lastPatternIndex = -1;

            // Pre-seed initial launch straight (30m) so player has clean takeoff
            var introChunk = new ChunkDefinition(ChunkType.StraightShort, 32f, 0f, 0, 0.1f);
            SpawnChunk(introChunk);

            // Populate initial road horizon (~400m ahead)
            while (Vector2.Distance(Vector2.zero, currentHeadPos) < generationAheadDistance)
            {
                EnsureNextChunk();
            }
        }

        private void Update()
        {
            ProcessUpdate(Time.deltaTime);
        }

        public void ProcessUpdate(float dt)
        {
            if (playerVehicle == null) return;

            Vector2 playerPos = playerVehicle.transform.position;
            totalDistanceTraveled += playerVehicle.currentSpeed * dt;
            zoneDistanceCounter += playerVehicle.currentSpeed * dt;

            // 1. Dynamic District Progression (transitions every 450m)
            if (zoneDistanceCounter >= 450f)
            {
                zoneDistanceCounter = 0f;
                AdvanceDistrict();
            }

            // 2. Generate ahead: Maintain continuous track horizon
            float distToHead = Vector2.Distance(playerPos, currentHeadPos);
            while (distToHead < generationAheadDistance)
            {
                EnsureNextChunk();
                distToHead = Vector2.Distance(playerPos, currentHeadPos);
            }

            // 3. Recycle behind: Remove chunks that are far behind the player
            RecyclePassedChunks(playerPos);

            // 4. Floating Origin: Shift world when coordinates exceed threshold
            if (Mathf.Abs(playerPos.y) > floatingOriginThreshold || Mathf.Abs(playerPos.x) > floatingOriginThreshold)
            {
                ExecuteFloatingOriginShift();
            }
        }

        private void AdvanceDistrict()
        {
            currentZone = currentZone switch
            {
                MicroZone.NeonCore => MicroZone.Backstreets,
                MicroZone.Backstreets => MicroZone.ElevatedMetro,
                MicroZone.ElevatedMetro => MicroZone.NightMarket,
                MicroZone.NightMarket => MicroZone.CanalSide,
                MicroZone.CanalSide => MicroZone.NeonCore,
                _ => MicroZone.NeonCore
            };
            OnZoneChanged?.Invoke(currentZone);
        }

        private void EnsureNextChunk()
        {
            if (pendingChunks.Count == 0)
            {
                QueueNextPattern();
            }

            if (pendingChunks.Count > 0)
            {
                ChunkDefinition def = pendingChunks.Dequeue();
                SpawnChunk(def);
            }
        }

        private int lastExitDirection = 0;

        private void QueueNextPattern()
        {
            float difficulty = Mathf.Clamp01(totalDistanceTraveled / 3000f);
            var library = DriftPatternLibrary.Patterns;

            List<int> validIndices = new List<int>();
            List<float> weights = new List<float>();

            for (int i = 0; i < library.Count; i++)
            {
                var pat = library[i];
                if (pat.minDifficulty > difficulty) continue;
                if (i == lastPatternIndex && library.Count > 1) continue;

                // 1. Heading Envelope Enforcement:
                // Keep highway progression within +-45 deg cone. Disqualify patterns that turn further away from center.
                if (currentHeadHeading > 0f && pat.entryDirection > 0) continue; // Forbid turning further right
                if (currentHeadHeading < 0f && pat.entryDirection < 0) continue; // Forbid turning further left

                // 2. Entry/Exit Direction Compatibility:
                // If exiting in an active drift, forbid immediately snapping into opposite hard drift without transition
                if (lastExitDirection != 0 && pat.entryDirection != 0 && pat.entryDirection != lastExitDirection)
                {
                    // Check if pattern starts with a transition/straight chunk (>= 20m)
                    if (pat.chunks.Length > 0 && pat.chunks[0].driftDirection != 0)
                    {
                        continue; // Requires smooth transition first
                    }
                }

                float w = pat.baseWeight;

                validIndices.Add(i);
                weights.Add(w);
            }

            int selectedIdx = 0;
            if (validIndices.Count > 0)
            {
                float totalW = 0f;
                foreach (float w in weights) totalW += w;

                double roll = rng.NextDouble() * totalW;
                float running = 0f;
                for (int i = 0; i < validIndices.Count; i++)
                {
                    running += weights[i];
                    if (roll <= running)
                    {
                        selectedIdx = validIndices[i];
                        break;
                    }
                }
            }
            else
            {
                // Fallback to flow pattern 0
                selectedIdx = 0;
            }

            lastPatternIndex = selectedIdx;
            var chosenPattern = library[selectedIdx];
            lastExitDirection = chosenPattern.exitDirection;

            foreach (var chk in chosenPattern.chunks)
            {
                pendingChunks.Enqueue(chk);
            }
        }

        private void SpawnChunk(ChunkDefinition def)
        {
            RoadChunk chunk;
            if (chunkPool.Count > 0)
            {
                chunk = chunkPool.Dequeue();
            }
            else
            {
                GameObject go = new GameObject($"Chunk_{activeChunks.Count}");
                go.transform.SetParent(chunksRoot);
                chunk = go.AddComponent<RoadChunk>();
            }

            chunk.Configure(
                currentHeadPos,
                currentHeadHeading,
                def,
                currentZone,
                roadMaterial,
                buildingTowerSprite,
                buildingShopSprite,
                lampSprite,
                vendingSprite,
                roadWidth,
                mountainPineSprite,
                rockyCliffSprite,
                mountainGuardrailSprite,
                chevronSignSprite);

            currentHeadPos = chunk.exitPosition;
            currentHeadHeading = chunk.exitHeading;
            activeChunks.Add(chunk);
        }

        private void RecyclePassedChunks(Vector2 playerPos)
        {
            Camera cam = Camera.main;
            Vector2 camPos = cam != null ? (Vector2)cam.transform.position : playerPos;
            float orthoH = cam != null ? cam.orthographicSize : 15f;
            float orthoW = cam != null ? orthoH * cam.aspect : 9f;

            // Expanded frustum boundary with 35m safety margin
            float margin = 35f;
            float camMinX = camPos.x - orthoW - margin;
            float camMaxX = camPos.x + orthoW + margin;
            float camMinY = camPos.y - orthoH - margin;
            float camMaxY = camPos.y + orthoH + margin;

            for (int i = activeChunks.Count - 1; i >= 0; i--)
            {
                var chunk = activeChunks[i];
                if (chunk == null)
                {
                    activeChunks.RemoveAt(i);
                    continue;
                }

                Bounds b = chunk.GetWorldBounds();

                // 1. Chunk must be strictly behind the camera's bottom frustum margin
                bool isPastBottomFrustum = (b.max.y < camMinY);

                // 2. Chunk must not intersect the expanded camera viewport box
                bool overlapsFrustumX = !(b.max.x < camMinX || b.min.x > camMaxX);
                bool overlapsFrustumY = !(b.max.y < camMinY || b.min.y > camMaxY);
                bool overlapsFrustum = overlapsFrustumX && overlapsFrustumY;

                if (isPastBottomFrustum && !overlapsFrustum)
                {
                    chunk.Recycle();
                    chunkPool.Enqueue(chunk);
                    activeChunks.RemoveAt(i);
                }
            }
        }

        public void ExecuteFloatingOriginShift()
        {
            if (playerVehicle == null) return;

            Vector3 shift = new Vector3(-playerVehicle.transform.position.x, -playerVehicle.transform.position.y, 0f);

            // 1. Shift Player Vehicle
            playerVehicle.transform.position += shift;
            if (playerVehicle.Rigidbody != null)
            {
                playerVehicle.Rigidbody.position += (Vector2)shift;
            }

            // 2. Shift Camera
            var camCtrl = UnityEngine.Object.FindFirstObjectByType<NeonDrift.Presentation.CameraController>();
            if (camCtrl != null)
            {
                camCtrl.transform.position += shift;
            }

            // 3. Shift all active chunks
            foreach (var chunk in activeChunks)
            {
                if (chunk != null)
                {
                    chunk.ShiftPosition(shift);
                }
            }

            // 4. Shift head position
            currentHeadPos += (Vector2)shift;

            // 5. Notify all listeners (TrafficManager, VFX, etc.)
            OnFloatingOriginShift?.Invoke(shift);

            // 6. Immediately synchronize 2D physics transforms to prevent ghost collisions
            Physics2D.SyncTransforms();

            Debug.Log($"[EndlessTrack] Floating Origin Shift executed: {shift}. World recentered seamlessly.");
        }

        /// <summary>
        /// Queries the road frame (center, rotation, density, drift direction) at a specified distance ahead of the player.
        /// Used by TrafficManager to spawn cars strictly in highway lanes.
        /// </summary>
        public bool GetRoadSampleAtDistance(float distance, float lateralOffset, out Vector2 pos, out Quaternion rot)
        {
            pos = Vector2.zero;
            rot = Quaternion.identity;
            if (activeChunks.Count == 0) return false;

            float accumulatedDist = 0f;
            for (int i = 0; i < activeChunks.Count; i++)
            {
                var chunk = activeChunks[i];
                if (chunk == null) continue;
                float chunkLen = chunk.lengthMeters;

                if (distance <= accumulatedDist + chunkLen || i == activeChunks.Count - 1)
                {
                    float localT = Mathf.Clamp01((distance - accumulatedDist) / Mathf.Max(1f, chunkLen));
                    if (chunk.stepPositions != null && chunk.stepPositions.Length > 0)
                    {
                        int stepIdx = Mathf.Clamp(Mathf.RoundToInt(localT * (chunk.stepPositions.Length - 1)), 0, chunk.stepPositions.Length - 1);
                        Vector2 center = chunk.stepPositions[stepIdx];
                        rot = chunk.stepRotations[stepIdx];
                        Vector2 right = rot * Vector2.right;
                        pos = center + right * lateralOffset;
                        return true;
                    }
                    else
                    {
                        Vector2 center = Vector2.Lerp(chunk.entryPosition, chunk.exitPosition, localT);
                        rot = Quaternion.Euler(0, 0, -Mathf.LerpAngle(chunk.entryHeading, chunk.exitHeading, localT));
                        Vector2 right = rot * Vector2.right;
                        pos = center + right * lateralOffset;
                        return true;
                    }
                }
                accumulatedDist += chunkLen;
            }
            return false;
        }

        public bool GetRoadFrameAhead(
            float distanceAhead,
            out Vector2 center,
            out Quaternion rotation,
            out float density,
            out int driftDirection,
            out ChunkType type)
        {
            center = playerVehicle != null ? (Vector2)playerVehicle.transform.position + (Vector2)playerVehicle.transform.up * distanceAhead : Vector2.zero;
            rotation = playerVehicle != null ? playerVehicle.transform.rotation : Quaternion.identity;
            density = 0.5f;
            driftDirection = 0;
            type = ChunkType.StraightShort;

            if (activeChunks.Count == 0) return false;

            Vector2 playerPos = playerVehicle != null ? (Vector2)playerVehicle.transform.position : Vector2.zero;

            // Find closest step in active chunks to determine vehicle's current road position
            int bestChunkIdx = 0;
            int bestStepIdx = 0;
            float minSqrDist = float.MaxValue;

            for (int c = 0; c < activeChunks.Count; c++)
            {
                var chk = activeChunks[c];
                if (chk == null || chk.stepPositions == null) continue;

                for (int s = 0; s < chk.stepPositions.Length; s++)
                {
                    float sqrDist = (chk.stepPositions[s] - playerPos).sqrMagnitude;
                    if (sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        bestChunkIdx = c;
                        bestStepIdx = s;
                    }
                }
            }

            // Advance forward along the road centerline steps (4.0m per step)
            int stepsToAdvance = Mathf.Max(0, Mathf.RoundToInt(distanceAhead / 4.0f));
            int targetChunkIdx = bestChunkIdx;
            int targetStepIdx = bestStepIdx + stepsToAdvance;

            while (targetChunkIdx < activeChunks.Count)
            {
                var chk = activeChunks[targetChunkIdx];
                if (chk != null && chk.stepPositions != null && chk.stepPositions.Length > 0)
                {
                    if (targetStepIdx < chk.stepPositions.Length)
                    {
                        center = chk.stepPositions[targetStepIdx];
                        rotation = chk.stepRotations[targetStepIdx];
                        density = chk.trafficDensity;
                        driftDirection = chk.driftDirection;
                        type = chk.chunkType;
                        return true;
                    }
                    else
                    {
                        targetStepIdx -= chk.stepPositions.Length;
                        targetChunkIdx++;
                    }
                }
                else
                {
                    targetChunkIdx++;
                }
            }

            if (activeChunks.Count > 0)
            {
                var lastChunk = activeChunks[activeChunks.Count - 1];
                center = lastChunk.exitPosition;
                rotation = Quaternion.Euler(0f, 0f, -lastChunk.exitHeading);
                density = lastChunk.trafficDensity;
                driftDirection = lastChunk.driftDirection;
                type = lastChunk.chunkType;
                return true;
            }

            return false;
        }

        // Backward compatibility hooks
        public float GetProgress(Vector2 playerPosition)
        {
            return Mathf.Clamp01(totalDistanceTraveled / 3000f);
        }

        public float GetTrafficDensityAtProgress(float progress)
        {
            return Mathf.Lerp(0.25f, 0.75f, progress);
        }
    }
}

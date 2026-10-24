using System.Collections.Generic;
using UnityEngine;

namespace NeonDrift.World
{
    public enum MicroZone
    {
        NeonCore,
        Backstreets,
        ElevatedMetro,
        NightMarket,
        CanalSide
    }

    /// <summary>
    /// Pooled, modular road chunk for NEON DRIFT endless procedural generation.
    /// Manages internal road tiles, anti-void foundation, side colliders, and thematic props.
    /// Fully recyclable with zero per-frame runtime allocations.
    /// </summary>
    public class RoadChunk : MonoBehaviour
    {
        public ChunkType chunkType;
        public MicroZone microZone;
        public Vector2 entryPosition;
        public float entryHeading;
        public Vector2 exitPosition;
        public float exitHeading;
        public float lengthMeters;
        public float curvatureDelta;
        public int driftDirection; // -1 Left, +1 Right, 0 None
        public float trafficDensity;

        // Baked step points along the chunk centerline for rapid frame queries
        public Vector2[] stepPositions;
        public Quaternion[] stepRotations;

        private readonly List<GameObject> activeSegments = new List<GameObject>();
        private readonly List<GameObject> segmentPool = new List<GameObject>();

        private const float SEGMENT_STEP = 4.0f; // 4m per tile

        public void Configure(
            Vector2 startPos,
            float startHeading,
            ChunkDefinition def,
            MicroZone zone,
            Sprite roadSprite,
            Sprite towerSprite,
            Sprite shopSprite,
            Sprite lampSprite,
            Sprite vmSprite,
            float roadWidth)
        {
            chunkType = def.type;
            microZone = zone;
            entryPosition = startPos;
            entryHeading = startHeading;
            lengthMeters = def.length;
            curvatureDelta = def.curvature;
            driftDirection = def.driftDirection;
            trafficDensity = def.trafficDensity;

            int steps = Mathf.CeilToInt(lengthMeters / SEGMENT_STEP);
            if (stepPositions == null || stepPositions.Length != steps)
            {
                stepPositions = new Vector2[steps];
                stepRotations = new Quaternion[steps];
            }

            Vector2 curPos = startPos;
            float curHeading = startHeading;

            for (int i = 0; i < steps; i++)
            {
                float stepAngleDelta;
                if (def.isCompoundCurve)
                {
                    // First half turns +curvature, second half turns -curvature
                    int half = steps / 2;
                    float sign = (i < half) ? 1f : -1f;
                    stepAngleDelta = (def.curvature / (half > 0 ? half : 1)) * sign;
                }
                else
                {
                    stepAngleDelta = curvatureDelta / steps;
                }

                curHeading += stepAngleDelta;
                Quaternion rot = Quaternion.Euler(0f, 0f, -curHeading);
                Vector2 forwardDir = rot * Vector2.up;
                Vector2 rightDir = rot * Vector2.right;

                stepPositions[i] = curPos;
                stepRotations[i] = rot;

                GameObject seg = GetOrCreateSegment(i, roadSprite, towerSprite, shopSprite, lampSprite, vmSprite, roadWidth);
                seg.transform.position = curPos;
                seg.transform.rotation = rot;
                seg.SetActive(true);

                // Configure visual props for the active microzone
                DecorateSegment(seg, i, zone, rightDir, rot, roadWidth);

                curPos += forwardDir * SEGMENT_STEP;
            }

            // Deactivate any unused pre-pooled segments
            for (int i = steps; i < segmentPool.Count; i++)
            {
                segmentPool[i].SetActive(false);
            }

            exitPosition = curPos;
            exitHeading = curHeading;
            gameObject.SetActive(true);
        }

        private GameObject GetOrCreateSegment(
            int index,
            Sprite roadSprite,
            Sprite towerSprite,
            Sprite shopSprite,
            Sprite lampSprite,
            Sprite vmSprite,
            float roadWidth)
        {
            if (index < segmentPool.Count)
            {
                return segmentPool[index];
            }

            GameObject seg = new GameObject($"Seg_{index}");
            seg.transform.SetParent(transform);

            // 1. Road Surface (Sorting -10) with 6% longitudinal overlap to eliminate curve seams
            SpriteRenderer sr = seg.AddComponent<SpriteRenderer>();
            sr.sprite = roadSprite;
            sr.sortingOrder = -10;
            sr.transform.localScale = new Vector3(roadWidth / 14f, 1.06f, 1f);

            // 3. Left Curb Wall Collider
            GameObject wallL = new GameObject("LeftCurb");
            wallL.transform.SetParent(seg.transform);
            wallL.transform.localPosition = new Vector3(-roadWidth * 0.5f, 0f, 0f);
            wallL.transform.localRotation = Quaternion.identity;
            wallL.tag = "Wall";
            BoxCollider2D colL = wallL.AddComponent<BoxCollider2D>();
            colL.size = new Vector2(1.2f, SEGMENT_STEP + 0.1f);

            // 4. Right Curb Wall Collider
            GameObject wallR = new GameObject("RightCurb");
            wallR.transform.SetParent(seg.transform);
            wallR.transform.localPosition = new Vector3(roadWidth * 0.5f, 0f, 0f);
            wallR.transform.localRotation = Quaternion.identity;
            wallR.tag = "Wall";
            BoxCollider2D colR = wallR.AddComponent<BoxCollider2D>();
            colR.size = new Vector2(1.2f, SEGMENT_STEP + 0.1f);

            // 5. Curbside Prop Sockets (Lamp L/R, Flank L/R)
            CreatePropHolder(seg, "LampL", lampSprite, -5);
            CreatePropHolder(seg, "LampR", lampSprite, -5);
            CreatePropHolder(seg, "FlankL", shopSprite, -15);
            CreatePropHolder(seg, "FlankR", shopSprite, -15);
            CreatePropHolder(seg, "TowerL", towerSprite, -18);
            CreatePropHolder(seg, "TowerR", towerSprite, -18);
            CreatePropHolder(seg, "VendingL", vmSprite, -4);
            CreatePropHolder(seg, "VendingR", vmSprite, -4);

            segmentPool.Add(seg);
            return seg;
        }

        private void CreatePropHolder(GameObject parent, string name, Sprite sprite, int sortOrder)
        {
            GameObject prop = new GameObject(name);
            prop.transform.SetParent(parent.transform);
            prop.transform.localPosition = Vector3.zero;
            prop.transform.localRotation = Quaternion.identity;
            SpriteRenderer sr = prop.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortOrder;
            prop.SetActive(false);
        }

        private void DecorateSegment(GameObject seg, int stepIdx, MicroZone zone, Vector2 rightDir, Quaternion rot, float roadWidth)
        {
            var lampL = seg.transform.Find("LampL")?.gameObject;
            var lampR = seg.transform.Find("LampR")?.gameObject;
            var flankL = seg.transform.Find("FlankL")?.gameObject;
            var flankR = seg.transform.Find("FlankR")?.gameObject;
            var towerL = seg.transform.Find("TowerL")?.gameObject;
            var towerR = seg.transform.Find("TowerR")?.gameObject;
            var vmL = seg.transform.Find("VendingL")?.gameObject;
            var vmR = seg.transform.Find("VendingR")?.gameObject;

            // Reset all prop visibilities
            if (lampL) lampL.SetActive(false);
            if (lampR) lampR.SetActive(false);
            if (flankL) flankL.SetActive(false);
            if (flankR) flankR.SetActive(false);
            if (towerL) towerL.SetActive(false);
            if (towerR) towerR.SetActive(false);
            if (vmL) vmL.SetActive(false);
            if (vmR) vmR.SetActive(false);

            float halfRoad = roadWidth * 0.5f;

            Color lampColor = zone switch
            {
                MicroZone.NeonCore => new Color(0f, 0.9f, 1f),       // Vibrant Cyan
                MicroZone.Backstreets => new Color(1f, 0.75f, 0.25f), // Warm Amber
                MicroZone.ElevatedMetro => new Color(0.4f, 0.85f, 1f),// Steel Cyan
                MicroZone.NightMarket => new Color(1f, 0.2f, 0.5f),   // Vibrant Magenta
                MicroZone.CanalSide => new Color(0.85f, 0.95f, 1f),   // Cold White/Ice
                _ => Color.white
            };

            Color buildingColor = zone switch
            {
                MicroZone.NeonCore => Color.white,
                MicroZone.Backstreets => new Color(0.95f, 0.9f, 0.8f),
                MicroZone.ElevatedMetro => new Color(0.5f, 0.6f, 0.75f),
                MicroZone.NightMarket => new Color(1f, 0.85f, 0.9f),
                MicroZone.CanalSide => new Color(0.35f, 0.45f, 0.8f),
                _ => Color.white
            };

            // 1. Street Lamps
            int lampInterval = (zone == MicroZone.NeonCore || zone == MicroZone.NightMarket) ? 3 : 5;
            if (stepIdx % lampInterval == 0)
            {
                if (lampL)
                {
                    lampL.transform.localPosition = new Vector3(-halfRoad - 1.2f, 0f, 0f);
                    lampL.GetComponent<SpriteRenderer>().color = lampColor;
                    lampL.SetActive(true);
                }
                if (lampR)
                {
                    lampR.transform.localPosition = new Vector3(halfRoad + 1.2f, 0f, 0f);
                    lampR.GetComponent<SpriteRenderer>().color = lampColor;
                    lampR.SetActive(true);
                }
            }

            // 2. Vending Machines
            if ((zone == MicroZone.Backstreets || zone == MicroZone.NightMarket) && stepIdx % 4 == 1)
            {
                if (vmL)
                {
                    vmL.transform.localPosition = new Vector3(-halfRoad - 2.2f, 0f, 0f);
                    vmL.SetActive(true);
                }
                if (vmR)
                {
                    vmR.transform.localPosition = new Vector3(halfRoad + 2.2f, 0f, 0f);
                    vmR.SetActive(true);
                }
            }

            // 3. Storefronts
            if (stepIdx % 4 == 2 && zone != MicroZone.CanalSide)
            {
                if (flankL)
                {
                    flankL.transform.localPosition = new Vector3(-halfRoad - 5.5f, 0f, 0f);
                    flankL.GetComponent<SpriteRenderer>().color = buildingColor;
                    flankL.SetActive(true);
                }
                if (flankR)
                {
                    flankR.transform.localPosition = new Vector3(halfRoad + 5.5f, 0f, 0f);
                    flankR.GetComponent<SpriteRenderer>().color = buildingColor;
                    flankR.SetActive(true);
                }
            }

            // 4. Skyscraper Towers in Background
            if (stepIdx % 6 == 3)
            {
                if (towerL)
                {
                    towerL.transform.localPosition = new Vector3(-halfRoad - 11.5f, 0f, 0f);
                    towerL.GetComponent<SpriteRenderer>().color = buildingColor;
                    towerL.SetActive(true);
                }
                if (towerR)
                {
                    towerR.transform.localPosition = new Vector3(halfRoad + 11.5f, 0f, 0f);
                    towerR.GetComponent<SpriteRenderer>().color = buildingColor;
                    towerR.SetActive(true);
                }
            }
        }

        public void ShiftPosition(Vector3 delta)
        {
            transform.position += delta;
            entryPosition += (Vector2)delta;
            exitPosition += (Vector2)delta;

            if (stepPositions != null)
            {
                for (int i = 0; i < stepPositions.Length; i++)
                {
                    stepPositions[i] += (Vector2)delta;
                }
            }
        }

        public void Recycle()
        {
            gameObject.SetActive(false);
        }

        public bool TryGetPointAtDistance(float distanceAlongChunk, out Vector2 center, out Quaternion rot)
        {
            center = entryPosition;
            rot = Quaternion.Euler(0f, 0f, -entryHeading);

            if (stepPositions == null || stepPositions.Length == 0) return false;

            int stepIdx = Mathf.Clamp(Mathf.FloorToInt(distanceAlongChunk / SEGMENT_STEP), 0, stepPositions.Length - 1);
            center = stepPositions[stepIdx];
            rot = stepRotations[stepIdx];
            return true;
        }
    }
}

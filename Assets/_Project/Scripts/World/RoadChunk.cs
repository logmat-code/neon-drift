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
    /// Uses continuous procedural 2D ribbon mesh and smooth EdgeCollider2D boundaries.
    /// Completely eliminates overlapping slabs, stepped edges, diagonal cuts, and ghost vertices.
    /// Fully recyclable with zero per-frame runtime allocations.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
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

        // Baked step points along the chunk centerline for rapid frame and traffic queries
        public Vector2[] stepPositions;
        public Quaternion[] stepRotations;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh roadMesh;

        private GameObject wallL_GO;
        private GameObject wallR_GO;
        private EdgeCollider2D edgeColliderL;
        private EdgeCollider2D edgeColliderR;

        // Pooled prop holders
        private readonly List<GameObject> activeProps = new List<GameObject>();
        private readonly List<GameObject> propPool = new List<GameObject>();

        private const float MESH_STEP = 1.5f; // 1.5m slice resolution for silky smooth curvature
        private const float QUERY_STEP = 4.0f; // 4.0m step resolution for external traffic queries

        private static readonly float[] SLICE_OFFSETS = new float[]
        {
            -110.0f, // 0: Mountain Wall Far Left (-110m)
            -25.0f,  // 1: Mountain Base Left (-25m)
            -25.0f,  // 2: Shoulder Verge Left (-25m)
            -11.2f,  // 3: Shoulder Edge Left (-11.2m)
            -11.2f,  // 4: Curb Outer Left (-11.2m)
            -10.2f,  // 5: Curb Inner Left (-10.2m)
            -10.2f,  // 6: Asphalt Outer Left (-10.2m)
            -0.25f,  // 7: Asphalt Inner Left (-0.25m)
            -0.25f,  // 8: Centerline Left (-0.25m)
            +0.25f,  // 9: Centerline Right (+0.25m)
            +0.25f,  // 10: Asphalt Inner Right (+0.25m)
            +10.2f,  // 11: Asphalt Outer Right (+10.2m)
            +10.2f,  // 12: Curb Inner Right (+10.2m)
            +11.2f,  // 13: Curb Outer Right (+11.2m)
            +11.2f,  // 14: Shoulder Edge Right (+11.2m)
            +25.0f,  // 15: Cliff Verge Right (+25m)
            +25.0f,  // 16: Precipice Edge Right (+25m)
            +110.0f  // 17: Deep Valley Abyss Right (+110m)
        };

        private static readonly Color COLOR_MOUNTAIN = new Color(0.06f, 0.08f, 0.12f, 1f);  // Dark rocky alpine slope
        private static readonly Color COLOR_SHOULDER = new Color(0.09f, 0.10f, 0.13f, 1f);  // Mountain dirt / gravel verge
        private static readonly Color COLOR_CURB     = new Color(0.95f, 0.95f, 0.98f, 1f);  // Crisp white/cyan curb
        private static readonly Color COLOR_ASPHALT  = new Color(0.14f, 0.15f, 0.18f, 1f);  // Dark highway asphalt
        private static readonly Color COLOR_YELLOW   = new Color(0.96f, 0.82f, 0.15f, 1f);  // Neon gold highway double-line
        private static readonly Color COLOR_ABYSS    = new Color(0.03f, 0.04f, 0.07f, 1f);  // Night cliff valley abyss

        private static readonly Color[] SLICE_COLORS = new Color[]
        {
            COLOR_MOUNTAIN, COLOR_MOUNTAIN, // 0, 1: Mountain Face Far Left
            COLOR_SHOULDER, COLOR_SHOULDER, // 2, 3: Gravel Verge Left
            COLOR_CURB, COLOR_CURB,         // 4, 5: Curb Left
            COLOR_ASPHALT, COLOR_ASPHALT,   // 6, 7: Asphalt Road Left
            COLOR_YELLOW, COLOR_YELLOW,     // 8, 9: Centerline
            COLOR_ASPHALT, COLOR_ASPHALT,   // 10, 11: Asphalt Road Right
            COLOR_CURB, COLOR_CURB,         // 12, 13: Curb Right
            COLOR_SHOULDER, COLOR_SHOULDER, // 14, 15: Precipice Verge Right
            COLOR_ABYSS, COLOR_ABYSS        // 16, 17: Valley Abyss Right
        };

        private void EnsureComponents(Material roadMat)
        {
            if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = -10;
                if (roadMat != null && meshRenderer.sharedMaterial != roadMat)
                {
                    meshRenderer.sharedMaterial = roadMat;
                }
            }

            if (roadMesh == null)
            {
                roadMesh = new Mesh { name = $"ChunkMesh_{gameObject.name}" };
                roadMesh.MarkDynamic();
                meshFilter.sharedMesh = roadMesh;
            }

            if (wallL_GO == null)
            {
                Transform tL = transform.Find("LeftWall");
                if (tL != null) wallL_GO = tL.gameObject;
                else
                {
                    wallL_GO = new GameObject("LeftWall");
                    wallL_GO.transform.SetParent(transform, false);
                }
                wallL_GO.tag = "Wall";
                if (!wallL_GO.TryGetComponent(out edgeColliderL))
                {
                    edgeColliderL = wallL_GO.AddComponent<EdgeCollider2D>();
                }
            }

            if (wallR_GO == null)
            {
                Transform tR = transform.Find("RightWall");
                if (tR != null) wallR_GO = tR.gameObject;
                else
                {
                    wallR_GO = new GameObject("RightWall");
                    wallR_GO.transform.SetParent(transform, false);
                }
                wallR_GO.tag = "Wall";
                if (!wallR_GO.TryGetComponent(out edgeColliderR))
                {
                    edgeColliderR = wallR_GO.AddComponent<EdgeCollider2D>();
                }
            }
        }

        public void Configure(
            Vector2 startPos,
            float startHeading,
            ChunkDefinition def,
            MicroZone zone,
            Material roadMaterial,
            Sprite towerSprite,
            Sprite shopSprite,
            Sprite lampSprite,
            Sprite vmSprite,
            float roadWidth,
            Sprite pineSprite = null,
            Sprite cliffSprite = null,
            Sprite guardrailSprite = null,
            Sprite chevronSprite = null)
        {
            EnsureComponents(roadMaterial);

            chunkType = def.type;
            microZone = zone;
            entryPosition = startPos;
            entryHeading = startHeading;
            lengthMeters = def.length;
            curvatureDelta = def.curvature;
            driftDirection = def.driftDirection;
            trafficDensity = def.trafficDensity;

            transform.position = startPos;
            transform.rotation = Quaternion.identity;

            // 1. Calculate centerline ribbon slices
            int sliceCount = Mathf.Max(3, Mathf.CeilToInt(lengthMeters / MESH_STEP) + 1);
            float stepDist = lengthMeters / (sliceCount - 1);

            Vector2[] localPoints = new Vector2[sliceCount];
            Vector2[] localNormals = new Vector2[sliceCount];

            Vector2 curLocalPos = Vector2.zero;
            float curHeading = startHeading;

            for (int i = 0; i < sliceCount; i++)
            {
                float angleDeltaThisStep;
                if (def.isCompoundCurve)
                {
                    int half = sliceCount / 2;
                    float sign = (i < half) ? 1f : -1f;
                    angleDeltaThisStep = (def.curvature / (half > 0 ? half : 1)) * sign;
                }
                else
                {
                    angleDeltaThisStep = curvatureDelta / (sliceCount - 1);
                }

                if (i > 0)
                {
                    curHeading += angleDeltaThisStep;
                }

                Quaternion rot = Quaternion.Euler(0f, 0f, -curHeading);
                Vector2 forwardDir = rot * Vector2.up;
                Vector2 rightDir = rot * Vector2.right;

                localPoints[i] = curLocalPos;
                localNormals[i] = rightDir;

                if (i < sliceCount - 1)
                {
                    curLocalPos += forwardDir * stepDist;
                }
            }

            exitPosition = startPos + curLocalPos;
            exitHeading = curHeading;

            // 2. Build or update continuous 2D procedural ribbon mesh
            BuildProceduralMesh(localPoints, localNormals, sliceCount);

            // 3. Update continuous boundary EdgeCollider2D (no internal corners or ghost vertices)
            Vector2[] ptsL = new Vector2[sliceCount];
            Vector2[] ptsR = new Vector2[sliceCount];
            float halfRoad = roadWidth * 0.5f;

            for (int i = 0; i < sliceCount; i++)
            {
                ptsL[i] = localPoints[i] - localNormals[i] * halfRoad;
                ptsR[i] = localPoints[i] + localNormals[i] * halfRoad;
            }

            edgeColliderL.points = ptsL;
            edgeColliderR.points = ptsR;

            // 4. Sample and store query steps for traffic and positioning
            int querySteps = Mathf.Max(2, Mathf.CeilToInt(lengthMeters / QUERY_STEP));
            if (stepPositions == null || stepPositions.Length != querySteps)
            {
                stepPositions = new Vector2[querySteps];
                stepRotations = new Quaternion[querySteps];
            }

            for (int q = 0; q < querySteps; q++)
            {
                float t = (float)q / (querySteps > 1 ? (querySteps - 1) : 1);
                int sliceIdx = Mathf.Clamp(Mathf.RoundToInt(t * (sliceCount - 1)), 0, sliceCount - 1);
                stepPositions[q] = startPos + localPoints[sliceIdx];
                stepRotations[q] = Quaternion.Euler(0f, 0f, -Mathf.Lerp(startHeading, exitHeading, t));
            }

            // 5. Populate and position mountain touge props
            DecorateProps(localPoints, localNormals, sliceCount, zone, towerSprite, shopSprite, lampSprite, vmSprite, halfRoad, pineSprite, cliffSprite, guardrailSprite, chevronSprite);

            gameObject.SetActive(true);
        }

        private void BuildProceduralMesh(Vector2[] pts, Vector2[] normals, int count)
        {
            int vertCount = count * 18;
            Vector3[] verts = new Vector3[vertCount];
            Color[] colors = new Color[vertCount];
            Vector2[] uvs = new Vector2[vertCount];

            for (int i = 0; i < count; i++)
            {
                Vector2 p = pts[i];
                Vector2 n = normals[i];
                float vCoord = (float)i / (count - 1);

                for (int v = 0; v < 18; v++)
                {
                    int idx = i * 18 + v;
                    Vector2 pos = p + n * SLICE_OFFSETS[v];
                    verts[idx] = new Vector3(pos.x, pos.y, 0f);
                    colors[idx] = SLICE_COLORS[v];
                    uvs[idx] = new Vector2((float)v / 17f, vCoord);
                }
            }

            int segCount = count - 1;
            int triCount = segCount * 9 * 6;
            int[] triangles = new int[triCount];
            int triIdx = 0;

            for (int i = 0; i < segCount; i++)
            {
                int next = i + 1;
                for (int q = 0; q < 9; q++)
                {
                    int c0 = i * 18 + q * 2;
                    int c1 = i * 18 + q * 2 + 1;
                    int n0 = next * 18 + q * 2;
                    int n1 = next * 18 + q * 2 + 1;

                    triangles[triIdx++] = c0;
                    triangles[triIdx++] = n0;
                    triangles[triIdx++] = n1;

                    triangles[triIdx++] = c0;
                    triangles[triIdx++] = n1;
                    triangles[triIdx++] = c1;
                }
            }

            roadMesh.Clear();
            roadMesh.vertices = verts;
            roadMesh.colors = colors;
            roadMesh.uv = uvs;
            roadMesh.triangles = triangles;
            roadMesh.RecalculateNormals();
            roadMesh.RecalculateBounds();
        }

        private void DecorateProps(
            Vector2[] localPoints,
            Vector2[] localNormals,
            int sliceCount,
            MicroZone zone,
            Sprite towerSprite,
            Sprite shopSprite,
            Sprite lampSprite,
            Sprite vmSprite,
            float halfRoad,
            Sprite pineSprite,
            Sprite cliffSprite,
            Sprite guardrailSprite,
            Sprite chevronSprite)
        {
            // Deactivate existing active props into pool
            for (int i = 0; i < activeProps.Count; i++)
            {
                activeProps[i].SetActive(false);
                propPool.Add(activeProps[i]);
            }
            activeProps.Clear();

            Color lampColor = new Color(0f, 0.9f, 1f); // Alpine night cyan highway glow

            for (int i = 1; i < sliceCount - 1; i++)
            {
                Vector2 pt = localPoints[i];
                Vector2 nrm = localNormals[i];

                // 1. Guardrails along highway edge (both sides or outer precipice)
                if (guardrailSprite != null && i % 2 == 0)
                {
                    SpawnProp(guardrailSprite, pt - nrm * (halfRoad + 0.6f), Color.white, -3, new Vector3(1.1f, 1.1f, 1f));
                    SpawnProp(guardrailSprite, pt + nrm * (halfRoad + 0.6f), Color.white, -3, new Vector3(1.1f, 1.1f, 1f));
                }

                // 2. Touge Drift Chevron Signs on Sharp Curves
                if (chevronSprite != null && Mathf.Abs(curvatureDelta) > 15f && i % 4 == 1)
                {
                    float side = (driftDirection < 0) ? -1f : 1f;
                    SpawnProp(chevronSprite, pt + nrm * (halfRoad + 2.0f) * side, Color.white, -2, new Vector3(1.2f, 1.2f, 1f));
                }

                // 3. Mountain Rocky Cliff Wall (Inner Mountain Slope side)
                if (cliffSprite != null && i % 3 == 0)
                {
                    float cliffOffset = halfRoad + 4.5f + (i % 2) * 1.5f;
                    SpawnProp(cliffSprite, pt - nrm * cliffOffset, new Color(0.85f, 0.85f, 0.95f), -8, new Vector3(1.6f, 1.6f, 1f));
                }

                // 4. Dense Alpine Mountain Pines (Both sides, forest slopes)
                if (pineSprite != null && i % 2 == 1)
                {
                    float treeDistL = halfRoad + 5.0f + (i % 3) * 2.0f;
                    float treeDistR = halfRoad + 6.0f + ((i + 1) % 3) * 2.5f;
                    SpawnProp(pineSprite, pt - nrm * treeDistL, Color.white, -7, new Vector3(1.4f, 1.5f, 1f));
                    SpawnProp(pineSprite, pt + nrm * treeDistR, Color.white, -7, new Vector3(1.3f, 1.4f, 1f));
                }

                // 5. Alpine Highway Cobra Street Lamps (Periodic cyan illumination)
                if (i % 4 == 0 && lampSprite != null)
                {
                    SpawnProp(lampSprite, pt - nrm * (halfRoad + 1.8f), lampColor, -4, new Vector3(1.2f, 1.2f, 1f));
                    SpawnProp(lampSprite, pt + nrm * (halfRoad + 1.8f), lampColor, -4, new Vector3(1.2f, 1.2f, 1f));
                }
            }
        }

        private void SpawnProp(Sprite sprite, Vector2 localPos, Color color, int sortOrder, Vector3? scale = null)
        {
            GameObject propGO;
            if (propPool.Count > 0)
            {
                propGO = propPool[propPool.Count - 1];
                propPool.RemoveAt(propPool.Count - 1);
            }
            else
            {
                propGO = new GameObject("CurbsideProp");
                propGO.transform.SetParent(transform, false);
                propGO.AddComponent<SpriteRenderer>();
            }

            propGO.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
            propGO.transform.localRotation = Quaternion.identity;
            propGO.transform.localScale = scale ?? Vector3.one;

            SpriteRenderer sr = propGO.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortOrder;

            propGO.SetActive(true);
            activeProps.Add(propGO);
        }

        public Bounds GetWorldBounds()
        {
            if (meshRenderer != null)
            {
                return meshRenderer.bounds;
            }
            return new Bounds(transform.position, new Vector3(100f, lengthMeters * 1.5f, 1f));
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
            for (int i = 0; i < activeProps.Count; i++)
            {
                activeProps[i].SetActive(false);
                propPool.Add(activeProps[i]);
            }
            activeProps.Clear();

            gameObject.SetActive(false);
        }

        public bool TryGetPointAtDistance(float distanceAlongChunk, out Vector2 center, out Quaternion rot)
        {
            center = entryPosition;
            rot = Quaternion.Euler(0f, 0f, -entryHeading);

            if (stepPositions == null || stepPositions.Length == 0) return false;

            int stepIdx = Mathf.Clamp(Mathf.FloorToInt(distanceAlongChunk / QUERY_STEP), 0, stepPositions.Length - 1);
            center = stepPositions[stepIdx];
            rot = stepRotations[stepIdx];
            return true;
        }
    }
}

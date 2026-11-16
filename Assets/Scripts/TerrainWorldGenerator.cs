using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace CalmWoods
{
    // Same world rules as WorldGenerator, but the ground is a real Unity Terrain painted with
    // TerrainLayer assets (the textures of your asset pack).
    //
    // Rule order:
    //   1. Heights  : flat clearing, gentle hills, mountain ring at the edges, lake bowl
    //   2. Dock     : optional, on the shore facing the player
    //   3. Forest   : noise clusters (coverage %) minus empty patches (meadows)
    //   4. Splatmap : rock on high/steep ground, dirt on the lake shore and under forests, grass elsewhere
    //   5. Props    : tree and rock prefabs placed on the terrain surface
    //
    // Coordinates: the world is centered on this object. (0, 0, 0) is the player's spot.
    public class TerrainWorldGenerator : MonoBehaviour
    {
        [Header("Seed")]
        public bool randomSeed = true;
        public int seed = 12345;

        [Header("Terrain size")]
        public float worldSize = 160f;                       // Meters per side
        [Tooltip("Must be 2^n + 1 (129, 257, 513). 257 is a good default for Quest.")]
        public int heightmapResolution = 257;
        [Tooltip("Must be a power of two. 256 is enough for Quest.")]
        public int alphamapResolution = 256;
        public float terrainHeight = 40f;                    // Vertical range of the TerrainData
        public float baseOffset = 4f;                        // Terrain sits this far below y = 0 so the lake can go below water

        [Header("Terrain layers (TerrainLayer assets from your pack)")]
        [FormerlySerializedAs("grassLayer")]
        public TerrainLayer grassLightLayer;                 // Required. Meadows, clearing, open ground
        public TerrainLayer grassDarkLayer;                  // Optional. Under forests and in random darker patches
        public TerrainLayer dirtLayer;                       // Optional. Lake shore and small patches under trees
        public TerrainLayer rockLayer;                       // Optional. High and steep ground
        [Tooltip("How much darker grass shows up in random patches outside forests (0 = only under forests).")]
        [Range(0f, 1f)] public float grassVariation = 0.6f;
        public float grassPatchScale = 0.06f;                // Smaller = bigger light/dark patches
        [Tooltip("Optional. Leave empty to use the render pipeline's default terrain material.")]
        public Material terrainMaterial;

        [Header("Relief")]
        public float mountainHeight = 18f;
        [Range(0.3f, 0.95f)] public float mountainStart = 0.6f;
        public float hillHeight = 1.2f;
        public float hillScale = 0.05f;
        public float ridgeScale = 0.04f;
        public float rockLineHeight = 7f;                    // Above this height the ground is rock
        public float rockSlopeDegrees = 38f;                 // Steeper than this is rock
        public float treeLineHeight = 6f;                    // No trees above this height
        public float maxTreeSlopeDegrees = 30f;

        [Header("Player clearing")]
        public float clearingRadius = 6f;
        public float clearingBlend = 6f;

        [Header("Lake")]
        public bool generateLake = true;
        public float lakeRadius = 14f;
        public Vector2 lakeDistanceRange = new Vector2(16f, 28f);
        public float lakeDepth = 2.5f;
        public float waterLevel = -0.4f;
        [Tooltip("Shore band around the lake, as a fraction of the lake radius. 0.25 = 25% beyond the water line.")]
        [Range(0f, 0.6f)] public float shoreMargin = 0.25f;
        public Material waterMaterial;
        public bool lakeHasDock = true;
        public float dockLength = 5f;
        public float dockWidth = 1.4f;
        public float dockHeightAboveWater = 0.35f;
        [Tooltip("Optional. Its forward axis (+Z) must point toward the water.")]
        public GameObject dockPrefab;
        public Material dockMaterial;

        [Header("Forest")]
        [Range(0f, 1f)] public float forestCoverage = 0.45f;        // Fraction of eligible land covered by forest
        public float clusterScale = 0.07f;                          // Smaller = bigger clusters
        [Range(0.01f, 0.3f)] public float clusterEdgeSoftness = 0.08f;
        [Tooltip("Trees per 100 m2 inside a full-density forest. The world is big, so think in area, not in cells.")]
        [Range(0f, 40f)] public float treesPer100m2 = 8f;
        public float placementCellSize = 4f;                        // Meters per forest cell
        public Vector2 treeScaleRange = new Vector2(0.8f, 1.2f);
        public int maxTrees = 600;                                  // Hard performance cap
        [Range(0f, 1f)] public float forestDirtAmount = 0.25f;      // How much the ground under forests turns to dirt

        [Header("Empty spaces (meadows)")]
        [Range(0f, 1f)] public float emptyPatchAmount = 0.15f;
        public float emptyPatchScale = 0.09f;

        [Header("Rocks")]
        [Range(0f, 1f)] public float rockChance = 0.04f;            // Per cell
        public Vector2 rockScaleRange = new Vector2(0.7f, 1.8f);

        public enum GrassMode
        {
            [Tooltip("Drawn by GrassInstancedRenderer (GPU instancing in chunks). Does not depend on the Terrain detail system.")]
            InstancedDraw,
            [Tooltip("Painted into the Terrain's detail layer. Fewer scripts, but depends on Unity's terrain detail rendering.")]
            TerrainDetails
        }

        [Header("Grass (instanced, culled by distance)")]
        public GrassMode grassMode = GrassMode.InstancedDraw;
        [Tooltip("Prefab with a MeshFilter + MeshRenderer on its ROOT (like grass01). Its material needs 'Enable GPU Instancing' (turned on automatically). Leave empty for no grass.")]
        public GameObject grassPrefab;
        [Tooltip("InstancedDraw only. Hard limit on tufts in the whole world, so the grass never eats all the memory.")]
        public int maxGrassInstances = 120000;
        [Tooltip("Grass blades per square meter on fully grassy ground, before the density scale below.")]
        [Range(0f, 20f)] public float grassPerM2 = 6f;
        [Tooltip("Bigger tufts cover more ground per instance, so a lush look costs fewer instances. Try 1.0 - 1.8.")]
        public Vector2 grassScaleRange = new Vector2(1f, 1.6f);
        [Tooltip("Detail map resolution. Multiple of 16. 512 over 160 m = one cell every 0.3 m (needed for dense grass: max 16 per cell).")]
        public int detailResolution = 512;
        [Tooltip("0 = even lawn, 1 = clumps with bare spots (looks much more natural).")]
        [Range(0f, 1f)] public float grassClumpiness = 0.5f;
        public float grassClumpScale = 0.3f;                 // Higher = smaller clumps
        [Tooltip("Grass thinner than this ground weight is skipped, so there is no grass on dirt, path edges or rock.")]
        [Range(0f, 0.75f)] public float grassMinGroundWeight = 0.35f;
        [Range(0f, 1f)] public float grassUnderDarkFactor = 0.8f;   // Relative density on the dark (forest) grass layer
        [Tooltip("Grass farther than this from the camera is not drawn. This is the main Quest performance knob.")]
        public float grassDrawDistance = 25f;
        [Tooltip("Global multiplier applied at runtime. Lower it if the frame rate drops.")]
        [Range(0f, 1f)] public float grassDensityScale = 0.5f;

        [Header("Dirt path (from the player's clearing to the dock / lake)")]
        public bool generatePath = true;
        public float pathWidth = 2.2f;                       // Meters of full dirt
        [Tooltip("Width of the soft transition from dirt to grass on each side.")]
        public float pathEdgeSoftness = 0.9f;
        [Tooltip("Side-to-side wandering in meters. 0 = straight line. 5-9 looks like a trail people walked.")]
        public float pathMeander = 3.5f;
        [Tooltip("The trail leaves the clearing, weaves through the whole flat area and ends at the dock.")]
        public bool pathExplore = true;
        [Tooltip("Stops between the clearing and the dock. More stops = a longer trail that covers more of the world.")]
        [Range(0, 12)] public int pathStops = 6;
        [Tooltip("How far out the trail goes, as a fraction of the flat part of the world (before the mountains).")]
        [Range(0.3f, 0.95f)] public float pathReach = 0.85f;
        [Tooltip("Grass never grows closer than this to the dirt (meters from the path edge).")]
        public float grassPathMargin = 0.8f;
        [Tooltip("Smaller = longer, lazier curves.")]
        public float pathMeanderScale = 0.05f;
        [Tooltip("How much the path is worn into the ground (meters).")]
        public float pathDepth = 0.08f;
        [Tooltip("Tree trunks stay this far from the path edge. Your trees have thick trunks (about 2 m wide), so keep it above 3.")]
        public float pathTreeClearance = 3.5f;

        [Header("Prefab pools (use prefabs with LODGroup and GPU instancing)")]
        public GameObject[] treePrefabs;
        public GameObject[] rockPrefabs;

        [Header("Bushes (small trees / plants on the ground)")]
        [Tooltip("For example TreeSapling_02B or ForestPlant_B_02. Leave empty for no bushes.")]
        public GameObject[] bushPrefabs;
        [Tooltip("Bushes per 100 m2 of grassy ground.")]
        [Range(0f, 20f)] public float bushesPer100m2 = 3f;
        [Tooltip("0 = bushes spread evenly, 1 = only in clumps (natural).")]
        [Range(0f, 1f)] public float bushClumpiness = 0.6f;
        [Tooltip("Extra bushes near forests (0 = none, 1 = twice as many).")]
        [Range(0f, 1f)] public float bushForestBoost = 0.5f;
        public Vector2 bushScaleRange = new Vector2(0.6f, 1.1f);
        public int maxBushes = 300;                          // Hard performance cap
        public float bushPathClearance = 0.6f;               // Bushes may grow closer to the path than trees

        [Header("Quest performance")]
        public bool terrainCastsShadows = false;
        public float heightmapPixelError = 8f;               // Higher = fewer terrain triangles
        public float basemapDistance = 40f;                  // Beyond this, the terrain uses a cheap blended texture

        // ---------- Runtime state ----------
        float half;
        System.Random rng;
        Vector2 offHill, offRidge, offLake, offForest, offEmpty, offGrass, offPath, offBush;
        Vector2 lakeCenter;
        Vector2 pathTarget;                                  // Where the path ends (the dock, or a random point)
        bool hasPathTarget;
        readonly List<Vector2> pathPoints = new List<Vector2>();
        float forestThreshold = float.MaxValue;
        float emptyThreshold = float.MaxValue;
        readonly List<Vector3> exclusions = new List<Vector3>();   // x, z = position, y = radius
        Transform worldRoot;
        Terrain terrain;

        // Diagnostics: why spawn attempts failed (printed in the Console after generating)
        int rejClearing, rejLake, rejHeight, rejSlope, rejDock, rejPath;
        int grassInstances;                                  // Grass tufts written into the detail map

        void Start()
        {
            Generate();
        }

        [ContextMenu("Generate World")]
        public void Generate()
        {
            if (grassLightLayer == null)
            {
                Debug.LogError("TerrainWorldGenerator: assign at least the Grass Light layer.");
                return;
            }

            ClearWorld();

            half = worldSize * 0.5f;
            if (randomSeed) seed = Random.Range(int.MinValue, int.MaxValue);
            rng = new System.Random(seed);

            offHill = RandomOffset();
            offRidge = RandomOffset();
            offLake = RandomOffset();
            offForest = RandomOffset();
            offEmpty = RandomOffset();
            offGrass = RandomOffset();
            offPath = RandomOffset();
            offBush = RandomOffset();

            exclusions.Clear();
            pathPoints.Clear();
            pathGrid = null;
            grassInstances = 0;
            hasPathTarget = false;
            rejClearing = rejLake = rejHeight = rejSlope = rejDock = rejPath = 0;
            worldRoot = new GameObject("GeneratedWorld").transform;
            worldRoot.SetParent(transform, false);

            PickLakeCenter();
            PlaceDock();             // First, so the forest and rocks avoid it
            BuildPath();             // Needs the dock position; before anything that reads heights or ground
            ComputeForestThresholds();
            BuildTerrain();
            BuildWater();
            PlaceProps();
            if (grassMode == GrassMode.InstancedDraw) BuildGrassInstanced();
        }

        void ClearWorld()
        {
            Transform old = transform.Find("GeneratedWorld");
            if (old == null) return;

#if UNITY_EDITOR
            // If something inside the old world is selected, the Inspector keeps pointing at it after it is
            // destroyed and throws "SerializedObjectNotCreatableException". Clear the selection first.
            foreach (Transform selected in UnityEditor.Selection.transforms)
            {
                if (selected != null && selected.IsChildOf(old))
                {
                    UnityEditor.Selection.activeObject = null;
                    break;
                }
            }
#endif

            if (Application.isPlaying)
            {
                old.name = "GeneratedWorld_old";
                old.gameObject.SetActive(false);
                Destroy(old.gameObject);
            }
            else
            {
                DestroyImmediate(old.gameObject);
            }
        }

        // Destroy() is not allowed in edit mode (Generate World from the context menu) and
        // DestroyImmediate() should not be used while playing, so pick the right one.
        static void Discard(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Destroy(obj);
            else DestroyImmediate(obj);
        }

        Vector2 RandomOffset()
        {
            return new Vector2((float)rng.NextDouble() * 1000f, (float)rng.NextDouble() * 1000f);
        }

        // ================= 1. Heights =================

        void PickLakeCenter()
        {
            if (!generateLake) return;

            float minD = clearingRadius + lakeRadius * 0.8f + 2f;
            float maxD = mountainStart * half - lakeRadius * 0.9f;
            float lo = Mathf.Max(lakeDistanceRange.x, minD);
            float hi = Mathf.Min(lakeDistanceRange.y, maxD);
            if (hi < lo)
            {
                Debug.LogWarning("TerrainWorldGenerator: the lake does not fit between the clearing and the mountains. Reduce lakeRadius or enlarge the world.");
                hi = lo;
            }

            float dist = Mathf.Lerp(lo, hi, (float)rng.NextDouble());
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            lakeCenter = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        // How far a point is from the lake, relative to the lake's own (wobbly) outline:
        // 0 = center, 1 = the edge of the bowl, > 1 = outside. Large when there is no lake.
        float LakeRatio(float x, float z)
        {
            if (!generateLake) return 999f;
            float dx = x - lakeCenter.x;
            float dz = z - lakeCenter.y;
            float dl = Mathf.Sqrt(dx * dx + dz * dz);
            float angle = Mathf.Atan2(dz, dx);
            float wobble = Mathf.PerlinNoise(Mathf.Cos(angle) * 1.5f + offLake.x, Mathf.Sin(angle) * 1.5f + offLake.y);
            float r = lakeRadius * Mathf.Lerp(0.75f, 1.25f, wobble);
            return dl / r;
        }

        // Height in meters relative to y = 0 (the lake bottom is negative).
        float ComputeHeight(float x, float z)
        {
            float d = Mathf.Sqrt(x * x + z * z);

            float flat = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(clearingRadius, clearingRadius + clearingBlend, d));
            float h = Mathf.PerlinNoise(x * hillScale + offHill.x, z * hillScale + offHill.y) * hillHeight * flat;

            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(mountainStart * half, half, d));
            float ridge = Mathf.Lerp(0.55f, 1.15f, Mathf.PerlinNoise(x * ridgeScale + offRidge.x, z * ridgeScale + offRidge.y));
            float jagged = (Mathf.PerlinNoise(x * 0.15f + offRidge.y, z * 0.15f + offRidge.x) - 0.5f) * 0.25f;
            h += t * mountainHeight * (ridge + jagged);

            if (generateLake)
            {
                float k = 1f - Mathf.SmoothStep(0f, 1f, LakeRatio(x, z));   // 1 at the center, 0 at the rim
                h = Mathf.Lerp(h, -lakeDepth, k);
            }

            // A trail is slightly worn into the ground.
            if (pathPoints.Count > 1) h -= pathDepth * PathMask(x, z);

            return h;
        }

        float SlopeDegrees(float x, float z)
        {
            float dx = (ComputeHeight(x + 1f, z) - ComputeHeight(x - 1f, z)) * 0.5f;
            float dz = (ComputeHeight(x, z + 1f) - ComputeHeight(x, z - 1f)) * 0.5f;
            return Mathf.Atan(Mathf.Sqrt(dx * dx + dz * dz)) * Mathf.Rad2Deg;
        }

        // ================= Path =================

        // Builds the trail in three steps:
        //   1. Waypoints: the player, (optionally) two points in the forest around the lake, and the dock.
        //   2. A smooth Catmull-Rom curve through them, so there are no sharp corners.
        //   3. A little Perlin wiggle sideways, fading in/out at both ends, so it looks walked and not drawn.
        void BuildPath()
        {
            if (!generatePath) return;

            float reach = Mathf.Max(clearingRadius + 8f, pathReach * mountainStart * half);
            float baseAngle = hasPathTarget
                ? Mathf.Atan2(pathTarget.y, pathTarget.x)
                : (float)rng.NextDouble() * Mathf.PI * 2f;
            Vector2 end = hasPathTarget
                ? pathTarget
                : new Vector2(Mathf.Cos(baseAngle), Mathf.Sin(baseAngle)) * reach;

            var waypoints = new List<Vector2> { Vector2.zero };
            if (pathExplore && pathStops > 0)
            {
                // Stops sweep a full circle around the world, alternating between an outer and an inner
                // ring, so the trail weaves through the whole flat area and finally comes back to the dock.
                float sweep = 360f / (pathStops + 1);
                for (int stop = 0; stop < pathStops; stop++)
                {
                    float jitter = ((float)rng.NextDouble() - 0.5f) * sweep * 0.5f;
                    float angle = baseAngle + (sweep * (stop + 1) + jitter) * Mathf.Deg2Rad;
                    float ring = (stop % 2 == 0) ? 1f : 0.45f;
                    float dist = reach * ring * Mathf.Lerp(0.9f, 1.05f, (float)rng.NextDouble());
                    Vector2 p = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

                    // Never put a stop in the water: turn the point around the world until it is dry.
                    for (int tries = 0; tries < 12 && generateLake && LakeRatio(p.x, p.y) < 1.5f; tries++)
                    {
                        angle += 25f * Mathf.Deg2Rad;
                        p = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                    }
                    waypoints.Add(p);
                }
            }
            waypoints.Add(end);

            if (waypoints.Count == 2 && (end - waypoints[0]).magnitude < clearingRadius + 2f) return;   // Too short

            // 2. Catmull-Rom curve, one sample about every 2 m.
            var curve = new List<Vector2>();
            int n = waypoints.Count;
            for (int i = 0; i < n - 1; i++)
            {
                Vector2 p0 = waypoints[Mathf.Max(i - 1, 0)];
                Vector2 p1 = waypoints[i];
                Vector2 p2 = waypoints[i + 1];
                Vector2 p3 = waypoints[Mathf.Min(i + 2, n - 1)];
                int steps = Mathf.Max(4, Mathf.CeilToInt((p2 - p1).magnitude / 2f));
                for (int s = (i == 0 ? 0 : 1); s <= steps; s++)
                {
                    float u = s / (float)steps;
                    curve.Add(0.5f * (2f * p1 + (-p0 + p2) * u
                        + (2f * p0 - 5f * p1 + 4f * p2 - p3) * u * u
                        + (-p0 + 3f * p1 - 3f * p2 + p3) * u * u * u));
                }
            }

            // Keep the trail out of the lake: push samples that fall in the water or on its bank outward,
            // then smooth so there are no kinks. The last samples (the approach to the dock) are left alone.
            if (generateLake)
            {
                for (int i = 1; i < curve.Count - 5; i++)
                {
                    Vector2 p = curve[i];
                    float ratio = LakeRatio(p.x, p.y);
                    if (ratio >= 0.95f) continue;

                    Vector2 fromLake = p - lakeCenter;
                    float distNow = fromLake.magnitude;
                    Vector2 outward = distNow > 0.01f ? fromLake / distNow : Vector2.right;
                    curve[i] = lakeCenter + outward * (distNow / Mathf.Max(ratio, 0.05f));   // Exactly on the lake edge
                }
                for (int pass = 0; pass < 2; pass++)
                {
                    for (int i = 1; i < curve.Count - 1; i++)
                        curve[i] = (curve[i - 1] + curve[i] * 2f + curve[i + 1]) * 0.25f;
                }
            }

            // Total length, to fade the wiggle at both ends.
            float total = 0f;
            for (int i = 1; i < curve.Count; i++) total += (curve[i] - curve[i - 1]).magnitude;

            // 3. Sideways wiggle.
            float along = 0f;
            for (int i = 0; i < curve.Count; i++)
            {
                if (i > 0) along += (curve[i] - curve[i - 1]).magnitude;

                Vector2 tangent = curve[Mathf.Min(i + 1, curve.Count - 1)] - curve[Mathf.Max(i - 1, 0)];
                if (tangent.sqrMagnitude < 1e-6f) tangent = Vector2.right;
                tangent.Normalize();
                Vector2 perp = new Vector2(-tangent.y, tangent.x);

                float noise = Mathf.PerlinNoise(along * pathMeanderScale + offPath.x, offPath.y) * 2f - 1f;
                float taper = Mathf.Clamp01(along / 6f) * Mathf.Clamp01((total - along) / 6f);
                pathPoints.Add(curve[i] + perp * (noise * pathMeander * taper));
            }

            BuildPathDistanceGrid();
        }

        // The distance to the path is asked millions of times while painting and placing things,
        // so it is computed once on a coarse grid and then read with bilinear interpolation.
        const float PathGridCell = 0.5f;
        float[,] pathGrid;
        int pathGridSize;

        void BuildPathDistanceGrid()
        {
            pathGridSize = Mathf.CeilToInt(worldSize / PathGridCell) + 2;
            pathGrid = new float[pathGridSize, pathGridSize];
            for (int i = 0; i < pathGridSize; i++)
            {
                for (int j = 0; j < pathGridSize; j++)
                {
                    pathGrid[i, j] = ExactDistanceToPath(-half + i * PathGridCell, -half + j * PathGridCell);
                }
            }
        }

        // Distance from a point to the path, in meters. Large when there is no path.
        float DistanceToPath(float x, float z)
        {
            if (pathGrid == null) return 999f;

            float fx = Mathf.Clamp((x + half) / PathGridCell, 0f, pathGridSize - 1.001f);
            float fz = Mathf.Clamp((z + half) / PathGridCell, 0f, pathGridSize - 1.001f);
            int ix = (int)fx;
            int iz = (int)fz;
            float tx = fx - ix;
            float tz = fz - iz;
            float d0 = Mathf.Lerp(pathGrid[ix, iz], pathGrid[ix + 1, iz], tx);
            float d1 = Mathf.Lerp(pathGrid[ix, iz + 1], pathGrid[ix + 1, iz + 1], tx);
            return Mathf.Lerp(d0, d1, tz);
        }

        float ExactDistanceToPath(float x, float z)
        {
            if (pathPoints.Count < 2) return 999f;

            float best = float.MaxValue;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Vector2 a = pathPoints[i];
                Vector2 b = pathPoints[i + 1];
                float abx = b.x - a.x;
                float abz = b.y - a.y;
                float t = Mathf.Clamp01(((x - a.x) * abx + (z - a.y) * abz) / (abx * abx + abz * abz));
                float px = a.x + abx * t - x;
                float pz = a.y + abz * t - z;
                float d2 = px * px + pz * pz;
                if (d2 < best) best = d2;
            }
            return Mathf.Sqrt(best);
        }

        // 1 on the path, fading to 0 across pathEdgeSoftness.
        float PathMask(float x, float z)
        {
            float d = DistanceToPath(x, z);
            float halfWidth = pathWidth * 0.5f;
            return 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(halfWidth, halfWidth + pathEdgeSoftness, d));
        }

        // ================= 2. Dock =================

        void PlaceDock()
        {
            if (!generateLake || !lakeHasDock) return;

            // March from the lake center toward the player until the ground rises above the water.
            Vector2 toPlayer = (-lakeCenter).normalized;
            float s = 0f;
            for (; s < lakeRadius * 2.5f; s += 0.5f)
            {
                Vector2 p = lakeCenter + toPlayer * s;
                if (ComputeHeight(p.x, p.y) > waterLevel + 0.15f) break;
            }

            Vector2 shore = lakeCenter + toPlayer * s;
            Vector3 dir = new Vector3(-toPlayer.x, 0f, -toPlayer.y);   // From the shore toward the lake center
            Vector3 start = new Vector3(shore.x, waterLevel + dockHeightAboveWater, shore.y) - dir * 1.0f;

            // The path ends at the land side of the dock.
            pathTarget = new Vector2(start.x, start.z);
            hasPathTarget = true;

            // Keep trees and rocks away from the dock.
            for (float t = -1f; t <= dockLength + 2f; t += 1f)
            {
                Vector3 p = start + dir * t;
                exclusions.Add(new Vector3(p.x, p.z, dockWidth + 1.5f));
            }

            if (dockPrefab != null)
            {
                GameObject dock = Instantiate(dockPrefab, worldRoot);
                dock.transform.localPosition = start;
                dock.transform.localRotation = Quaternion.LookRotation(dir, Vector3.up);
            }
            else
            {
                BuildDockMesh(start, dir);   // Placeholder made of planks and posts
            }
        }

        // Builds a simple wooden dock: deck planks, side beams and posts going down into the water.
        GameObject BuildDockMesh(Vector3 start, Vector3 dir)
        {
            var root = new GameObject("Dock");
            root.transform.SetParent(worldRoot, false);
            root.transform.localPosition = start;
            root.transform.localRotation = Quaternion.LookRotation(dir, Vector3.up);

            Material wood = dockMaterial != null
                ? dockMaterial
                : MakeFallbackMaterial("Dock wood (auto)", new Color(0.45f, 0.30f, 0.17f), 0.1f);
            Material darkWood = dockMaterial != null
                ? dockMaterial
                : MakeFallbackMaterial("Dock posts (auto)", new Color(0.28f, 0.18f, 0.10f), 0.05f);

            // Local space: +Z goes from the shore toward the lake center.
            const float plankLength = 0.28f;
            const float plankGap = 0.04f;
            const float plankThickness = 0.08f;
            float step = plankLength + plankGap;
            int plankCount = Mathf.Max(2, Mathf.CeilToInt(dockLength / step));

            for (int i = 0; i < plankCount; i++)
            {
                AddDockPart(root.transform, "Plank", wood,
                    new Vector3(0f, 0f, i * step + plankLength * 0.5f),
                    new Vector3(dockWidth, plankThickness, plankLength));
            }

            float length = plankCount * step;
            for (int side = -1; side <= 1; side += 2)
            {
                AddDockPart(root.transform, "Beam", darkWood,
                    new Vector3(side * (dockWidth * 0.5f - 0.1f), -plankThickness - 0.05f, length * 0.5f),
                    new Vector3(0.12f, 0.1f, length));
            }

            // Posts every ~2.5 m, long enough to reach below the water surface.
            float postHeight = dockHeightAboveWater + 1.2f;
            int postRows = Mathf.Max(2, Mathf.CeilToInt(length / 2.5f) + 1);
            for (int r = 0; r < postRows; r++)
            {
                float z = Mathf.Min(length - 0.1f, r * 2.5f);
                for (int side = -1; side <= 1; side += 2)
                {
                    AddDockPart(root.transform, "Post", darkWood,
                        new Vector3(side * (dockWidth * 0.5f - 0.1f), -postHeight * 0.5f, z),
                        new Vector3(0.16f, postHeight, 0.16f));
                }
            }
            return root;
        }

        static void AddDockPart(Transform parent, string name, Material mat, Vector3 localPos, Vector3 localScale)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            Discard(part.GetComponent<Collider>());   // Visual only
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPos;
            part.transform.localScale = localScale;
            var r = part.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        // Creates a flat-colored URP Simple Lit material when none is assigned in the Inspector.
        // Note: Shader.Find can fail in builds if the shader is not included, so for the final
        // build assign real materials (or add Simple Lit to Always Included Shaders).
        static Material MakeFallbackMaterial(string name, Color color, float smoothness)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { name = name };
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetColor("_SpecColor", new Color(0.6f, 0.6f, 0.6f, 1f));
            return mat;
        }

        // ================= 3. Forest noise =================

        float ForestNoise(float x, float z)
        {
            return Mathf.PerlinNoise(x * clusterScale + offForest.x, z * clusterScale + offForest.y);
        }

        float EmptyNoise(float x, float z)
        {
            return Mathf.PerlinNoise(x * emptyPatchScale + offEmpty.x, z * emptyPatchScale + offEmpty.y);
        }

        // The lake shore is decided by DISTANCE TO THE LAKE, not by absolute height.
        // (Using height here was the bug: flat ground is only ~0-1 m high, so it counted as "shore"
        // and trees could only appear on the slopes near the mountains, which looked like a ring.)
        bool IsNearLake(float x, float z)
        {
            return generateLake && LakeRatio(x, z) < 1f + shoreMargin;
        }

        // Returns true when trees may grow here. Fills the diagnostic counters when they may not.
        bool IsTreeGround(float x, float z, float height, bool count)
        {
            if (Mathf.Sqrt(x * x + z * z) < clearingRadius + 1f) { if (count) rejClearing++; return false; }
            if (IsNearLake(x, z)) { if (count) rejLake++; return false; }
            if (DistanceToPath(x, z) < pathWidth * 0.5f + pathTreeClearance) { if (count) rejPath++; return false; }
            if (height > treeLineHeight) { if (count) rejHeight++; return false; }
            if (SlopeDegrees(x, z) > maxTreeSlopeDegrees) { if (count) rejSlope++; return false; }
            return true;
        }

        // Thresholds come from percentiles of the noise over the land that can hold trees,
        // so "coverage = 0.45" means about 45% of that land, not 45% of the noise range.
        void ComputeForestThresholds()
        {
            var forestValues = new List<float>();
            var emptyValues = new List<float>();
            int cells = Mathf.CeilToInt(worldSize / placementCellSize);

            for (int i = 0; i < cells; i++)
            {
                for (int j = 0; j < cells; j++)
                {
                    float x = -half + (i + 0.5f) * placementCellSize;
                    float z = -half + (j + 0.5f) * placementCellSize;
                    if (!IsTreeGround(x, z, ComputeHeight(x, z), false)) continue;
                    forestValues.Add(ForestNoise(x, z));
                    emptyValues.Add(EmptyNoise(x, z));
                }
            }

            forestThreshold = forestCoverage > 0f ? Percentile(forestValues, 1f - forestCoverage) : float.MaxValue;
            emptyThreshold = emptyPatchAmount > 0f ? Percentile(emptyValues, 1f - emptyPatchAmount) : float.MaxValue;
        }

        static float Percentile(List<float> values, float fraction)
        {
            if (values.Count == 0) return float.MaxValue;
            values.Sort();
            int index = Mathf.Clamp(Mathf.RoundToInt(fraction * (values.Count - 1)), 0, values.Count - 1);
            return values[index];
        }

        // 0 = no forest, 1 = full density. Empty patches win over forest.
        float ForestDensity(float x, float z)
        {
            if (EmptyNoise(x, z) >= emptyThreshold) return 0f;
            float n = ForestNoise(x, z);
            float soft = clusterEdgeSoftness;
            return Mathf.Clamp01(Mathf.InverseLerp(forestThreshold - soft * 0.5f, forestThreshold + soft, n));
        }

        // ================= Terrain build =================

        void BuildTerrain()
        {
            var data = new TerrainData();
            data.heightmapResolution = heightmapResolution;
            data.size = new Vector3(worldSize, terrainHeight, worldSize);

            // Heights: SetHeights wants values 0..1 indexed [z, x].
            int res = data.heightmapResolution;
            var heights = new float[res, res];
            for (int zi = 0; zi < res; zi++)
            {
                float z = Mathf.Lerp(-half, half, zi / (float)(res - 1));
                for (int xi = 0; xi < res; xi++)
                {
                    float x = Mathf.Lerp(-half, half, xi / (float)(res - 1));
                    heights[zi, xi] = Mathf.Clamp01((ComputeHeight(x, z) + baseOffset) / terrainHeight);
                }
            }
            data.SetHeights(0, 0, heights);

            // Layers
            // A missing optional layer falls back to the light grass, so the world never shows holes.
            var used = new List<TerrainLayer> { grassLightLayer };
            int lightIndex = 0;
            int darkIndex = grassDarkLayer != null ? AddLayer(used, grassDarkLayer) : lightIndex;
            int dirtIndex = dirtLayer != null ? AddLayer(used, dirtLayer) : lightIndex;
            int rockIndex = rockLayer != null ? AddLayer(used, rockLayer) : lightIndex;
            data.terrainLayers = used.ToArray();

            // Splatmap: SetAlphamaps wants [z, x, layer].
            data.alphamapResolution = alphamapResolution;
            int ares = data.alphamapResolution;
            var alphas = new float[ares, ares, used.Count];
            for (int zi = 0; zi < ares; zi++)
            {
                float z = Mathf.Lerp(-half, half, zi / (float)(ares - 1));
                for (int xi = 0; xi < ares; xi++)
                {
                    float x = Mathf.Lerp(-half, half, xi / (float)(ares - 1));
                    GroundWeights(x, z, out float rock, out float dirt, out float dark, out float light);

                    // "+=" because a missing optional layer shares the light grass index.
                    alphas[zi, xi, rockIndex] += rock;
                    alphas[zi, xi, dirtIndex] += dirt;
                    alphas[zi, xi, darkIndex] += dark;
                    alphas[zi, xi, lightIndex] += light;
                }
            }
            data.SetAlphamaps(0, 0, alphas);

            bool hasGrass = grassMode == GrassMode.TerrainDetails && BuildGrass(data);

            // Terrain object (comes with a TerrainCollider).
            GameObject go = Terrain.CreateTerrainGameObject(data);
            go.name = "Terrain";
            go.transform.SetParent(worldRoot, false);
            go.transform.localPosition = new Vector3(-half, -baseOffset, -half);

            terrain = go.GetComponent<Terrain>();
            if (terrainMaterial != null) terrain.materialTemplate = terrainMaterial;
            terrain.heightmapPixelError = heightmapPixelError;
            terrain.basemapDistance = basemapDistance;
            terrain.drawInstanced = true;
            // Trees are prefabs, so "trees and foliage" is only needed for the grass details.
            terrain.drawTreesAndFoliage = hasGrass;
            terrain.detailObjectDistance = grassDrawDistance;
            terrain.detailObjectDensity = grassDensityScale;
            terrain.shadowCastingMode = terrainCastsShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }

        // The four ground weights at a point. They always add up to 1:
        //   rock  : high or steep ground
        //   dirt  : lake shore, plus a little under forests
        //   dark  : darker grass under forests and in random patches
        //   light : lighter grass everywhere else (clearing, meadows)
        void GroundWeights(float x, float z, out float rock, out float dirt, out float dark, out float light)
        {
            float h = ComputeHeight(x, z);
            float slope = SlopeDegrees(x, z);

            rock = Mathf.Max(
                Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(rockLineHeight - 1.5f, rockLineHeight + 1.5f, h)),
                Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(rockSlopeDegrees - 5f, rockSlopeDegrees + 5f, slope)));

            // Shore: 1 inside the lake and its margin, fading to 0 a bit further out.
            float shore = generateLake
                ? 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.95f, 1f + shoreMargin, LakeRatio(x, z)))
                : 0f;

            float forest = ForestDensity(x, z);
            float patch = Mathf.PerlinNoise(x * grassPatchScale + offGrass.x, z * grassPatchScale + offGrass.y);
            float darkAmount = Mathf.Clamp01(forest * 0.9f + Mathf.SmoothStep(0.4f, 0.7f, patch) * grassVariation);

            float rest = 1f - rock;
            dirt = rest * Mathf.Max(shore, forest * forestDirtAmount);
            float grass = rest - dirt;
            dark = grass * darkAmount;
            light = grass - dark;

            // The path paints over everything: each weight shrinks and dirt takes the difference.
            // The weights still add up to 1.
            if (pathPoints.Count > 1)
            {
                float path = PathMask(x, z);
                if (path > 0f)
                {
                    float keep = 1f - path;
                    rock *= keep;
                    dark *= keep;
                    light *= keep;
                    dirt = dirt * keep + path;
                }
            }
        }

        // 0..1: how much grass this spot should have. Shared by both grass modes.
        // Only the light / dark grass layers carry grass: path, shore, rock and the dock area get none.
        float GrassDensity(float x, float z)
        {
            if (IsExcluded(x, z)) return 0f;                              // Keep the dock clear
            if (ComputeHeight(x, z) < waterLevel + 0.15f) return 0f;      // Not under water

            // Never on the dirt path or right next to it.
            if (pathPoints.Count > 1 && DistanceToPath(x, z) < pathWidth * 0.5f + grassPathMargin) return 0f;

            GroundWeights(x, z, out float rock, out float dirt, out float dark, out float light);

            // Never on bare ground (shore, dirt patches): above ~50% dirt there is no grass at all.
            float bare = 1f - Mathf.SmoothStep(0.15f, 0.5f, dirt);
            if (bare <= 0f) return 0f;

            // Full density from (min + 0.25) up. Under a forest the ground is already partly dirt,
            // so its grass weight is ~0.75 and must still count as "grass".
            float grassWeight = light + dark;
            float onGrass = Mathf.SmoothStep(grassMinGroundWeight, grassMinGroundWeight + 0.25f, grassWeight);
            if (onGrass <= 0f) return 0f;

            // Dark grass (under forests) is a bit sparser than light grass.
            float darkShare = dark / Mathf.Max(grassWeight, 0.001f);
            float density = onGrass * Mathf.Lerp(1f, grassUnderDarkFactor, darkShare);

            // Clumps: noise that leaves bare spots between lush tufts.
            float clump = Mathf.PerlinNoise(x * grassClumpScale + offGrass.y, z * grassClumpScale + offGrass.x);
            return density * bare * Mathf.Lerp(1f, Mathf.SmoothStep(0.25f, 0.65f, clump), grassClumpiness);
        }

        // Mode "InstancedDraw": the grass is drawn by GrassInstancedRenderer (GPU instancing, chunked,
        // culled by distance). It does not depend on the Terrain's detail system. Needs the terrain to exist.
        void BuildGrassInstanced()
        {
            if (grassPrefab == null || grassPerM2 <= 0f) return;

            var meshFilter = grassPrefab.GetComponent<MeshFilter>();
            var meshRenderer = grassPrefab.GetComponent<MeshRenderer>();
            if (meshFilter == null || meshFilter.sharedMesh == null || meshRenderer == null || meshRenderer.sharedMaterial == null)
            {
                Debug.LogWarning("TerrainWorldGenerator: the grass prefab needs a MeshFilter (with a mesh) and a MeshRenderer (with a material) on its ROOT object.");
                return;
            }

            Material mat = meshRenderer.sharedMaterial;
            if (!mat.enableInstancing)
            {
                mat.enableInstancing = true;
                Debug.LogWarning($"TerrainWorldGenerator: GPU Instancing was off on '{mat.name}'. It is now on.");
            }

            // Pass 1: grass density per sample cell (0.5 m cells).
            const float cell = 0.5f;
            int cellsPerSide = Mathf.CeilToInt(worldSize / cell);
            var densities = new float[cellsPerSide * cellsPerSide];
            double expected = 0.0;
            float perCell = grassPerM2 * grassDensityScale * cell * cell;
            for (int zi = 0; zi < cellsPerSide; zi++)
            {
                float z = -half + (zi + 0.5f) * cell;
                for (int xi = 0; xi < cellsPerSide; xi++)
                {
                    float d = GrassDensity(-half + (xi + 0.5f) * cell, z);
                    densities[zi * cellsPerSide + xi] = d;
                    expected += d * perCell;
                }
            }

            // Hard cap: if we would exceed it, thin everything out evenly.
            float keep = expected > maxGrassInstances ? (float)(maxGrassInstances / expected) : 1f;

            var go = new GameObject("Grass (instanced)");
            go.transform.SetParent(worldRoot, false);
            var grass = go.AddComponent<GrassInstancedRenderer>();
            grass.Begin(meshFilter.sharedMesh, mat, 16f, grassDrawDistance);

            // Pass 2: place the tufts on the real terrain surface.
            Vector3 rootPos = worldRoot.position;
            Quaternion prefabRot = grassPrefab.transform.localRotation;
            Vector3 prefabScale = grassPrefab.transform.localScale;
            int placed = 0;
            for (int zi = 0; zi < cellsPerSide; zi++)
            {
                for (int xi = 0; xi < cellsPerSide; xi++)
                {
                    float d = densities[zi * cellsPerSide + xi];
                    if (d <= 0f) continue;

                    int count = Mathf.FloorToInt(d * perCell * keep + (float)rng.NextDouble());
                    for (int k = 0; k < count; k++)
                    {
                        float x = -half + (xi + (float)rng.NextDouble()) * cell;
                        float z = -half + (zi + (float)rng.NextDouble()) * cell;
                        var world = new Vector3(x, 0f, z) + rootPos;
                        float y = terrain.SampleHeight(world) + terrain.transform.position.y - 0.02f;

                        Quaternion rot = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f) * prefabRot;
                        float s = Mathf.Lerp(grassScaleRange.x, grassScaleRange.y, (float)rng.NextDouble());
                        grass.Add(new Vector3(world.x, y, world.z), rot, prefabScale * s);
                        placed++;
                    }
                }
            }
            grass.End();
            grassInstances = placed;

            Debug.Log($"TerrainWorldGenerator: grass (instanced) {placed} tufts in {grass.ChunkCount} chunks " +
                      $"(wanted about {expected:0}, cap {maxGrassInstances}); drawn within {grassDrawDistance} m of each camera.");
        }

        // Mode "TerrainDetails": fills the terrain's detail layer with grass instances.
        // Returns false if there is no grass prefab.
        // The terrain draws them itself: instanced, culled by distance, no GameObjects created.
        bool BuildGrass(TerrainData data)
        {
            if (grassPrefab == null || grassPerM2 <= 0f) return false;

            if (grassPrefab.GetComponent<MeshFilter>() == null || grassPrefab.GetComponent<MeshRenderer>() == null)
            {
                Debug.LogWarning("TerrainWorldGenerator: the grass prefab needs a MeshFilter and MeshRenderer on its root object.");
                return false;
            }

            // With instancing on, the terrain draws nothing if the grass material does not allow it.
            Material grassMaterial = grassPrefab.GetComponent<MeshRenderer>().sharedMaterial;
            if (grassMaterial == null)
            {
                Debug.LogWarning("TerrainWorldGenerator: the grass prefab has no material.");
                return false;
            }
            if (!grassMaterial.enableInstancing)
            {
                grassMaterial.enableInstancing = true;
                Debug.LogWarning($"TerrainWorldGenerator: GPU Instancing was off on '{grassMaterial.name}'. It is now on (needed for terrain grass).");
            }

            int res = Mathf.Max(16, (detailResolution / 16) * 16);
            data.SetDetailResolution(res, 16);

            var proto = new DetailPrototype
            {
                prototype = grassPrefab,
                usePrototypeMesh = true,
                useInstancing = true,
                renderMode = DetailRenderMode.VertexLit,
                minWidth = grassScaleRange.x,
                maxWidth = grassScaleRange.y,
                minHeight = grassScaleRange.x,
                maxHeight = grassScaleRange.y,
                noiseSpread = 0.3f
            };
            data.detailPrototypes = new[] { proto };

            float cellSize = worldSize / res;
            float perCell = grassPerM2 * cellSize * cellSize;

            // SetDetailLayer wants [z, x] and an integer count per cell (max 16 for mesh details).
            var map = new int[res, res];
            for (int zi = 0; zi < res; zi++)
            {
                float z = -half + (zi + 0.5f) * cellSize;
                for (int xi = 0; xi < res; xi++)
                {
                    float x = -half + (xi + 0.5f) * cellSize;
                    if (IsExcluded(x, z)) continue;                    // Keep the dock clear
                    if (ComputeHeight(x, z) < waterLevel + 0.15f) continue;   // Not under water

                    float density = GrassDensity(x, z);
                    if (density <= 0f) continue;

                    int count = Mathf.FloorToInt(density * perCell + (float)rng.NextDouble());
                    map[zi, xi] = Mathf.Min(count, 16);
                    grassInstances += map[zi, xi];
                }
            }
            data.SetDetailLayer(0, 0, 0, map);
            return true;
        }

        static int AddLayer(List<TerrainLayer> list, TerrainLayer layer)
        {
            int existing = list.IndexOf(layer);
            if (existing >= 0) return existing;
            list.Add(layer);
            return list.Count - 1;
        }

        void BuildWater()
        {
            if (!generateLake) return;

            // One flat plane. The terrain above water level hides the excess.
            GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);   // 10 x 10 m by default
            water.name = "Water";
            Discard(water.GetComponent<Collider>());
            water.transform.SetParent(worldRoot, false);
            water.transform.localPosition = new Vector3(lakeCenter.x, waterLevel, lakeCenter.y);
            float size = lakeRadius * 3f;
            water.transform.localScale = new Vector3(size / 10f, 1f, size / 10f);

            var waterRenderer = water.GetComponent<MeshRenderer>();
            waterRenderer.sharedMaterial = waterMaterial != null
                ? waterMaterial
                : MakeFallbackMaterial("Water (auto)", new Color(0.12f, 0.38f, 0.45f), 0.85f);
            waterRenderer.shadowCastingMode = ShadowCastingMode.Off;
            waterRenderer.receiveShadows = false;
        }

        // ================= 4. Props =================

        bool IsExcluded(float x, float z)
        {
            foreach (Vector3 e in exclusions)
            {
                float dx = x - e.x;
                float dz = z - e.y;
                if (dx * dx + dz * dz < e.z * e.z) return true;
            }
            return false;
        }

        void PlaceProps()
        {
            var propsRoot = new GameObject("Props").transform;
            propsRoot.SetParent(worldRoot, false);

            bool hasTrees = HasPrefabs(treePrefabs);
            bool hasRocks = HasPrefabs(rockPrefabs);
            if (!hasTrees) Debug.LogWarning("TerrainWorldGenerator: no tree prefabs assigned, the forest will be empty.");

            int cells = Mathf.CeilToInt(worldSize / placementCellSize);

            // Visit the cells in random order. With a fixed order, the maxTrees cap would fill one
            // side of the world first and leave the rest empty.
            var order = new List<int>(cells * cells);
            for (int n = 0; n < cells * cells; n++) order.Add(n);
            for (int n = order.Count - 1; n > 0; n--)
            {
                int k = rng.Next(n + 1);
                int tmp = order[n]; order[n] = order[k]; order[k] = tmp;
            }

            float cellArea = placementCellSize * placementCellSize;
            float treesPerFullCell = treesPer100m2 * cellArea / 100f;

            bool hasBushes = HasPrefabs(bushPrefabs);
            float bushesPerCell = bushesPer100m2 * cellArea / 100f;

            int trees = 0;
            int rocks = 0;
            int bushes = 0;
            int forestCells = 0;

            foreach (int index in order)
            {
                int i = index / cells;
                int j = index % cells;
                float cx = -half + (i + 0.5f) * placementCellSize;
                float cz = -half + (j + 0.5f) * placementCellSize;

                if (hasTrees && trees < maxTrees)
                {
                    float density = ForestDensity(cx, cz);
                    if (density > 0f)
                    {
                        forestCells++;
                        int count = Mathf.FloorToInt(treesPerFullCell * density + (float)rng.NextDouble());
                        for (int k = 0; k < count && trees < maxTrees; k++)
                        {
                            if (TrySpawn(propsRoot, treePrefabs, treeScaleRange, cx, cz, true)) trees++;
                        }
                    }
                }

                if (hasRocks && rng.NextDouble() < rockChance)
                {
                    if (TrySpawn(propsRoot, rockPrefabs, rockScaleRange, cx, cz, false)) rocks++;
                }

                if (hasBushes && bushes < maxBushes)
                {
                    // Clumpy density: bushes gather in groups and are a bit more common near forests.
                    float clump = Mathf.SmoothStep(0.35f, 0.65f, Mathf.PerlinNoise(cx * 0.12f + offBush.x, cz * 0.12f + offBush.y));
                    float factor = Mathf.Lerp(1f, clump, bushClumpiness) * (1f + bushForestBoost * ForestDensity(cx, cz));
                    int count = Mathf.FloorToInt(bushesPerCell * factor + (float)rng.NextDouble());
                    for (int k = 0; k < count && bushes < maxBushes; k++)
                    {
                        if (TrySpawnBush(propsRoot, cx, cz)) bushes++;
                    }
                }
            }

            Debug.Log($"TerrainWorldGenerator: seed {seed}, {worldSize} m world, {trees} trees (cap {maxTrees}), {rocks} rocks, " +
                      $"{bushes} bushes (cap {maxBushes}), {forestCells} forest cells. Tree spawn rejected by: clearing {rejClearing}, " +
                      $"lake {rejLake}, path {rejPath}, height {rejHeight}, slope {rejSlope}, dock {rejDock}. Path points: {pathPoints.Count}. " +
                      $"Grass: {grassInstances} tufts in the map (drawn: about {Mathf.RoundToInt(grassInstances * grassDensityScale)} within {grassDrawDistance} m of the camera at most).");
        }

        static bool HasPrefabs(GameObject[] pool)
        {
            if (pool == null) return false;
            foreach (GameObject p in pool) if (p != null) return true;
            return false;
        }

        bool TrySpawn(Transform parent, GameObject[] pool, Vector2 scaleRange, float cx, float cz, bool needsTreeGround)
        {
            float x = cx + ((float)rng.NextDouble() - 0.5f) * placementCellSize * 0.9f;
            float z = cz + ((float)rng.NextDouble() - 0.5f) * placementCellSize * 0.9f;
            if (IsExcluded(x, z)) { rejDock++; return false; }

            float h = ComputeHeight(x, z);
            if (needsTreeGround)
            {
                if (!IsTreeGround(x, z, h, true)) return false;
            }
            else
            {
                // Rocks can sit on slopes, but not in the clearing, in the water or on the path.
                if (Mathf.Sqrt(x * x + z * z) < clearingRadius || h < waterLevel + 0.3f) return false;
                if (DistanceToPath(x, z) < pathWidth * 0.5f + 0.5f) { rejPath++; return false; }
            }

            return SpawnPrefab(parent, pool, scaleRange, x, z);
        }

        // Bushes: small prefabs on open grass. Never in the clearing, on the path, near the lake,
        // on steep ground or on bare (dirt / rock) ground.
        bool TrySpawnBush(Transform parent, float cx, float cz)
        {
            float x = cx + ((float)rng.NextDouble() - 0.5f) * placementCellSize * 0.95f;
            float z = cz + ((float)rng.NextDouble() - 0.5f) * placementCellSize * 0.95f;

            if (IsExcluded(x, z)) return false;
            if (Mathf.Sqrt(x * x + z * z) < clearingRadius + 0.5f) return false;
            if (IsNearLake(x, z)) return false;
            if (DistanceToPath(x, z) < pathWidth * 0.5f + bushPathClearance) return false;
            if (ComputeHeight(x, z) > treeLineHeight) return false;
            if (SlopeDegrees(x, z) > maxTreeSlopeDegrees) return false;

            GroundWeights(x, z, out float rock, out float dirt, out float dark, out float light);
            if (light + dark < 0.75f) return false;

            return SpawnPrefab(parent, bushPrefabs, bushScaleRange, x, z);
        }

        // Picks a prefab from the pool and places it on the real terrain surface.
        bool SpawnPrefab(Transform parent, GameObject[] pool, Vector2 scaleRange, float x, float z)
        {
            GameObject prefab = null;
            for (int tries = 0; tries < 5 && prefab == null; tries++) prefab = pool[rng.Next(pool.Length)];
            if (prefab == null) return false;

            // SampleHeight is relative to the terrain's own y.
            var world = new Vector3(x, 0f, z) + worldRoot.position;
            float y = terrain.SampleHeight(world) + terrain.transform.position.y - 0.05f;

            GameObject go = Instantiate(prefab, parent);
            go.transform.position = new Vector3(world.x, y, world.z);
            go.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            go.transform.localScale = prefab.transform.localScale * Mathf.Lerp(scaleRange.x, scaleRange.y, (float)rng.NextDouble());
            return true;
        }
    }
}
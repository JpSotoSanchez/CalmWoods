using System.Collections.Generic;
using UnityEngine;

namespace CalmWoods
{
    // Generates the whole Calm Woods world from a square grid of cells.
    //
    // Rule order (each step only touches what earlier steps left free):
    //   1. Heights   : flat clearing in the center, gentle hills, mountain ring at the edges, lake carved in.
    //   2. Cells     : water -> clearing -> rock/snow -> shore -> grass
    //   3. Dock      : optional, placed on the shore cell closest to the player
    //   4. Forest    : noise-based clusters (coverage %), minus "empty patches" (meadows)
    //   5. Props     : trees, rocks (all share one material, so static batching merges them)
    //
    // Coordinates: the world is centered on this object. (0, 0, 0) is the player's spot.
    public class WorldGenerator : MonoBehaviour
    {
        [Header("Seed")]
        public bool randomSeed = true;
        public int seed = 12345;

        [Header("Grid")]
        [Range(16, 128)] public int gridSize = 64;   // Cells per side
        public float cellSize = 2f;                  // Meters per cell (64 x 2 = 128 m wide world)

        [Header("Materials")]
        [Tooltip("Any URP material (Simple Lit or Unlit recommended on Quest). The palette texture is assigned by the script.")]
        public Material groundMaterial;
        [Tooltip("Optional. If empty, water uses the ground material with a flat blue palette color.")]
        public Material waterMaterial;
        public bool generateColliders = true;
        public bool staticBatchProps = true;         // Props need Read/Write enabled if they are your own prefabs

        [Header("Terrain")]
        public float mountainHeight = 18f;
        [Range(0.3f, 0.95f)] public float mountainStart = 0.6f;  // Fraction of the half-world where mountains start rising
        public float hillHeight = 1.2f;
        public float hillScale = 0.05f;              // Noise frequency
        public float ridgeScale = 0.04f;
        public float rockLineHeight = 5.5f;          // Ground turns to rock above this height (meters)
        public float snowLineHeight = 15f;           // ...and to snow above this one
        public float treeLineHeight = 4f;            // No trees above this height

        [Header("Player clearing")]
        public float clearingRadius = 6f;            // Flat, empty area around the player
        public float clearingBlend = 6f;             // Distance over which hills fade in

        [Header("Lake")]
        public bool generateLake = true;
        public float lakeRadius = 14f;
        public Vector2 lakeDistanceRange = new Vector2(16f, 28f);   // Distance from the player to the lake center
        public float lakeDepth = 2.5f;
        public float waterLevel = -0.4f;
        public bool lakeHasDock = true;
        public float dockLength = 5f;
        public float dockWidth = 1.4f;
        public float dockHeightAboveWater = 0.35f;

        [Header("Forest")]
        [Range(0f, 1f)] public float forestCoverage = 0.45f;        // Fraction of eligible land covered by forest
        public float clusterScale = 0.07f;                          // Smaller = bigger clusters
        [Range(0.01f, 0.3f)] public float clusterEdgeSoftness = 0.08f;  // Small = hard forest edges, large = gradual
        [Range(0f, 3f)] public float treesPerCell = 1.2f;           // Density inside a forest
        public Vector2 treeScaleRange = new Vector2(0.8f, 1.3f);
        public int maxTrees = 1800;                                 // Hard performance cap

        [Header("Empty spaces (meadows)")]
        [Range(0f, 1f)] public float emptyPatchAmount = 0.15f;      // Fraction of eligible land kept free of trees
        public float emptyPatchScale = 0.09f;

        [Header("Rocks")]
        [Range(0f, 1f)] public float rockChance = 0.04f;            // Per cell; 4x on rocky slopes
        public Vector2 rockScaleRange = new Vector2(0.7f, 1.8f);

        [Header("Palette (one color per Palette enum entry)")]
        public Color[] paletteColors = DefaultPalette();

        [Header("Asset pools (optional - leave empty to use built-in low-poly placeholders)")]
        public GameObject[] treePrefabs;
        public GameObject[] rockPrefabs;
        public GameObject dockPrefab;

        // ---------- Runtime state ----------
        int N;
        float half;
        System.Random rng;
        Vector2 offHill, offRidge, offLake, offForest, offEmpty;
        Vector2 lakeCenter;
        float[,] vh;              // Vertex heights, (N+1) x (N+1)
        Palette[,] ground;        // Ground color per cell
        bool[,] isWater;
        bool[,] reserved;         // No props allowed (water, clearing, dock)
        float[,] forestDensity;   // 0 = no trees, 1 = full density
        Material runtimeMaterial;
        Mesh[] treeMeshes, rockMeshes;
        Transform worldRoot, propsRoot;

        static Color[] DefaultPalette()
        {
            return new[]
            {
                new Color(0.55f, 0.75f, 0.40f), // Grass
                new Color(0.30f, 0.50f, 0.28f), // ForestFloor
                new Color(0.50f, 0.50f, 0.52f), // Rock
                new Color(0.93f, 0.95f, 0.98f), // Snow
                new Color(0.85f, 0.78f, 0.58f), // Sand
                new Color(0.25f, 0.40f, 0.45f), // LakeBed
                new Color(0.65f, 0.82f, 0.48f), // Clearing
                new Color(0.42f, 0.30f, 0.20f), // Trunk
                new Color(0.20f, 0.45f, 0.25f), // Leaves
                new Color(0.60f, 0.45f, 0.30f), // Wood
                new Color(0.25f, 0.55f, 0.70f)  // Water
            };
        }

        void OnValidate()
        {
            // Keep the palette array in sync with the enum.
            if (paletteColors == null || paletteColors.Length != PaletteInfo.Count)
            {
                Color[] fixedPalette = DefaultPalette();
                if (paletteColors != null)
                {
                    for (int i = 0; i < Mathf.Min(paletteColors.Length, fixedPalette.Length); i++)
                        fixedPalette[i] = paletteColors[i];
                }
                paletteColors = fixedPalette;
            }
        }

        void Start()
        {
            Generate();
        }

        // ================= Entry point =================

        [ContextMenu("Generate World")]
        public void Generate()
        {
            ClearWorld();

            N = gridSize;
            half = N * cellSize * 0.5f;
            if (randomSeed) seed = Random.Range(int.MinValue, int.MaxValue);
            rng = new System.Random(seed);

            offHill = RandomOffset();
            offRidge = RandomOffset();
            offLake = RandomOffset();
            offForest = RandomOffset();
            offEmpty = RandomOffset();

            worldRoot = new GameObject("GeneratedWorld").transform;
            worldRoot.SetParent(transform, false);

            BuildMaterial();
            PickLakeCenter();
            BuildHeights();
            ClassifyCells();
            PlaceDock();
            BuildTerrainMesh();
            BuildWaterMesh();
            PlaceProps();
        }

        void ClearWorld()
        {
            Transform old = transform.Find("GeneratedWorld");
            if (old == null) return;

            if (Application.isPlaying)
            {
                old.name = "GeneratedWorld_old";   // Destroy() is deferred; avoid name clashes
                old.gameObject.SetActive(false);
                Destroy(old.gameObject);
            }
            else
            {
                DestroyImmediate(old.gameObject);
            }
        }

        Vector2 RandomOffset()
        {
            return new Vector2((float)rng.NextDouble() * 1000f, (float)rng.NextDouble() * 1000f);
        }

        // ================= Material =================

        void BuildMaterial()
        {
            if (groundMaterial != null)
            {
                runtimeMaterial = new Material(groundMaterial);
            }
            else
            {
                Shader s = Shader.Find("Universal Render Pipeline/Simple Lit");
                if (s == null) s = Shader.Find("Standard");
                runtimeMaterial = new Material(s);
                Debug.LogWarning("WorldGenerator: no ground material assigned. Assign one to avoid shader stripping in builds.");
            }

            // Tiny palette texture: one texel per color, point filtered so colors never blend.
            var tex = new Texture2D(PaletteInfo.Count, 1, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            for (int i = 0; i < PaletteInfo.Count; i++)
            {
                Color c = (paletteColors != null && i < paletteColors.Length) ? paletteColors[i] : Color.magenta;
                tex.SetPixel(i, 0, c);
            }
            tex.Apply();

            runtimeMaterial.mainTexture = tex;
            if (runtimeMaterial.HasProperty("_BaseColor")) runtimeMaterial.SetColor("_BaseColor", Color.white);
            if (runtimeMaterial.HasProperty("_Smoothness")) runtimeMaterial.SetFloat("_Smoothness", 0f);
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
                Debug.LogWarning("WorldGenerator: the lake does not fit between the clearing and the mountains. Reduce lakeRadius or enlarge the world.");
                hi = lo;
            }

            float dist = Mathf.Lerp(lo, hi, (float)rng.NextDouble());
            float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            lakeCenter = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        float ComputeHeight(float x, float z)
        {
            float d = Mathf.Sqrt(x * x + z * z);

            // Gentle hills, faded out near the player's clearing so the spot stays flat.
            float flat = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(clearingRadius, clearingRadius + clearingBlend, d));
            float h = Mathf.PerlinNoise(x * hillScale + offHill.x, z * hillScale + offHill.y) * hillHeight * flat;

            // Mountain ring that frames the scene and hides the world edge.
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(mountainStart * half, half, d));
            float ridge = Mathf.Lerp(0.55f, 1.15f, Mathf.PerlinNoise(x * ridgeScale + offRidge.x, z * ridgeScale + offRidge.y));
            float jagged = (Mathf.PerlinNoise(x * 0.15f + offRidge.y, z * 0.15f + offRidge.x) - 0.5f) * 0.25f;
            h += t * mountainHeight * (ridge + jagged);

            // Lake: carve a bowl with an irregular (noise-wobbled) outline.
            if (generateLake)
            {
                float dx = x - lakeCenter.x;
                float dz = z - lakeCenter.y;
                float dl = Mathf.Sqrt(dx * dx + dz * dz);
                float angle = Mathf.Atan2(dz, dx);
                float wobble = Mathf.PerlinNoise(Mathf.Cos(angle) * 1.5f + offLake.x, Mathf.Sin(angle) * 1.5f + offLake.y);
                float r = lakeRadius * Mathf.Lerp(0.75f, 1.25f, wobble);
                float k = 1f - Mathf.SmoothStep(0f, 1f, dl / r);   // 1 at the center, 0 at the rim
                h = Mathf.Lerp(h, -lakeDepth, k);
            }

            return h;
        }

        void BuildHeights()
        {
            vh = new float[N + 1, N + 1];
            for (int i = 0; i <= N; i++)
            {
                for (int j = 0; j <= N; j++)
                {
                    float x = (i - N * 0.5f) * cellSize;
                    float z = (j - N * 0.5f) * cellSize;
                    vh[i, j] = ComputeHeight(x, z);
                }
            }
        }

        // Bilinear height lookup used to sit props on the ground.
        float SampleHeight(float x, float z)
        {
            float fx = x / cellSize + N * 0.5f;
            float fz = z / cellSize + N * 0.5f;
            int ix = Mathf.Clamp((int)fx, 0, N - 1);
            int iz = Mathf.Clamp((int)fz, 0, N - 1);
            float tx = Mathf.Clamp01(fx - ix);
            float tz = Mathf.Clamp01(fz - iz);

            float h0 = Mathf.Lerp(vh[ix, iz], vh[ix + 1, iz], tx);
            float h1 = Mathf.Lerp(vh[ix, iz + 1], vh[ix + 1, iz + 1], tx);
            return Mathf.Lerp(h0, h1, tz);
        }

        Vector2 CellCenter(int i, int j)
        {
            return new Vector2((i + 0.5f - N * 0.5f) * cellSize, (j + 0.5f - N * 0.5f) * cellSize);
        }

        bool TryGetCell(float x, float z, out int i, out int j)
        {
            i = Mathf.FloorToInt(x / cellSize + N * 0.5f);
            j = Mathf.FloorToInt(z / cellSize + N * 0.5f);
            return i >= 0 && i < N && j >= 0 && j < N;
        }

        // ================= 2. Cell classification =================

        void ClassifyCells()
        {
            ground = new Palette[N, N];
            isWater = new bool[N, N];
            reserved = new bool[N, N];
            forestDensity = new float[N, N];

            var eligible = new bool[N, N];   // Can hold trees

            // Pass 1: water, clearing, rock/snow, grass
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    Vector2 c = CellCenter(i, j);
                    float avg = (vh[i, j] + vh[i + 1, j] + vh[i, j + 1] + vh[i + 1, j + 1]) * 0.25f;

                    if (avg < waterLevel)
                    {
                        isWater[i, j] = true;
                        reserved[i, j] = true;
                        ground[i, j] = Palette.LakeBed;
                        continue;
                    }

                    if (c.magnitude < clearingRadius)
                    {
                        reserved[i, j] = true;
                        ground[i, j] = Palette.Clearing;
                        continue;
                    }

                    if (avg > snowLineHeight) ground[i, j] = Palette.Snow;
                    else if (avg > rockLineHeight) ground[i, j] = Palette.Rock;
                    else ground[i, j] = Palette.Grass;

                    eligible[i, j] = avg <= treeLineHeight;
                }
            }

            // Pass 2: shore (any land cell touching water) turns to sand and stays tree-free
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (isWater[i, j] || reserved[i, j]) continue;
                    if (TouchesWater(i, j))
                    {
                        ground[i, j] = Palette.Sand;
                        eligible[i, j] = false;
                    }
                }
            }

            // Pass 3: forest clusters and empty patches. Thresholds come from percentiles of the noise,
            // so "coverage = 0.45" really means about 45% of the eligible land.
            var forestNoise = new float[N, N];
            var emptyNoise = new float[N, N];
            var forestValues = new List<float>();
            var emptyValues = new List<float>();

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (!eligible[i, j]) continue;
                    Vector2 c = CellCenter(i, j);
                    forestNoise[i, j] = Mathf.PerlinNoise(c.x * clusterScale + offForest.x, c.y * clusterScale + offForest.y);
                    emptyNoise[i, j] = Mathf.PerlinNoise(c.x * emptyPatchScale + offEmpty.x, c.y * emptyPatchScale + offEmpty.y);
                    forestValues.Add(forestNoise[i, j]);
                    emptyValues.Add(emptyNoise[i, j]);
                }
            }

            float forestThreshold = forestCoverage > 0f ? Percentile(forestValues, 1f - forestCoverage) : float.MaxValue;
            float emptyThreshold = emptyPatchAmount > 0f ? Percentile(emptyValues, 1f - emptyPatchAmount) : float.MaxValue;
            float soft = clusterEdgeSoftness;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (!eligible[i, j]) continue;

                    // Empty patches win over forest: they carve meadows out of the clusters.
                    if (emptyNoise[i, j] >= emptyThreshold) continue;

                    float n = forestNoise[i, j];
                    if (n >= forestThreshold - soft * 0.5f)
                    {
                        forestDensity[i, j] = Mathf.Clamp01(Mathf.InverseLerp(forestThreshold - soft * 0.5f, forestThreshold + soft, n));
                        ground[i, j] = Palette.ForestFloor;
                    }
                }
            }
        }

        bool TouchesWater(int i, int j)
        {
            for (int di = -1; di <= 1; di++)
            {
                for (int dj = -1; dj <= 1; dj++)
                {
                    int ni = i + di;
                    int nj = j + dj;
                    if (ni < 0 || nj < 0 || ni >= N || nj >= N) continue;
                    if (isWater[ni, nj]) return true;
                }
            }
            return false;
        }

        static float Percentile(List<float> values, float fraction)
        {
            if (values.Count == 0) return float.MaxValue;
            values.Sort();
            int index = Mathf.Clamp(Mathf.RoundToInt(fraction * (values.Count - 1)), 0, values.Count - 1);
            return values[index];
        }

        // ================= 3. Dock =================

        void PlaceDock()
        {
            if (!generateLake || !lakeHasDock) return;

            // Pick the shore cell closest to the player that has water directly beside it (4-neighborhood).
            int bestI = -1, bestJ = -1;
            float bestDist = float.MaxValue;
            Vector2 bestDir = Vector2.zero;
            int[] dis = { 1, -1, 0, 0 };
            int[] djs = { 0, 0, 1, -1 };

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (isWater[i, j] || ground[i, j] != Palette.Sand) continue;

                    Vector2 dir = Vector2.zero;
                    for (int k = 0; k < 4; k++)
                    {
                        int ni = i + dis[k];
                        int nj = j + djs[k];
                        if (ni < 0 || nj < 0 || ni >= N || nj >= N) continue;
                        if (isWater[ni, nj]) dir += new Vector2(dis[k], djs[k]);
                    }
                    if (dir == Vector2.zero) continue;

                    float dist = CellCenter(i, j).magnitude;
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestI = i;
                        bestJ = j;
                        bestDir = dir.normalized;
                    }
                }
            }

            if (bestI < 0)
            {
                Debug.LogWarning("WorldGenerator: no valid shore cell found for the dock.");
                return;
            }

            Vector2 c = CellCenter(bestI, bestJ);
            Vector3 forward = new Vector3(bestDir.x, 0f, bestDir.y);
            Vector3 start = new Vector3(c.x, waterLevel + dockHeightAboveWater, c.y) - forward * (cellSize * 0.5f);

            // Keep trees and rocks away from the dock (3x3 cells along its length).
            for (float s = -cellSize; s <= dockLength + cellSize; s += cellSize * 0.5f)
            {
                Vector3 p = start + forward * s;
                for (int di = -1; di <= 1; di++)
                {
                    for (int dj = -1; dj <= 1; dj++)
                    {
                        if (TryGetCell(p.x + di * cellSize, p.z + dj * cellSize, out int ci, out int cj))
                            reserved[ci, cj] = true;
                    }
                }
            }

            Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
            GameObject dock;
            if (dockPrefab != null)
            {
                dock = Instantiate(dockPrefab, worldRoot);
            }
            else
            {
                dock = new GameObject("Dock");
                dock.transform.SetParent(worldRoot, false);
                dock.AddComponent<MeshFilter>().sharedMesh = BuildDockMesh();
                dock.AddComponent<MeshRenderer>().sharedMaterial = runtimeMaterial;
            }
            dock.transform.localPosition = start;
            dock.transform.localRotation = rotation;
        }

        Mesh BuildDockMesh()
        {
            // Local space: the dock extends along +Z from the origin.
            var mb = new LowPolyMeshBuilder();
            mb.AddBox(new Vector3(0f, 0f, dockLength * 0.5f), new Vector3(dockWidth, 0.12f, dockLength), Palette.Wood);

            float postX = dockWidth * 0.5f - 0.1f;
            for (float z = 0.1f; z <= dockLength + 0.01f; z += 1.5f)
            {
                mb.AddBox(new Vector3(-postX, -0.8f, z), new Vector3(0.14f, 1.6f, 0.14f), Palette.Wood);
                mb.AddBox(new Vector3(postX, -0.8f, z), new Vector3(0.14f, 1.6f, 0.14f), Palette.Wood);
            }
            return mb.ToMesh("DockMesh");
        }

        // ================= Meshes =================

        void BuildTerrainMesh()
        {
            var mb = new LowPolyMeshBuilder();
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    float x0 = (i - N * 0.5f) * cellSize;
                    float x1 = x0 + cellSize;
                    float z0 = (j - N * 0.5f) * cellSize;
                    float z1 = z0 + cellSize;

                    var p00 = new Vector3(x0, vh[i, j], z0);
                    var p10 = new Vector3(x1, vh[i + 1, j], z0);
                    var p01 = new Vector3(x0, vh[i, j + 1], z1);
                    var p11 = new Vector3(x1, vh[i + 1, j + 1], z1);
                    Palette c = ground[i, j];

                    // Alternate the diagonal so the facets don't all line up.
                    if (((i + j) & 1) == 0)
                    {
                        mb.AddTriangle(p00, p01, p11, c, Vector3.up);
                        mb.AddTriangle(p00, p11, p10, c, Vector3.up);
                    }
                    else
                    {
                        mb.AddTriangle(p00, p01, p10, c, Vector3.up);
                        mb.AddTriangle(p10, p01, p11, c, Vector3.up);
                    }
                }
            }

            Mesh mesh = mb.ToMesh("Terrain");
            var go = new GameObject("Terrain");
            go.transform.SetParent(worldRoot, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = runtimeMaterial;
            if (generateColliders) go.AddComponent<MeshCollider>().sharedMesh = mesh;
        }

        void BuildWaterMesh()
        {
            int minI = int.MaxValue, minJ = int.MaxValue, maxI = int.MinValue, maxJ = int.MinValue;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (!isWater[i, j]) continue;
                    minI = Mathf.Min(minI, i);
                    maxI = Mathf.Max(maxI, i);
                    minJ = Mathf.Min(minJ, j);
                    maxJ = Mathf.Max(maxJ, j);
                }
            }
            if (minI == int.MaxValue) return;   // No water

            // One flat quad covering the lake bounds. Land above water level hides the excess.
            float x0 = (minI - 1 - N * 0.5f) * cellSize;
            float x1 = (maxI + 2 - N * 0.5f) * cellSize;
            float z0 = (minJ - 1 - N * 0.5f) * cellSize;
            float z1 = (maxJ + 2 - N * 0.5f) * cellSize;

            var mb = new LowPolyMeshBuilder();
            mb.AddQuad(
                new Vector3(x0, waterLevel, z0), new Vector3(x0, waterLevel, z1),
                new Vector3(x1, waterLevel, z1), new Vector3(x1, waterLevel, z0),
                Palette.Water, Vector3.up);

            var go = new GameObject("Water");
            go.transform.SetParent(worldRoot, false);
            go.AddComponent<MeshFilter>().sharedMesh = mb.ToMesh("Water");
            go.AddComponent<MeshRenderer>().sharedMaterial = waterMaterial != null ? waterMaterial : runtimeMaterial;
        }

        // ================= 5. Props =================

        void PlaceProps()
        {
            propsRoot = new GameObject("Props").transform;
            propsRoot.SetParent(worldRoot, false);
            CreatePlaceholderMeshes();

            int trees = 0;
            int rocks = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (reserved[i, j]) continue;
                    Vector2 c = CellCenter(i, j);

                    float f = forestDensity[i, j];
                    if (f > 0f && trees < maxTrees)
                    {
                        int count = Mathf.FloorToInt(treesPerCell * f + (float)rng.NextDouble());
                        for (int k = 0; k < count && trees < maxTrees; k++)
                        {
                            SpawnProp(c, treePrefabs, treeMeshes, treeScaleRange, "Tree");
                            trees++;
                        }
                    }

                    bool rocky = ground[i, j] == Palette.Rock || ground[i, j] == Palette.Snow;
                    if (rng.NextDouble() < rockChance * (rocky ? 4f : 1f))
                    {
                        SpawnProp(c, rockPrefabs, rockMeshes, rockScaleRange, "Rock");
                        rocks++;
                    }
                }
            }

            if (staticBatchProps) StaticBatchingUtility.Combine(propsRoot.gameObject);

            Debug.Log($"WorldGenerator: seed {seed}, {N}x{N} cells ({N * cellSize} m), {trees} trees, {rocks} rocks, {N * N * 2} terrain triangles.");
        }

        void SpawnProp(Vector2 cellCenter, GameObject[] prefabs, Mesh[] meshes, Vector2 scaleRange, string propName)
        {
            float x = cellCenter.x + ((float)rng.NextDouble() - 0.5f) * cellSize * 0.9f;
            float z = cellCenter.y + ((float)rng.NextDouble() - 0.5f) * cellSize * 0.9f;
            float y = SampleHeight(x, z) - 0.05f;   // Sink slightly so slopes never expose the base
            Quaternion rot = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
            float s = Mathf.Lerp(scaleRange.x, scaleRange.y, (float)rng.NextDouble());

            GameObject go;
            if (prefabs != null && prefabs.Length > 0 && prefabs[0] != null)
            {
                go = Instantiate(prefabs[rng.Next(prefabs.Length)], propsRoot);
            }
            else
            {
                go = new GameObject(propName);
                go.transform.SetParent(propsRoot, false);
                go.AddComponent<MeshFilter>().sharedMesh = meshes[rng.Next(meshes.Length)];
                go.AddComponent<MeshRenderer>().sharedMaterial = runtimeMaterial;
            }

            go.transform.localPosition = new Vector3(x, y, z);
            go.transform.localRotation = rot;
            go.transform.localScale = Vector3.one * s;
        }

        // ================= Placeholder meshes =================

        void CreatePlaceholderMeshes()
        {
            if (treePrefabs == null || treePrefabs.Length == 0 || treePrefabs[0] == null)
                treeMeshes = new[] { BuildPineMesh(), BuildBroadleafMesh() };

            if (rockPrefabs == null || rockPrefabs.Length == 0 || rockPrefabs[0] == null)
                rockMeshes = new[] { BuildRockMesh(5, 0.7f, 0.6f), BuildRockMesh(6, 0.9f, 0.75f) };
        }

        static Mesh BuildPineMesh()
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddBox(new Vector3(0f, 0.6f, 0f), new Vector3(0.3f, 1.2f, 0.3f), Palette.Trunk);
            mb.AddCone(new Vector3(0f, 1.0f, 0f), 1.5f, 2.8f, 6, Palette.Leaves);
            mb.AddCone(new Vector3(0f, 2.6f, 0f), 1.1f, 2.4f, 6, Palette.Leaves);
            mb.AddCone(new Vector3(0f, 4.0f, 0f), 0.7f, 2.0f, 6, Palette.Leaves);
            return mb.ToMesh("PinePlaceholder");
        }

        static Mesh BuildBroadleafMesh()
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddBox(new Vector3(0f, 1.0f, 0f), new Vector3(0.35f, 2.0f, 0.35f), Palette.Trunk);
            // Canopy: two cones joined at their bases (a low-poly "gem" shape)
            mb.AddCone(new Vector3(0f, 2.8f, 0f), 1.8f, 1.8f, 5, Palette.Leaves, false);
            mb.AddCone(new Vector3(0f, 2.8f, 0f), 1.8f, -1.4f, 5, Palette.Leaves, false);
            return mb.ToMesh("BroadleafPlaceholder");
        }

        static Mesh BuildRockMesh(int sides, float radius, float height)
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddCone(new Vector3(0f, height * 0.4f, 0f), radius, height * 0.6f, sides, Palette.Rock, false);
            mb.AddCone(new Vector3(0f, height * 0.4f, 0f), radius, -height * 0.4f, sides, Palette.Rock, false);
            return mb.ToMesh("RockPlaceholder");
        }
    }
}

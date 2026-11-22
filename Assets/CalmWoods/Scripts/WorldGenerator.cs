using System.Collections.Generic;
using UnityEngine;

namespace CalmWoods
{
    // Generates the whole Calm Woods world from a square grid of cells.
    //
    // Visual upgrade notes (Quest 2 safe):
    //   - 16-color palette with richer, more natural tones
    //   - Trees: 4 varieties (pine, broadleaf, birch, bush) with 8-10 sided geometry
    //   - Rocks: dome-only (no UFO diamonds), sunk into the ground
    //   - Wildflower and grass-tuft props on meadows and clearings
    //   - Automatic URP fog setup for depth and atmosphere
    //   - Auto-created translucent water material when none is assigned
    //   - Invisible boundary walls so the player cannot leave the world
    //   - Shallow lake (waist-deep wading, head never submerges)
    //   - Single shared material + static batching = one draw call for all props
    //
    // Rule order (each step only touches what earlier steps left free):
    //   1. Heights   : flat clearing in the center, gentle hills, mountain ring at the edges, lake carved in.
    //   2. Cells     : water -> clearing -> rock/snow -> shore -> grass
    //   3. Dock      : optional, placed on the shore cell closest to the player
    //   4. Forest    : noise-based clusters (coverage %), minus "empty patches" (meadows)
    //   5. Props     : trees, rocks, bushes, grass tufts, wildflowers
    //
    // Coordinates: the world is centered on this object. (0, 0, 0) is the player's spot.
    public class WorldGenerator : MonoBehaviour
    {
        [Header("Seed")]
        public bool randomSeed = true;
        public int seed = 12345;

        [Header("Grid")]
        [Range(16, 128)] public int gridSize = 48;      // Cells per side (48 x 2 = 96 m world — good Quest 2 size)
        public float cellSize = 2f;

        [Header("Materials")]
        [Tooltip("Any URP material (Simple Lit recommended on Quest). The palette texture is assigned by the script.")]
        public Material groundMaterial;
        [Tooltip("Optional. If empty, an auto-generated translucent water material is created.")]
        public Material waterMaterial;
        public bool generateColliders = true;
        public bool staticBatchProps = true;

        [Header("Terrain")]
        public float mountainHeight = 18f;
        [Range(0.3f, 0.95f)] public float mountainStart = 0.6f;
        public float hillHeight = 1.5f;
        public float hillScale = 0.05f;
        public float ridgeScale = 0.04f;
        public float rockLineHeight = 5.5f;
        public float snowLineHeight = 15f;
        public float treeLineHeight = 4f;

        [Header("Player clearing")]
        public float clearingRadius = 7f;
        public float clearingBlend = 7f;

        [Header("Lake")]
        public bool generateLake = true;
        public float lakeRadius = 14f;
        public Vector2 lakeDistanceRange = new Vector2(16f, 28f);
        public float lakeDepth = 1.0f;               // Shallow — waist-deep wading only
        public float waterLevel = -0.3f;
        public bool lakeHasDock = true;
        public float dockLength = 5f;
        public float dockWidth = 1.4f;
        public float dockHeightAboveWater = 0.35f;

        [Header("Forest")]
        [Range(0f, 1f)] public float forestCoverage = 0.50f;
        public float clusterScale = 0.07f;
        [Range(0.01f, 0.3f)] public float clusterEdgeSoftness = 0.08f;
        [Range(0f, 3f)] public float treesPerCell = 1.3f;
        public Vector2 treeScaleRange = new Vector2(0.8f, 1.35f);
        public int maxTrees = 2200;

        [Header("Empty spaces (meadows)")]
        [Range(0f, 1f)] public float emptyPatchAmount = 0.15f;
        public float emptyPatchScale = 0.09f;

        [Header("Rocks")]
        [Range(0f, 1f)] public float rockChance = 0.04f;
        public Vector2 rockScaleRange = new Vector2(0.7f, 1.8f);

        [Header("Bushes & ground details")]
        [Range(0f, 0.15f)] public float bushChance = 0.06f;
        [Range(0f, 0.2f)] public float grassTuftChance = 0.10f;
        [Range(0f, 0.08f)] public float wildflowerChance = 0.04f;
        public int maxGroundDetails = 1200;

        [Header("Play Area")]
        public bool buildBoundary = true;
        [Tooltip("How far inside the mountain ring the invisible walls are placed.")]
        public float boundaryMargin = 4f;
        [Tooltip("Assign your XR Origin here so the player spawns at the clearing height.")]
        public Transform playerRig;

        [Header("Atmosphere")]
        public bool setupFog = true;
        public Color fogColor = new Color(0.72f, 0.82f, 0.90f, 1f);
        public float fogStartDistance = 25f;
        public float fogEndDistance = 85f;

        [Header("Palette (one color per Palette enum entry)")]
        public Color[] paletteColors = DefaultPalette();

        [Header("Asset pools (optional — leave empty to use built-in low-poly placeholders)")]
        public GameObject[] treePrefabs;
        public GameObject[] rockPrefabs;
        public GameObject dockPrefab;

        // ---------- Runtime state ----------
        int N;
        float half;
        System.Random rng;
        Vector2 offHill, offRidge, offLake, offForest, offEmpty;
        Vector2 lakeCenter;
        float[,] vh;
        Palette[,] ground;
        bool[,] isWater;
        bool[,] reserved;
        float[,] forestDensity;
        Material runtimeMaterial;
        Mesh[] treeMeshes, rockMeshes, bushMeshes, grassMeshes, flowerMeshes;
        Transform worldRoot, propsRoot;

        // ================= Palette =================

        static Color[] DefaultPalette()
        {
            return new[]
            {
                new Color(0.42f, 0.65f, 0.32f),   //  0 Grass          — rich natural green
                new Color(0.28f, 0.42f, 0.22f),    //  1 ForestFloor    — dark earthy green
                new Color(0.52f, 0.50f, 0.48f),    //  2 Rock           — warm grey
                new Color(0.92f, 0.94f, 0.97f),    //  3 Snow           — slightly blue-white
                new Color(0.78f, 0.70f, 0.52f),    //  4 Sand           — warm golden
                new Color(0.22f, 0.35f, 0.38f),    //  5 LakeBed        — dark teal
                new Color(0.52f, 0.72f, 0.38f),    //  6 Clearing       — fresh light green
                new Color(0.38f, 0.26f, 0.16f),    //  7 Trunk          — warm brown
                new Color(0.22f, 0.48f, 0.20f),    //  8 Leaves         — deep forest green
                new Color(0.55f, 0.40f, 0.25f),    //  9 Wood           — dock/plank brown
                new Color(0.20f, 0.50f, 0.65f),    // 10 Water          — calm blue
                new Color(0.14f, 0.32f, 0.16f),    // 11 DarkLeaves     — shadowed foliage
                new Color(0.58f, 0.78f, 0.42f),    // 12 LightGrass     — sunlit meadow
                new Color(0.40f, 0.42f, 0.38f),    // 13 DarkRock       — mossy stone
                new Color(0.82f, 0.55f, 0.62f),    // 14 Wildflower     — soft pink blossoms
                new Color(0.35f, 0.60f, 0.28f)     // 15 BrightLeaves   — lighter canopy
            };
        }

        void OnValidate()
        {
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

            if (buildBoundary) BuildBoundary();
            if (playerRig != null) PlacePlayer();
            if (setupFog) SetupFog();
            else RenderSettings.fog = false;
        }

        void ClearWorld()
        {
            Transform old = transform.Find("GeneratedWorld");
            if (old == null) return;

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

        Vector2 RandomOffset()
        {
            return new Vector2((float)rng.NextDouble() * 1000f, (float)rng.NextDouble() * 1000f);
        }

        // ================= Boundary walls =================

        void BuildBoundary()
        {
            float wallHalf = half - boundaryMargin;
            float wallHeight = mountainHeight + 10f;
            float wallThick = 1f;

            string[] names = { "Wall_N", "Wall_S", "Wall_E", "Wall_W" };
            Vector3[] positions =
            {
                new Vector3(0f, wallHeight * 0.5f, wallHalf),
                new Vector3(0f, wallHeight * 0.5f, -wallHalf),
                new Vector3(wallHalf, wallHeight * 0.5f, 0f),
                new Vector3(-wallHalf, wallHeight * 0.5f, 0f)
            };
            Vector3[] sizes =
            {
                new Vector3(wallHalf * 2f, wallHeight, wallThick),
                new Vector3(wallHalf * 2f, wallHeight, wallThick),
                new Vector3(wallThick, wallHeight, wallHalf * 2f),
                new Vector3(wallThick, wallHeight, wallHalf * 2f)
            };

            for (int i = 0; i < 4; i++)
            {
                var wall = new GameObject(names[i]);
                wall.transform.SetParent(worldRoot, false);
                wall.transform.localPosition = positions[i];
                var col = wall.AddComponent<BoxCollider>();
                col.size = sizes[i];
                // No renderer — invisible walls
            }
        }

        void PlacePlayer()
        {
            float y = SampleHeight(0f, 0f) + 0.05f;
            playerRig.position = transform.TransformPoint(new Vector3(0f, y, 0f));
        }

        // ================= Fog / atmosphere =================

        void SetupFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;

            // Match the camera background to the fog so the horizon blends seamlessly.
            if (Camera.main != null)
                Camera.main.backgroundColor = fogColor;
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

        // Auto-create a translucent water material when the user hasn't assigned one.
        Material GetOrCreateWaterMaterial()
        {
            if (waterMaterial != null) return waterMaterial;

            Shader s = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (s == null) s = Shader.Find("Standard");

            var mat = new Material(s);
            mat.name = "AutoWater";

            // Set surface type to Transparent in URP Simple Lit.
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1f);  // 0 = Opaque, 1 = Transparent
                mat.SetFloat("_Blend", 0f);    // Alpha blend
            }
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // Enable alpha blending keywords for URP.
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);

            // Use the palette texture so the Water palette texel gives the base tint.
            mat.mainTexture = runtimeMaterial.mainTexture;

            Color waterTint = (paletteColors != null && paletteColors.Length > (int)Palette.Water)
                ? paletteColors[(int)Palette.Water]
                : new Color(0.20f, 0.50f, 0.65f);
            waterTint.a = 0.55f;   // Semi-transparent

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", waterTint);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.75f);
            if (mat.HasProperty("_SpecColor")) mat.SetColor("_SpecColor", new Color(0.6f, 0.6f, 0.7f, 1f));

            return mat;
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

            // Secondary smaller hills for more organic terrain.
            float microHills = Mathf.PerlinNoise(x * 0.12f + offHill.y, z * 0.12f + offHill.x) * 0.5f * flat;
            h += microHills;

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
                float k = 1f - Mathf.SmoothStep(0f, 1f, dl / r);
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

            var eligible = new bool[N, N];

            // Pass 1: water, clearing, rock/snow, grass (with LightGrass variation)
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
                        // Mix Clearing with LightGrass for a natural meadow look.
                        float n = Mathf.PerlinNoise(c.x * 0.3f + 100f, c.y * 0.3f + 100f);
                        ground[i, j] = n > 0.6f ? Palette.LightGrass : Palette.Clearing;
                        continue;
                    }

                    if (avg > snowLineHeight) ground[i, j] = Palette.Snow;
                    else if (avg > rockLineHeight)
                    {
                        // Mix Rock with DarkRock for variety on slopes.
                        float n = Mathf.PerlinNoise(c.x * 0.2f + 50f, c.y * 0.2f + 50f);
                        ground[i, j] = n > 0.55f ? Palette.DarkRock : Palette.Rock;
                    }
                    else
                    {
                        // Mix Grass with LightGrass for sunlit patches.
                        float n = Mathf.PerlinNoise(c.x * 0.15f + 200f, c.y * 0.15f + 200f);
                        ground[i, j] = n > 0.65f ? Palette.LightGrass : Palette.Grass;
                    }

                    eligible[i, j] = avg <= treeLineHeight;
                }
            }

            // Pass 2: shore (any land cell touching water) turns to sand and stays tree-free.
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

            // Pass 3: forest clusters and empty patches.
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
            var mb = new LowPolyMeshBuilder();
            // Plank surface
            mb.AddBox(new Vector3(0f, 0f, dockLength * 0.5f), new Vector3(dockWidth, 0.12f, dockLength), Palette.Wood);

            // Support posts
            float postX = dockWidth * 0.5f - 0.1f;
            for (float z = 0.1f; z <= dockLength + 0.01f; z += 1.5f)
            {
                mb.AddBox(new Vector3(-postX, -0.8f, z), new Vector3(0.14f, 1.6f, 0.14f), Palette.Trunk);
                mb.AddBox(new Vector3(postX, -0.8f, z), new Vector3(0.14f, 1.6f, 0.14f), Palette.Trunk);
            }

            // Railing posts at the end
            mb.AddBox(new Vector3(-postX, 0.4f, dockLength), new Vector3(0.08f, 0.7f, 0.08f), Palette.Wood);
            mb.AddBox(new Vector3(postX, 0.4f, dockLength), new Vector3(0.08f, 0.7f, 0.08f), Palette.Wood);

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
            if (minI == int.MaxValue) return;

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
            go.AddComponent<MeshRenderer>().sharedMaterial = GetOrCreateWaterMaterial();
        }

        // ================= 5. Props =================

        void PlaceProps()
        {
            propsRoot = new GameObject("Props").transform;
            propsRoot.SetParent(worldRoot, false);
            CreatePlaceholderMeshes();

            int trees = 0;
            int rocks = 0;
            int details = 0;

            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (reserved[i, j]) continue;
                    Vector2 c = CellCenter(i, j);

                    // --- Trees ---
                    float f = forestDensity[i, j];
                    if (f > 0f && trees < maxTrees)
                    {
                        int count = Mathf.FloorToInt(treesPerCell * f + (float)rng.NextDouble());
                        for (int k = 0; k < count && trees < maxTrees; k++)
                        {
                            SpawnProp(c, treePrefabs, treeMeshes, treeScaleRange, "Tree", 0.08f);
                            trees++;
                        }

                        // Bushes at forest edges (lower density zones)
                        if (f < 0.5f && details < maxGroundDetails && rng.NextDouble() < bushChance)
                        {
                            SpawnProp(c, null, bushMeshes, new Vector2(0.6f, 1.2f), "Bush", 0.12f);
                            details++;
                        }
                    }

                    // --- Rocks ---
                    bool rocky = ground[i, j] == Palette.Rock || ground[i, j] == Palette.Snow
                              || ground[i, j] == Palette.DarkRock;
                    if (rng.NextDouble() < rockChance * (rocky ? 4f : 1f))
                    {
                        SpawnProp(c, rockPrefabs, rockMeshes, rockScaleRange, "Rock", 0.25f);
                        rocks++;
                    }

                    // --- Grass tufts on meadows and clearing edges ---
                    if (details < maxGroundDetails)
                    {
                        bool isMeadow = ground[i, j] == Palette.Grass || ground[i, j] == Palette.LightGrass
                                      || ground[i, j] == Palette.Clearing;
                        if (isMeadow && rng.NextDouble() < grassTuftChance)
                        {
                            SpawnProp(c, null, grassMeshes, new Vector2(0.5f, 1.0f), "GrassTuft", 0.05f);
                            details++;
                        }

                        // Wildflowers — sparse splashes of color
                        if (isMeadow && rng.NextDouble() < wildflowerChance)
                        {
                            SpawnProp(c, null, flowerMeshes, new Vector2(0.4f, 0.8f), "Flower", 0.03f);
                            details++;
                        }
                    }
                }
            }

            if (staticBatchProps) StaticBatchingUtility.Combine(propsRoot.gameObject);

            Debug.Log($"WorldGenerator: seed {seed}, {N}x{N} cells ({N * cellSize} m), " +
                      $"{trees} trees, {rocks} rocks, {details} ground details, " +
                      $"{N * N * 2} terrain tris.");
        }

        void SpawnProp(Vector2 cellCenter, GameObject[] prefabs, Mesh[] meshes, Vector2 scaleRange, string propName, float sinkAmount)
        {
            float x = cellCenter.x + ((float)rng.NextDouble() - 0.5f) * cellSize * 0.9f;
            float z = cellCenter.y + ((float)rng.NextDouble() - 0.5f) * cellSize * 0.9f;
            float y = SampleHeight(x, z) - sinkAmount;
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
                treeMeshes = new[] { BuildPineMesh(), BuildBroadleafMesh(), BuildBirchMesh(), BuildTallPineMesh() };

            if (rockPrefabs == null || rockPrefabs.Length == 0 || rockPrefabs[0] == null)
                rockMeshes = new[] { BuildRockMesh(8, 0.7f, 0.5f), BuildRockMesh(10, 0.9f, 0.65f), BuildRockMesh(6, 0.5f, 0.4f) };

            bushMeshes = new[] { BuildBushMesh(), BuildBushMesh2() };
            grassMeshes = new[] { BuildGrassTuft() };
            flowerMeshes = new[] { BuildWildflowerMesh() };
        }

        // --- Pine tree: octagonal trunk, 4 tiered foliage layers (10 sides each) ---
        static Mesh BuildPineMesh()
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddCone(new Vector3(0f, 0f, 0f), 0.15f, 1.4f, 8, Palette.Trunk, true);

            mb.AddCone(new Vector3(0f, 0.8f, 0f),  1.6f, 1.8f, 10, Palette.DarkLeaves);
            mb.AddCone(new Vector3(0f, 1.8f, 0f),  1.3f, 1.8f, 10, Palette.Leaves);
            mb.AddCone(new Vector3(0f, 2.8f, 0f),  1.0f, 1.6f, 10, Palette.Leaves);
            mb.AddCone(new Vector3(0f, 3.6f, 0f),  0.65f, 1.4f, 10, Palette.BrightLeaves);
            return mb.ToMesh("PinePlaceholder");
        }

        // --- Tall pine variant: slimmer, taller silhouette ---
        static Mesh BuildTallPineMesh()
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddCone(new Vector3(0f, 0f, 0f), 0.12f, 2.0f, 8, Palette.Trunk, true);

            mb.AddCone(new Vector3(0f, 1.4f, 0f), 1.1f, 1.5f, 10, Palette.DarkLeaves);
            mb.AddCone(new Vector3(0f, 2.2f, 0f), 0.9f, 1.5f, 10, Palette.Leaves);
            mb.AddCone(new Vector3(0f, 3.0f, 0f), 0.7f, 1.4f, 10, Palette.Leaves);
            mb.AddCone(new Vector3(0f, 3.8f, 0f), 0.45f, 1.2f, 10, Palette.BrightLeaves);
            mb.AddCone(new Vector3(0f, 4.4f, 0f), 0.25f, 0.9f, 8, Palette.BrightLeaves);
            return mb.ToMesh("TallPinePlaceholder");
        }

        // --- Broadleaf tree: thick trunk, hemisphere canopy with color variation ---
        static Mesh BuildBroadleafMesh()
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddCone(new Vector3(0f, 0f, 0f), 0.20f, 0.5f, 8, Palette.Trunk, true);
            mb.AddBox(new Vector3(0f, 0.9f, 0f), new Vector3(0.28f, 1.4f, 0.28f), Palette.Trunk);

            // Main canopy — hemisphere dome
            mb.AddHemisphere(new Vector3(0f, 2.4f, 0f), 2.0f, 3, 8, Palette.Leaves);
            // Extra smaller blob on top for organic shape
            mb.AddHemisphere(new Vector3(0.3f, 3.2f, 0.2f), 1.0f, 2, 6, Palette.BrightLeaves);
            // Shadow blob underneath
            mb.AddCone(new Vector3(0f, 2.4f, 0f), 1.8f, -0.8f, 8, Palette.DarkLeaves, false);

            return mb.ToMesh("BroadleafPlaceholder");
        }

        // --- Birch tree: tall, slim trunk with smaller canopy ---
        static Mesh BuildBirchMesh()
        {
            var mb = new LowPolyMeshBuilder();
            // Slim white-ish trunk (using Snow palette for birch bark look)
            mb.AddCone(new Vector3(0f, 0f, 0f), 0.10f, 2.6f, 8, Palette.Snow, true);
            // Dark ring marks on the bark
            mb.AddBox(new Vector3(0f, 0.8f, 0f), new Vector3(0.14f, 0.08f, 0.14f), Palette.Trunk);
            mb.AddBox(new Vector3(0f, 1.5f, 0f), new Vector3(0.13f, 0.06f, 0.13f), Palette.Trunk);

            // Airy canopy — two offset hemispheres
            mb.AddHemisphere(new Vector3(-0.2f, 3.0f, 0f), 1.4f, 2, 8, Palette.BrightLeaves);
            mb.AddHemisphere(new Vector3(0.3f, 3.4f, 0.1f), 1.1f, 2, 6, Palette.Leaves);

            return mb.ToMesh("BirchPlaceholder");
        }

        // --- Rock: dome only (no bottom cone = no UFO look), sunk into ground ---
        static Mesh BuildRockMesh(int sides, float radius, float height)
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddHemisphere(Vector3.zero, radius, 2, sides, Palette.Rock);
            // A few faces in DarkRock for mossy variety
            mb.AddCone(new Vector3(0.1f, 0f, -0.05f), radius * 0.6f, height * 0.4f, sides / 2, Palette.DarkRock, false);

            return mb.ToMesh("RockPlaceholder");
        }

        // --- Bush: low dome of foliage ---
        static Mesh BuildBushMesh()
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddHemisphere(new Vector3(0f, 0.1f, 0f), 0.7f, 2, 6, Palette.DarkLeaves);
            mb.AddHemisphere(new Vector3(0.2f, 0.2f, 0.1f), 0.5f, 2, 6, Palette.Leaves);
            return mb.ToMesh("BushPlaceholder");
        }

        static Mesh BuildBushMesh2()
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddHemisphere(new Vector3(0f, 0.05f, 0f), 0.55f, 2, 6, Palette.Leaves);
            mb.AddHemisphere(new Vector3(-0.15f, 0.15f, 0.1f), 0.45f, 2, 6, Palette.BrightLeaves);
            return mb.ToMesh("BushPlaceholder2");
        }

        // --- Grass tuft: 3 small upright triangles (6 tris total — dirt cheap) ---
        static Mesh BuildGrassTuft()
        {
            var mb = new LowPolyMeshBuilder();
            float h = 0.35f;
            float w = 0.08f;

            for (int i = 0; i < 3; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                float dx = Mathf.Cos(angle) * 0.05f;
                float dz = Mathf.Sin(angle) * 0.05f;
                Vector3 baseL = new Vector3(dx - w, 0f, dz);
                Vector3 baseR = new Vector3(dx + w, 0f, dz);
                Vector3 tip = new Vector3(dx, h + i * 0.05f, dz);

                Palette c = (i == 0) ? Palette.LightGrass : Palette.Grass;
                mb.AddTriangle(baseL, baseR, tip, c, Vector3.forward);
                mb.AddTriangle(baseR, baseL, tip, c, Vector3.back);
            }
            return mb.ToMesh("GrassTuft");
        }

        // --- Wildflower: tiny stem + small colored blossom ---
        static Mesh BuildWildflowerMesh()
        {
            var mb = new LowPolyMeshBuilder();
            mb.AddBox(new Vector3(0f, 0.15f, 0f), new Vector3(0.03f, 0.3f, 0.03f), Palette.Leaves);
            mb.AddCone(new Vector3(0f, 0.28f, 0f), 0.12f, 0.1f, 5, Palette.Wildflower);
            mb.AddTriangle(
                new Vector3(-0.04f, 0.08f, 0f),
                new Vector3(0.04f, 0.08f, 0f),
                new Vector3(0f, 0.2f, 0.06f),
                Palette.Leaves, Vector3.forward);
            return mb.ToMesh("WildflowerPlaceholder");
        }
    }
}
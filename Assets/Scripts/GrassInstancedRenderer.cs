using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CalmWoods
{
    // Draws thousands of grass tufts with GPU instancing, without creating one GameObject per tuft.
    //
    // How it works:
    //   - The world is split into square chunks (16 m). Each chunk stores its tufts as matrices.
    //   - Every frame, for every camera, only the chunks near that camera are drawn.
    //   - Graphics.DrawMeshInstanced sends up to 1023 tufts per call.
    //
    // The tufts live in memory only (they are rebuilt by TerrainWorldGenerator.Generate), so after a
    // script recompile in the editor you must press "Generate World" again.
    [ExecuteAlways]
    public class GrassInstancedRenderer : MonoBehaviour
    {
        const int MaxPerBatch = 1023;   // Limit of one DrawMeshInstanced call

        class Chunk
        {
            public Vector3 center;
            public readonly List<Matrix4x4> matrices = new List<Matrix4x4>();
            public Matrix4x4[][] batches;
            public int[] batchCounts;
        }

        [System.NonSerialized] Mesh mesh;
        [System.NonSerialized] Material material;
        [System.NonSerialized] Dictionary<long, Chunk> building;
        [System.NonSerialized] Chunk[] chunks;
        [System.NonSerialized] float chunkSize = 16f;
        [System.NonSerialized] float drawDistance = 25f;

        public int ChunkCount => chunks != null ? chunks.Length : 0;

        // ---------- Building ----------

        public void Begin(Mesh grassMesh, Material grassMaterial, float chunkSizeMeters, float drawDistanceMeters)
        {
            mesh = grassMesh;
            material = grassMaterial;
            chunkSize = Mathf.Max(4f, chunkSizeMeters);
            drawDistance = drawDistanceMeters;
            building = new Dictionary<long, Chunk>();
            chunks = null;
        }

        public void Add(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            int cx = Mathf.FloorToInt(position.x / chunkSize);
            int cz = Mathf.FloorToInt(position.z / chunkSize);
            long key = ((long)cx << 32) ^ (uint)cz;

            if (!building.TryGetValue(key, out Chunk chunk))
            {
                chunk = new Chunk { center = new Vector3((cx + 0.5f) * chunkSize, 0f, (cz + 0.5f) * chunkSize) };
                building.Add(key, chunk);
            }
            chunk.matrices.Add(Matrix4x4.TRS(position, rotation, scale));
        }

        // Converts the lists into fixed arrays of at most 1023 matrices (what DrawMeshInstanced wants).
        public void End()
        {
            chunks = new Chunk[building.Count];
            int index = 0;
            foreach (Chunk chunk in building.Values)
            {
                int total = chunk.matrices.Count;
                int batchCount = Mathf.CeilToInt(total / (float)MaxPerBatch);
                chunk.batches = new Matrix4x4[batchCount][];
                chunk.batchCounts = new int[batchCount];
                for (int b = 0; b < batchCount; b++)
                {
                    int start = b * MaxPerBatch;
                    int count = Mathf.Min(MaxPerBatch, total - start);
                    chunk.batches[b] = new Matrix4x4[count];
                    chunk.matrices.CopyTo(start, chunk.batches[b], 0, count);
                    chunk.batchCounts[b] = count;
                }
                chunk.matrices.Clear();
                chunk.matrices.Capacity = 0;
                chunks[index++] = chunk;
            }
            building = null;
        }

        // ---------- Drawing ----------

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        }

        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        }

        // Called for every camera that is about to render (game camera, VR eyes, Scene view).
        void OnBeginCamera(ScriptableRenderContext context, Camera cam)
        {
            if (chunks == null || mesh == null || material == null) return;
            if (cam.cameraType == CameraType.Preview || cam.cameraType == CameraType.Reflection) return;

            Vector3 eye = cam.transform.position;
            float reach = drawDistance + chunkSize * 0.75f;   // Chunk center distance + half a chunk diagonal
            float reachSqr = reach * reach;

            for (int i = 0; i < chunks.Length; i++)
            {
                Chunk chunk = chunks[i];
                float dx = chunk.center.x - eye.x;
                float dz = chunk.center.z - eye.z;
                if (dx * dx + dz * dz > reachSqr) continue;

                for (int b = 0; b < chunk.batches.Length; b++)
                {
                    Graphics.DrawMeshInstanced(mesh, 0, material, chunk.batches[b], chunk.batchCounts[b],
                        null, ShadowCastingMode.Off, false, gameObject.layer, cam);
                }
            }
        }
    }
}

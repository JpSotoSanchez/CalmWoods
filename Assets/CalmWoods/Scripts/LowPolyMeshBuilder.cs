using System.Collections.Generic;
using UnityEngine;

namespace CalmWoods
{
    // Every color used by the generated world. Each entry is one texel of a tiny palette texture,
    // so terrain, trees, rocks, water and dock can all share ONE material (very cheap on Quest).
    //
    // The first 11 entries keep backward compatibility. Entries 11-15 add visual variety
    // (shaded foliage, sunlit grass, mossy rock, wildflowers, bright canopy highlights).
    public enum Palette
    {
        Grass,          // 0
        ForestFloor,    // 1
        Rock,           // 2
        Snow,           // 3
        Sand,           // 4
        LakeBed,        // 5
        Clearing,       // 6
        Trunk,          // 7
        Leaves,         // 8
        Wood,           // 9
        Water,          // 10
        DarkLeaves,     // 11  — shaded inner foliage / bushes
        LightGrass,     // 12  — sunlit meadow highlights
        DarkRock,       // 13  — mossy / shadowed stone
        Wildflower,     // 14  — small pops of color on the ground
        BrightLeaves    // 15  — lighter canopy highlights
    }

    public static class PaletteInfo
    {
        public static readonly int Count = System.Enum.GetValues(typeof(Palette)).Length;
    }

    // Builds flat-shaded low-poly meshes. Every triangle owns its vertices, so
    // RecalculateNormals gives hard, faceted lighting (the classic low-poly look).
    public class LowPolyMeshBuilder
    {
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<Vector2> uvs = new List<Vector2>();
        readonly List<int> triangles = new List<int>();

        // UV that points at the center of a palette texel.
        public static Vector2 PaletteUV(Palette color)
        {
            return new Vector2(((int)color + 0.5f) / PaletteInfo.Count, 0.5f);
        }

        // Adds one triangle. The winding is flipped if needed so the face looks toward 'outward'.
        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Palette color, Vector3 outward)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(normal, outward) < 0f)
            {
                (b, c) = (c, b);
            }

            int start = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);

            Vector2 uv = PaletteUV(color);
            uvs.Add(uv);
            uvs.Add(uv);
            uvs.Add(uv);

            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
        }

        // Adds a planar quad whose corners are given in perimeter order.
        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Palette color, Vector3 outward)
        {
            AddTriangle(a, b, c, color, outward);
            AddTriangle(a, c, d, color, outward);
        }

        public void AddBox(Vector3 center, Vector3 size, Palette color)
        {
            Vector3 h = size * 0.5f;
            Vector3 P(int x, int y, int z) => center + new Vector3(x * h.x, y * h.y, z * h.z);

            AddQuad(P(1, -1, -1), P(1, 1, -1), P(1, 1, 1), P(1, -1, 1), color, Vector3.right);
            AddQuad(P(-1, -1, -1), P(-1, -1, 1), P(-1, 1, 1), P(-1, 1, -1), color, Vector3.left);
            AddQuad(P(-1, 1, -1), P(-1, 1, 1), P(1, 1, 1), P(1, 1, -1), color, Vector3.up);
            AddQuad(P(-1, -1, -1), P(1, -1, -1), P(1, -1, 1), P(-1, -1, 1), color, Vector3.down);
            AddQuad(P(-1, -1, 1), P(1, -1, 1), P(1, 1, 1), P(-1, 1, 1), color, Vector3.forward);
            AddQuad(P(-1, -1, -1), P(-1, 1, -1), P(1, 1, -1), P(1, -1, -1), color, Vector3.back);
        }

        // Cone with 'sides' faces. A negative height points the apex downward (useful for rocks and
        // leafy canopies made of two cones). 'cap' adds the flat base.
        public void AddCone(Vector3 baseCenter, float radius, float height, int sides, Palette color, bool cap = true)
        {
            Vector3 apex = baseCenter + Vector3.up * height;
            for (int i = 0; i < sides; i++)
            {
                float a0 = (i / (float)sides) * Mathf.PI * 2f;
                float a1 = ((i + 1) / (float)sides) * Mathf.PI * 2f;
                Vector3 p0 = baseCenter + new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * radius;
                Vector3 p1 = baseCenter + new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * radius;

                Vector3 horizontalOut = (p0 + p1) * 0.5f - baseCenter;
                AddTriangle(p0, p1, apex, color, horizontalOut);

                if (cap)
                {
                    AddTriangle(p0, p1, baseCenter, color, height > 0f ? Vector3.down : Vector3.up);
                }
            }
        }

        // Hemisphere dome (half sphere). Used for bush canopies and rounded rock tops.
        // 'rings' latitude bands x 'segments' longitude slices. Cheap enough for Quest
        // at 2x6 (24 tris) or 3x8 (48 tris).
        // When flipDown is true the hemisphere faces downward (bottom half of a sphere),
        // so pairing one normal + one flipped at the same center gives a full sphere with
        // no seam at the equator — both halves share the same equator ring of vertices.
        public void AddHemisphere(Vector3 center, float radius, int rings, int segments, Palette color, bool flipDown = false)
        {
            float sign = flipDown ? -1f : 1f;

            for (int ring = 0; ring < rings; ring++)
            {
                float phi0 = (ring / (float)rings) * Mathf.PI * 0.5f;
                float phi1 = ((ring + 1) / (float)rings) * Mathf.PI * 0.5f;
                float y0 = Mathf.Sin(phi0) * radius * sign;
                float y1 = Mathf.Sin(phi1) * radius * sign;
                float r0 = Mathf.Cos(phi0) * radius;
                float r1 = Mathf.Cos(phi1) * radius;

                for (int seg = 0; seg < segments; seg++)
                {
                    float t0 = (seg / (float)segments) * Mathf.PI * 2f;
                    float t1 = ((seg + 1) / (float)segments) * Mathf.PI * 2f;

                    Vector3 a = center + new Vector3(Mathf.Cos(t0) * r0, y0, Mathf.Sin(t0) * r0);
                    Vector3 b = center + new Vector3(Mathf.Cos(t1) * r0, y0, Mathf.Sin(t1) * r0);
                    Vector3 c = center + new Vector3(Mathf.Cos(t1) * r1, y1, Mathf.Sin(t1) * r1);
                    Vector3 d = center + new Vector3(Mathf.Cos(t0) * r1, y1, Mathf.Sin(t0) * r1);

                    Vector3 outward = ((a + b + c + d) * 0.25f - center).normalized;
                    AddTriangle(a, b, c, color, outward);
                    AddTriangle(a, c, d, color, outward);
                }
            }
        }

        public Mesh ToMesh(string meshName)
        {
            var mesh = new Mesh { name = meshName };
            mesh.indexFormat = vertices.Count > 65535
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();   // Unshared vertices => flat shading
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace CalmWoods
{
    // Every color used by the generated world. Each entry is one texel of a tiny palette texture,
    // so terrain, trees, rocks, water and dock can all share ONE material (very cheap on Quest).
    public enum Palette
    {
        Grass, ForestFloor, Rock, Snow, Sand, LakeBed, Clearing, Trunk, Leaves, Wood, Water
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

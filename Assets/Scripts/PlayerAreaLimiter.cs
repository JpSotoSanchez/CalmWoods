using UnityEngine;

namespace CalmWoods
{
    // Keeps the player rig inside a square around the world center and out of the lake, whatever
    // locomotion is used (teleport, smooth movement, CharacterController or not).
    // Added and configured by TerrainWorldGenerator.
    public class PlayerAreaLimiter : MonoBehaviour
    {
        [SerializeField] Vector3 center;
        [SerializeField] float extent = 30f;   // Half of the square side, in meters

        // Water: the player may not stand where the ground is below waterY, except on the dock.
        Terrain terrain;
        float waterY;
        bool hasDock;
        Vector3 dockStart, dockDir;
        float dockLength, dockWidth;

        Vector3 lastValid;
        bool hasLastValid;
        CharacterController controller;

        public void Configure(Vector3 worldCenter, float halfExtent)
        {
            center = worldCenter;
            extent = halfExtent;
        }

        public void ConfigureWater(Terrain groundTerrain, float waterWorldY, bool dockExists,
            Vector3 dockStartWorld, Vector3 dockDirection, float length, float width)
        {
            terrain = groundTerrain;
            waterY = waterWorldY;
            hasDock = dockExists;
            dockStart = dockStartWorld;
            dockDir = new Vector3(dockDirection.x, 0f, dockDirection.z).normalized;
            dockLength = length;
            dockWidth = width;
            hasLastValid = false;
        }

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        void LateUpdate()
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, center.x - extent, center.x + extent);
            p.z = Mathf.Clamp(p.z, center.z - extent, center.z + extent);

            // In the water and not on the dock: go back to the last dry position.
            if (terrain != null && IsWater(p.x, p.z) && !OnDock(p.x, p.z) && hasLastValid)
            {
                p.x = lastValid.x;
                p.z = lastValid.z;
            }
            else
            {
                lastValid = p;
                hasLastValid = true;
            }

            if (p.x != transform.position.x || p.z != transform.position.z)
            {
                // A CharacterController overrides direct position changes, so pause it for a moment.
                if (controller != null) controller.enabled = false;
                transform.position = new Vector3(p.x, transform.position.y, p.z);
                if (controller != null) controller.enabled = true;
            }
        }

        bool IsWater(float x, float z)
        {
            float ground = terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y;
            return ground < waterY;
        }

        // Rectangle of the dock, seen from above.
        bool OnDock(float x, float z)
        {
            if (!hasDock) return false;
            Vector3 rel = new Vector3(x - dockStart.x, 0f, z - dockStart.z);
            float along = Vector3.Dot(rel, dockDir);
            float side = Vector3.Dot(rel, new Vector3(dockDir.z, 0f, -dockDir.x));
            return along >= -1f && along <= dockLength + 0.2f && Mathf.Abs(side) <= dockWidth * 0.5f + 0.2f;
        }
    }
}

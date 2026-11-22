using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Breathing-reactive weather. Clouds circle around the center of the world and react to the calm score:
///
///   Low calm (stressed)  -> many clouds, dark, big, low and close to the center, moving faster (wind).
///   High calm            -> clouds get lighter, smaller, higher and move away; at the calmest level
///                           only a couple of distant clouds remain.
///
/// It only reacts while Feedback mode is active (the trigger toggle). When the mode ends, the weather
/// returns to the cleanest state.
///
/// Clouds are created at runtime (a few squashed spheres each) unless a cloud prefab is assigned.
/// All clouds share one material, so the color change is a single material update per frame.
/// </summary>
public class WeatherController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TriggerModeSwitcher modeSwitcher;
    [SerializeField] private CalmScoreCalculator calmScore;

    [Tooltip("Center of the world (e.g. the XR Origin or the middle of the generated map). Defaults to this object.")]
    [SerializeField] private Transform center;

    [Tooltip("Optional on-screen label to tune the weather inside the headset.")]
    [SerializeField] private TMP_Text debugLabel;

    [Header("Score mapping")]
    [Tooltip("Calm score at (or above) which the sky is at its cleanest.")]
    [SerializeField, Range(0f, 1f)] private float clearSkyScore = 0.9f;

    [Tooltip("Calm score at (or below) which the storm is at its worst.")]
    [SerializeField, Range(0f, 1f)] private float stormScore = 0.1f;

    [Tooltip("Time constant (s) of the weather response. Larger = slower transitions.")]
    [SerializeField] private float responseSeconds = 2f;

    [Header("Clouds")]
    [Tooltip("Optional. If empty, simple clouds are built from squashed spheres. " +
             "Its materials are replaced by one shared material so the color can change.")]
    [SerializeField] private GameObject cloudPrefab;

    [Tooltip("Optional. A simple (e.g. URP Unlit) white material works best. Falls back to the default primitive material.")]
    [SerializeField] private Material cloudMaterial;

    [Tooltip("Total number of clouds created (visible at maximum stress).")]
    [SerializeField] private int maxClouds = 16;

    [Tooltip("Clouds that remain at the cleanest level. Use 0 for a completely clear sky.")]
    [SerializeField] private int minClouds = 2;

    [Tooltip("Spheres per procedural cloud. Keep low on Quest.")]
    [SerializeField] private int puffsPerCloud = 3;

    [Tooltip("Size of a cloud in meters (roughly its width / 2).")]
    [SerializeField] private float cloudScale = 12f;

    [Tooltip("Clouds grow by this factor at maximum stress.")]
    [SerializeField] private float stormSizeMultiplier = 1.6f;

    [SerializeField] private int seed = 12345;

    [Header("Position")]
    [Tooltip("Distance of the clouds from the center when calm.")]
    [SerializeField] private float farRadius = 120f;

    [Tooltip("Distance of the clouds from the center at maximum stress.")]
    [SerializeField] private float nearRadius = 25f;

    [SerializeField] private float calmHeight = 45f;
    [SerializeField] private float stormHeight = 18f;

    [Header("Wind")]
    [Tooltip("Degrees per second that the clouds circle the center when calm.")]
    [SerializeField] private float calmOrbitSpeed = 0.5f;

    [Tooltip("Degrees per second that the clouds circle the center at maximum stress.")]
    [SerializeField] private float stormOrbitSpeed = 4f;

    [Header("Color")]
    [SerializeField] private Color calmColor = Color.white;
    [SerializeField] private Color stormColor = new Color(0.15f, 0.17f, 0.22f, 1f);

    [Header("Testing")]
    [Tooltip("Ignore the real calm score and the mode toggle, and use the value below. Handy to tune the look without breathing.")]
    [SerializeField] private bool useDebugScore;
    [SerializeField, Range(0f, 1f)] private float debugScore = 0.5f;

    private class Cloud
    {
        public Transform transform;
        public float angle;         // Starting angle around the center, in degrees.
        public float radiusFactor;  // Per-cloud variation of the distance.
        public float heightOffset;
        public float sizeFactor;
    }

    private readonly List<Cloud> clouds = new List<Cloud>();
    private Transform cloudsRoot;
    private Material runtimeMaterial;
    private int colorPropertyId;

    private float visualStress;
    private float orbitAngle;

    // Values kept for the debug label.
    private float lastRadius;
    private float lastHeight;
    private float lastVisibleCount;
    private float debugTimer;

    private void Awake()
    {
        if (center == null)
        {
            center = transform;
        }

        BuildClouds();
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }

        if (cloudsRoot != null)
        {
            Destroy(cloudsRoot.gameObject);
        }
    }

    // ------------------------------------------------------------------
    // Cloud creation
    // ------------------------------------------------------------------

    private void BuildClouds()
    {
        // Own root without scale, so a scaled parent can't distort the clouds.
        cloudsRoot = new GameObject("Clouds").transform;

        System.Random rng = new System.Random(seed);
        List<Renderer> allRenderers = new List<Renderer>();

        const float goldenAngle = 137.508f; // Spreads clouds evenly around the circle for any cloud count.

        for (int i = 0; i < maxClouds; i++)
        {
            GameObject cloudObject = cloudPrefab != null
                ? Instantiate(cloudPrefab, cloudsRoot)
                : CreateProceduralCloud(rng);

            cloudObject.name = "Cloud_" + i;
            cloudObject.transform.SetParent(cloudsRoot, false);
            cloudObject.transform.rotation = Quaternion.Euler(0f, RandomRange(rng, 0f, 360f), 0f);

            allRenderers.AddRange(cloudObject.GetComponentsInChildren<Renderer>());

            clouds.Add(new Cloud
            {
                transform = cloudObject.transform,
                angle = i * goldenAngle + RandomRange(rng, -10f, 10f),
                radiusFactor = RandomRange(rng, 0.8f, 1.2f),
                heightOffset = RandomRange(rng, -4f, 4f),
                sizeFactor = RandomRange(rng, 0.7f, 1.3f)
            });
        }

        // One shared material for every cloud: the color is changed once per frame.
        Material source = cloudMaterial != null
            ? cloudMaterial
            : (allRenderers.Count > 0 ? allRenderers[0].sharedMaterial : null);

        if (source != null)
        {
            runtimeMaterial = new Material(source);
            colorPropertyId = Shader.PropertyToID(runtimeMaterial.HasProperty("_BaseColor") ? "_BaseColor" : "_Color");
        }

        foreach (Renderer cloudRenderer in allRenderers)
        {
            if (runtimeMaterial != null)
            {
                cloudRenderer.sharedMaterial = runtimeMaterial;
            }

            // Cloud shadows are expensive on mobile hardware.
            cloudRenderer.shadowCastingMode = ShadowCastingMode.Off;
            cloudRenderer.receiveShadows = false;
        }

        // Start in the cleanest state.
        ApplyWeather(0f, 0f);
    }

    private GameObject CreateProceduralCloud(System.Random rng)
    {
        GameObject root = new GameObject("Cloud");

        for (int p = 0; p < puffsPerCloud; p++)
        {
            GameObject puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            Collider puffCollider = puff.GetComponent<Collider>();
            if (puffCollider != null)
            {
                Destroy(puffCollider);
            }

            puff.transform.SetParent(root.transform, false);
            puff.transform.localPosition = new Vector3(
                RandomRange(rng, -0.5f, 0.5f),
                RandomRange(rng, -0.05f, 0.12f),
                RandomRange(rng, -0.3f, 0.3f));

            // Flattened spheres look like clouds.
            puff.transform.localScale = new Vector3(
                RandomRange(rng, 0.5f, 0.8f),
                RandomRange(rng, 0.25f, 0.4f),
                RandomRange(rng, 0.45f, 0.7f));
        }

        return root;
    }

    private static float RandomRange(System.Random rng, float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }

    // ------------------------------------------------------------------
    // Per-frame update
    // ------------------------------------------------------------------

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        bool feedbackActive = useDebugScore ||
                              (modeSwitcher != null && modeSwitcher.CurrentMode == AppMode.Feedback);

        // Outside Feedback mode the target stress is 0: the weather returns to the cleanest state.
        float targetStress = 0f;
        if (feedbackActive)
        {
            float score = useDebugScore ? debugScore : (calmScore != null ? calmScore.Score : 0.5f);

            // Reversed range: 0 at clearSkyScore (calm), 1 at stormScore (stressed).
            targetStress = Mathf.InverseLerp(clearSkyScore, stormScore, score);
        }

        float alpha = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, responseSeconds));
        visualStress = Mathf.Lerp(visualStress, targetStress, alpha);

        ApplyWeather(visualStress, dt);
        UpdateDebugLabel(dt);
    }

    /// <summary>Places, sizes and colors the clouds for a stress level between 0 (calm) and 1 (storm).</summary>
    private void ApplyWeather(float stress, float dt)
    {
        float radius = Mathf.Lerp(farRadius, nearRadius, stress);
        float height = Mathf.Lerp(calmHeight, stormHeight, stress);
        float orbitSpeed = Mathf.Lerp(calmOrbitSpeed, stormOrbitSpeed, stress);
        float sizeMultiplier = Mathf.Lerp(1f, stormSizeMultiplier, stress);

        int lowest = Mathf.Clamp(minClouds, 0, clouds.Count);
        float visibleCount = Mathf.Lerp(lowest, clouds.Count, stress);

        orbitAngle += orbitSpeed * dt;

        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetColor(colorPropertyId, Color.Lerp(calmColor, stormColor, stress));
        }

        Vector3 centerPosition = center.position;

        for (int i = 0; i < clouds.Count; i++)
        {
            Cloud cloud = clouds[i];

            // Cloud i appears when the visible count passes i; the one on the edge grows in smoothly.
            float presence = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(visibleCount - i));
            bool visible = presence > 0.01f;

            if (cloud.transform.gameObject.activeSelf != visible)
            {
                cloud.transform.gameObject.SetActive(visible);
            }

            if (!visible) continue;

            float angleRadians = (cloud.angle + orbitAngle) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angleRadians), 0f, Mathf.Sin(angleRadians)) * (radius * cloud.radiusFactor);

            cloud.transform.position = centerPosition + offset + Vector3.up * (height + cloud.heightOffset);
            cloud.transform.localScale = Vector3.one * (cloudScale * cloud.sizeFactor * sizeMultiplier * presence);
        }

        lastRadius = radius;
        lastHeight = height;
        lastVisibleCount = visibleCount;
    }

    private void UpdateDebugLabel(float dt)
    {
        if (debugLabel == null) return;

        debugTimer += dt;
        if (debugTimer < 0.25f) return;
        debugTimer = 0f;

        float score = useDebugScore ? debugScore : (calmScore != null ? calmScore.Score : 0f);

        debugLabel.text =
            $"Weather stress: {visualStress:F2} (score {score:F2})\n" +
            $"Clouds: {lastVisibleCount:F1}/{clouds.Count}\n" +
            $"Radius: {lastRadius:F0} m | Height: {lastHeight:F0} m";
    }

    // Shows the two extremes in the Scene view when this object is selected.
    private void OnDrawGizmosSelected()
    {
        Vector3 centerPosition = center != null ? center.position : transform.position;

        Gizmos.color = new Color(1f, 1f, 1f, 0.7f);
        DrawCircle(centerPosition + Vector3.up * calmHeight, farRadius);

        Gizmos.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);
        DrawCircle(centerPosition + Vector3.up * stormHeight, nearRadius);
    }

    private static void DrawCircle(Vector3 circleCenter, float circleRadius)
    {
        const int segments = 48;
        Vector3 previous = circleCenter + new Vector3(circleRadius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            Vector3 next = circleCenter + new Vector3(Mathf.Cos(angle) * circleRadius, 0f, Mathf.Sin(angle) * circleRadius);
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}

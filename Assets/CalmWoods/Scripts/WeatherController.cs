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
    [SerializeField] private float responseSeconds = 1.5f;

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

    [Header("Breathing motion (the sky breathes with the user)")]
    [Tooltip("Optional. Found automatically in the scene when empty.")]
    [SerializeField] private ControllerBreathingTracker breathTracker;

    [Tooltip("Meters the clouds rise on a full inhale (and sink on a full exhale).")]
    [SerializeField] private float breathLift = 8f;

    [Tooltip("Extra cloud size on a full inhale (0.15 = 15 % bigger).")]
    [SerializeField, Range(0f, 0.4f)] private float breathSwell = 0.15f;

    [Tooltip("Smoothing (s) of the breathing motion. Small = follows each breath closely.")]
    [SerializeField] private float breathSmoothing = 0.35f;

    [Tooltip("Clouds get this much brighter on a full inhale (0.15 = 15 %).")]
    [SerializeField, Range(0f, 0.5f)] private float breathBrighten = 0.15f;

    [Header("Environment (sun, ambient light, fog follow the stress)")]
    [SerializeField] private bool affectEnvironment = true;

    [Tooltip("Sun intensity multiplier at maximum stress.")]
    [SerializeField, Range(0f, 1f)] private float stormSunIntensity = 0.3f;

    [Tooltip("Sun color multiplier at maximum stress (cold and grey).")]
    [SerializeField] private Color stormSunTint = new Color(0.6f, 0.66f, 0.8f, 1f);

    [Tooltip("Ambient light multiplier at maximum stress.")]
    [SerializeField, Range(0f, 1f)] private float stormAmbient = 0.45f;

    [Tooltip("Fog color at maximum stress (stormy grey).")]
    [SerializeField] private Color stormFogColor = new Color(0.3f, 0.32f, 0.36f, 1f);

    [Tooltip("Fog distance multiplier at maximum stress (smaller = thicker fog).")]
    [SerializeField, Range(0.1f, 1f)] private float stormFogDistance = 0.35f;

    [Tooltip("Sun gets this much brighter on a full inhale (0.12 = 12 %).")]
    [SerializeField, Range(0f, 0.5f)] private float breathSunlight = 0.12f;

    [Header("Lightning (soft flashes in a strong storm)")]
    [SerializeField] private bool lightning = true;

    [Tooltip("Stress above which flashes can happen.")]
    [SerializeField, Range(0f, 1f)] private float lightningFromStress = 0.7f;

    [Tooltip("Seconds between flashes (random in this range; shorter as the storm grows).")]
    [SerializeField] private Vector2 lightningInterval = new Vector2(6f, 14f);

    [Tooltip("Extra sun intensity at the peak of a flash. Kept soft for comfort.")]
    [SerializeField] private float lightningIntensity = 1.5f;

    [Tooltip("How long a flash takes to fade.")]
    [SerializeField] private float lightningSeconds = 0.3f;

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
    private float breath;          // -1 = lungs empty, 1 = lungs full (smoothed)
    private float flash;           // 0..1, current lightning flash
    private float lightningTimer;

    // Environment values set by the time of day, restored when the weather is calm.
    private Light sun;
    private bool baselineValid;
    private bool environmentModified;
    private float baseSunIntensity;
    private Color baseSunColor;
    private Color baseAmbientSky, baseAmbientEquator, baseAmbientGround;
    private Color baseFogColor;
    private float baseFogStart, baseFogEnd;
    private CalmWoods.Environment.TimeOfDayManager timeOfDay;

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

        if (breathTracker == null)
        {
            breathTracker = FindAnyObjectByType<ControllerBreathingTracker>(FindObjectsInactive.Include);
        }

        BuildClouds();
    }

    private void Start()
    {
        // A new time of day changes the light: take it as the new calm baseline.
        timeOfDay = CalmWoods.Environment.TimeOfDayManager.Instance;
        if (timeOfDay != null) timeOfDay.Changed += OnTimeOfDayChanged;
    }

    private void OnTimeOfDayChanged(CalmWoods.Environment.TimeOfDay _)
    {
        CaptureBaseline();
    }

    private void OnDisable()
    {
        RestoreEnvironment();
    }

    private void OnDestroy()
    {
        if (timeOfDay != null) timeOfDay.Changed -= OnTimeOfDayChanged;

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

        // Breathing motion: chest position relative to its typical range, -1..1.
        float targetBreath = 0f;
        if (feedbackActive && breathTracker != null && breathTracker.HasSignal)
        {
            float range = Mathf.Max(breathTracker.SignalRms * 1.4f, 0.002f);
            targetBreath = Mathf.Clamp(breathTracker.Signal / range, -1f, 1f);
        }
        float breathAlpha = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, breathSmoothing));
        breath = Mathf.Lerp(breath, targetBreath, breathAlpha);

        UpdateLightning(feedbackActive, dt);
        ApplyWeather(visualStress, dt);
        UpdateEnvironment();
        UpdateDebugLabel(dt);
    }

    /// <summary>Places, sizes and colors the clouds for a stress level between 0 (calm) and 1 (storm).</summary>
    private void ApplyWeather(float stress, float dt)
    {
        float radius = Mathf.Lerp(farRadius, nearRadius, stress);
        // The sky rises and swells with each inhale, and settles with each exhale.
        float height = Mathf.Lerp(calmHeight, stormHeight, stress) + breath * breathLift;
        float orbitSpeed = Mathf.Lerp(calmOrbitSpeed, stormOrbitSpeed, stress);
        float sizeMultiplier = Mathf.Lerp(1f, stormSizeMultiplier, stress) * (1f + breath * breathSwell);

        int lowest = Mathf.Clamp(minClouds, 0, clouds.Count);
        float visibleCount = Mathf.Lerp(lowest, clouds.Count, stress);

        orbitAngle += orbitSpeed * dt;

        if (runtimeMaterial != null)
        {
            // Storm darkens the clouds; each inhale brightens them a little; lightning lights them up.
            Color color = Color.Lerp(calmColor, stormColor, stress) * (1f + breath * breathBrighten);
            color = Color.Lerp(color, Color.white, flash * 0.7f);
            color.a = 1f;
            runtimeMaterial.SetColor(colorPropertyId, color);
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

    // ------------------------------------------------------------------
    // Lightning and environment
    // ------------------------------------------------------------------

    private void UpdateLightning(bool feedbackActive, float dt)
    {
        flash = Mathf.Max(0f, flash - dt / Mathf.Max(0.01f, lightningSeconds));

        if (!lightning || !feedbackActive || visualStress < lightningFromStress)
        {
            lightningTimer = Random.Range(lightningInterval.x, lightningInterval.y) * 0.5f;
            return;
        }

        lightningTimer -= dt;
        if (lightningTimer > 0f) return;

        flash = 1f;
        // Stronger storm = flashes a bit more often.
        float severity = Mathf.InverseLerp(lightningFromStress, 1f, visualStress);
        lightningTimer = Random.Range(lightningInterval.x, lightningInterval.y) * Mathf.Lerp(1.3f, 0.7f, severity);
    }

    private void UpdateEnvironment()
    {
        if (!affectEnvironment) return;

        bool atRest = visualStress < 0.003f && Mathf.Abs(breath) < 0.003f && flash <= 0f;
        if (atRest)
        {
            // Put the exact time-of-day values back, then keep following them while calm.
            if (environmentModified) RestoreEnvironment();
            else CaptureBaseline();
            return;
        }

        if (!baselineValid) CaptureBaseline();
        environmentModified = true;
        float s = visualStress;

        if (sun != null)
        {
            float intensity = baseSunIntensity * Mathf.Lerp(1f, stormSunIntensity, s) * (1f + breath * breathSunlight);
            sun.intensity = intensity + flash * lightningIntensity;
            Color sunColor = Color.Lerp(baseSunColor, baseSunColor * stormSunTint, s);
            sun.color = Color.Lerp(sunColor, Color.white, flash);
        }

        float ambient = Mathf.Lerp(1f, stormAmbient, s);
        Color flashAmbient = Color.white * (flash * 0.5f);
        RenderSettings.ambientSkyColor = baseAmbientSky * ambient + flashAmbient;
        RenderSettings.ambientEquatorColor = baseAmbientEquator * ambient + flashAmbient;
        RenderSettings.ambientGroundColor = baseAmbientGround * ambient + flashAmbient * 0.5f;

        RenderSettings.fogColor = Color.Lerp(baseFogColor, stormFogColor, s * 0.85f);
        float fogScale = Mathf.Lerp(1f, stormFogDistance, s);
        RenderSettings.fogStartDistance = baseFogStart * fogScale;
        RenderSettings.fogEndDistance = Mathf.Max(baseFogEnd * fogScale, RenderSettings.fogStartDistance + 1f);
    }

    private void CaptureBaseline()
    {
        sun = RenderSettings.sun;
        if (sun != null)
        {
            baseSunIntensity = sun.intensity;
            baseSunColor = sun.color;
        }
        baseAmbientSky = RenderSettings.ambientSkyColor;
        baseAmbientEquator = RenderSettings.ambientEquatorColor;
        baseAmbientGround = RenderSettings.ambientGroundColor;
        baseFogColor = RenderSettings.fogColor;
        baseFogStart = RenderSettings.fogStartDistance;
        baseFogEnd = RenderSettings.fogEndDistance;
        baselineValid = true;
        environmentModified = false;
    }

    private void RestoreEnvironment()
    {
        if (!baselineValid || !environmentModified) return;

        if (sun != null)
        {
            sun.intensity = baseSunIntensity;
            sun.color = baseSunColor;
        }
        RenderSettings.ambientSkyColor = baseAmbientSky;
        RenderSettings.ambientEquatorColor = baseAmbientEquator;
        RenderSettings.ambientGroundColor = baseAmbientGround;
        RenderSettings.fogColor = baseFogColor;
        RenderSettings.fogStartDistance = baseFogStart;
        RenderSettings.fogEndDistance = baseFogEnd;
        environmentModified = false;
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

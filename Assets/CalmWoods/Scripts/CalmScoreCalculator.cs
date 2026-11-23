using CalmWoods.Breathing;
using TMPro;
using UnityEngine;

/// <summary>
/// Turns the breathing statistics into a single calm score between 0 and 1.
///   0 = agitated: fast, irregular breathing.
///   1 = calm: slow, regular breathing (close to the 4-2-4-2 guide, about 5 breaths per minute).
///
/// Score = weighted mix of
///   - rate score:       how slow the breathing is (breaths per minute),
///   - regularity score: how even the intervals between breaths are.
///
/// The score is smoothed over time so the environment (wind, clouds, water) changes gradually
/// instead of reacting to every single breath. Other systems should just read "Score".
/// </summary>
public class CalmScoreCalculator : MonoBehaviour
{
    public enum SourceMode
    {
        ControllerFirst,   // Use the controller; fall back to the microphone.
        MicrophoneFirst,   // Use the microphone; fall back to the controller.
        Average            // Average both when both have an estimate.
    }

    public enum ActiveSource
    {
        None,
        Controller,
        Microphone,
        Both
    }

    [Header("References")]
    [SerializeField] private TriggerModeSwitcher modeSwitcher;
    [SerializeField] private ControllerBreathingTracker controllerTracker;
    [SerializeField] private MicrophoneBreathingTracker microphoneTracker;

    [Tooltip("Optional on-screen label to watch how the score is built.")]
    [SerializeField] private TMP_Text debugLabel;

    [Header("Source")]
    [SerializeField] private SourceMode sourceMode = SourceMode.ControllerFirst;

    [Header("Rate (breaths per minute)")]
    [Tooltip("At or below this rate the rate score is 1. The 4-2-4-2 guide is about 5/min.")]
    [SerializeField] private float calmRate = 6f;

    [Tooltip("At or above this rate the rate score is 0.")]
    [SerializeField] private float agitatedRate = 20f;

    [Header("Regularity (0..1)")]
    [Tooltip("At or below this regularity the regularity score is 0.")]
    [SerializeField] private float irregularValue = 0.5f;

    [Tooltip("At or above this regularity the regularity score is 1.")]
    [SerializeField] private float regularValue = 0.9f;

    [Header("Weights")]
    [SerializeField, Range(0f, 1f)] private float rateWeight = 0.7f;
    [SerializeField, Range(0f, 1f)] private float regularityWeight = 0.3f;

    [Header("Following the guide (controller vs. breathing wheel)")]
    [Tooltip("Compares, every frame, whether the chest is moving the way the wheel says. " +
             "Reacts in seconds instead of waiting for whole breaths.")]
    [SerializeField] private bool useGuideSync = true;

    [Tooltip("Optional. Found automatically in the scene when empty.")]
    [SerializeField] private BreathingGuide guide;

    [Tooltip("How much of the score comes from following the wheel (the rest is rate + regularity).")]
    [SerializeField, Range(0f, 1f)] private float syncWeight = 0.5f;

    [Tooltip("Memory (s) of the sync score. Smaller = reacts faster, more jumpy.")]
    [SerializeField] private float syncWindowSeconds = 4f;

    [Tooltip("Seconds ignored at the start of each inhale/exhale (people react a bit late).")]
    [SerializeField] private float phaseGraceSeconds = 0.6f;

    [Tooltip("Chest speed (m/s) below which the user counts as 'not moving'. 0.002 = 2 mm/s.")]
    [SerializeField] private float minChestSpeed = 0.002f;

    [Header("Smoothing")]
    [Tooltip("Score used until the first breathing estimate is available.")]
    [SerializeField, Range(0f, 1f)] private float startScore = 0.5f;

    [Tooltip("Time constant (s) of the smoothing. Larger = slower, calmer transitions.")]
    [SerializeField] private float smoothingSeconds = 3f;

    [Tooltip("Seconds after the first data during which the score reacts much faster " +
             "(so the sky answers right away), easing into the normal smoothing.")]
    [SerializeField] private float quickStartSeconds = 8f;

    [Tooltip("Smoothing (s) at the very start of the quick-start period.")]
    [SerializeField] private float quickStartSmoothing = 0.4f;

    // ---- Public state ----

    /// <summary>Smoothed calm score, 0 (agitated) to 1 (calm). Read this from other systems.</summary>
    public float Score { get; private set; }

    /// <summary>Unsmoothed score computed from the latest breathing estimate.</summary>
    public float TargetScore { get; private set; }

    /// <summary>False until the first breathing estimate arrives; Score is startScore until then.</summary>
    public bool HasScore { get; private set; }

    public float RateScore { get; private set; }
    public float RegularityScore { get; private set; }
    public float CurrentBreathsPerMinute { get; private set; }
    public float CurrentRegularity { get; private set; }
    public ActiveSource Source { get; private set; } = ActiveSource.None;

    /// <summary>0..1, how well the chest follows the wheel's inhale/exhale lately.</summary>
    public float SyncScore { get; private set; }
    public bool HasSync { get; private set; }

    private float debugTimer;
    private float rateRegularityScore;
    private bool hasRateEstimate;
    private float dataSeconds;   // Time since the first usable data of this session

    private void Awake()
    {
        if (guide == null) guide = FindAnyObjectByType<BreathingGuide>(FindObjectsInactive.Include);
        ResetScore();
    }

    private void OnEnable()
    {
        if (modeSwitcher != null)
        {
            modeSwitcher.OnModeChanged += HandleModeChanged;
        }
    }

    private void OnDisable()
    {
        if (modeSwitcher != null)
        {
            modeSwitcher.OnModeChanged -= HandleModeChanged;
        }
    }

    private void HandleModeChanged(AppMode mode)
    {
        if (mode == AppMode.Feedback)
        {
            ResetScore();
        }
    }

    private void ResetScore()
    {
        Score = startScore;
        TargetScore = startScore;
        HasScore = false;
        RateScore = 0f;
        RegularityScore = 0f;
        CurrentBreathsPerMinute = 0f;
        CurrentRegularity = 0f;
        Source = ActiveSource.None;
        SyncScore = startScore;
        HasSync = false;
        rateRegularityScore = startScore;
        hasRateEstimate = false;
        dataSeconds = 0f;
    }

    // 0 at the first data, 1 once the quick-start period is over.
    private float QuickStartProgress => quickStartSeconds > 0f ? Mathf.Clamp01(dataSeconds / quickStartSeconds) : 1f;

    private void Update()
    {
        if (modeSwitcher == null || modeSwitcher.CurrentMode != AppMode.Feedback) return;

        float dt = Time.unscaledDeltaTime;

        if (TryGetEstimate(out float breathsPerMinute, out float regularity, out ActiveSource source))
        {
            CurrentBreathsPerMinute = breathsPerMinute;
            CurrentRegularity = regularity;
            Source = source;

            // Reversed ranges: InverseLerp(a, b, v) is 0 at a and 1 at b.
            RateScore = Mathf.InverseLerp(agitatedRate, calmRate, breathsPerMinute);
            RegularityScore = Mathf.InverseLerp(irregularValue, regularValue, regularity);

            float totalWeight = rateWeight + regularityWeight;
            rateRegularityScore = totalWeight > 0f
                ? (RateScore * rateWeight + RegularityScore * regularityWeight) / totalWeight
                : RateScore;
            hasRateEstimate = true;
        }

        UpdateSync(dt);

        // Combine: following the wheel reacts in seconds; rate + regularity confirm it over breaths.
        if (HasSync && hasRateEstimate)
        {
            TargetScore = Mathf.Lerp(rateRegularityScore, SyncScore, syncWeight);
            HasScore = true;
        }
        else if (HasSync)
        {
            TargetScore = SyncScore;   // Available after a few seconds, before any full breath.
            HasScore = true;
        }
        else if (hasRateEstimate)
        {
            TargetScore = rateRegularityScore;
            HasScore = true;
        }
        // If nothing is available yet, the last target is kept (the score holds its value).

        // Exponential smoothing, independent of the frame rate.
        // Very fast right after the first data, then eases into the normal smoothing.
        if (HasScore) dataSeconds += dt;
        float smoothing = Mathf.Lerp(quickStartSmoothing, smoothingSeconds, QuickStartProgress);
        float alpha = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, smoothing));
        Score = Mathf.Lerp(Score, TargetScore, alpha);

        UpdateDebugLabel(dt);
    }

    // Compares the chest movement with the wheel's current phase.
    //   Inhale on the wheel + chest rising  -> match (1)
    //   Exhale on the wheel + chest falling -> match (1)
    //   Opposite direction or not moving    -> no match (0)
    // Hold phases are skipped (the chest barely moves, so the direction is just noise).
    private void UpdateSync(float dt)
    {
        if (!useGuideSync || guide == null || controllerTracker == null) return;
        if (!guide.IsPlaying || guide.IsWaitingToStart || !controllerTracker.HasSignal) return;

        BreathPhase phase = guide.CurrentPhase;
        if (phase != BreathPhase.Inhale && phase != BreathPhase.Exhale) return;

        float secondsIntoPhase = guide.PhaseProgress * guide.Pattern.Duration(phase);
        if (secondsIntoPhase < phaseGraceSeconds) return;

        float speed = controllerTracker.Slope;
        bool shouldRise = phase == BreathPhase.Inhale;
        bool moving = Mathf.Abs(speed) >= minChestSpeed;
        float match = moving && (speed > 0f) == shouldRise ? 1f : 0f;

        // Short memory at the start (first answer in ~1-2 s), longer once it has data.
        float window = Mathf.Lerp(1f, syncWindowSeconds, QuickStartProgress);
        float alpha = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, window));
        SyncScore = Mathf.Lerp(SyncScore, match, alpha);
        HasSync = true;
    }

    private bool TryGetEstimate(out float breathsPerMinute, out float regularity, out ActiveSource source)
    {
        bool hasController = controllerTracker != null && controllerTracker.HasEstimate;
        bool hasMicrophone = microphoneTracker != null && microphoneTracker.HasEstimate;

        breathsPerMinute = 0f;
        regularity = 0f;
        source = ActiveSource.None;

        if (sourceMode == SourceMode.Average && hasController && hasMicrophone)
        {
            breathsPerMinute = (controllerTracker.BreathsPerMinute + microphoneTracker.BreathsPerMinute) * 0.5f;
            regularity = (controllerTracker.Regularity + microphoneTracker.Regularity) * 0.5f;
            source = ActiveSource.Both;
            return true;
        }

        bool preferController = sourceMode != SourceMode.MicrophoneFirst;

        if (preferController)
        {
            if (hasController) return UseController(out breathsPerMinute, out regularity, out source);
            if (hasMicrophone) return UseMicrophone(out breathsPerMinute, out regularity, out source);
        }
        else
        {
            if (hasMicrophone) return UseMicrophone(out breathsPerMinute, out regularity, out source);
            if (hasController) return UseController(out breathsPerMinute, out regularity, out source);
        }

        return false;
    }

    private bool UseController(out float breathsPerMinute, out float regularity, out ActiveSource source)
    {
        breathsPerMinute = controllerTracker.BreathsPerMinute;
        regularity = controllerTracker.Regularity;
        source = ActiveSource.Controller;
        return true;
    }

    private bool UseMicrophone(out float breathsPerMinute, out float regularity, out ActiveSource source)
    {
        breathsPerMinute = microphoneTracker.BreathsPerMinute;
        regularity = microphoneTracker.Regularity;
        source = ActiveSource.Microphone;
        return true;
    }

    private void UpdateDebugLabel(float dt)
    {
        if (debugLabel == null) return;

        debugTimer += dt;
        if (debugTimer < 0.25f) return;
        debugTimer = 0f;

        if (!HasScore)
        {
            debugLabel.text = $"Calm score: {Score:F2}\nWaiting for breaths...";
            return;
        }

        debugLabel.text =
            $"Calm score: {Score:F2} (target {TargetScore:F2})\n" +
            $"Following wheel: {(HasSync ? SyncScore.ToString("F2") : "--")}\n" +
            $"Source: {Source}\n" +
            $"Rate: {CurrentBreathsPerMinute:F1}/min -> {RateScore:F2}\n" +
            $"Regularity: {CurrentRegularity:F2} -> {RegularityScore:F2}";
    }
}

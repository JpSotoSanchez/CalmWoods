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

    [Header("Smoothing")]
    [Tooltip("Score used until the first breathing estimate is available.")]
    [SerializeField, Range(0f, 1f)] private float startScore = 0.5f;

    [Tooltip("Time constant (s) of the smoothing. Larger = slower, calmer transitions.")]
    [SerializeField] private float smoothingSeconds = 6f;

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

    private float debugTimer;

    private void Awake()
    {
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
    }

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
            TargetScore = totalWeight > 0f
                ? (RateScore * rateWeight + RegularityScore * regularityWeight) / totalWeight
                : RateScore;

            HasScore = true;
        }
        // If no source has an estimate, the last target is kept (the score holds its value).

        // Exponential smoothing, independent of the frame rate.
        float alpha = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, smoothingSeconds));
        Score = Mathf.Lerp(Score, TargetScore, alpha);

        UpdateDebugLabel(dt);
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
            $"Source: {Source}\n" +
            $"Rate: {CurrentBreathsPerMinute:F1}/min -> {RateScore:F2}\n" +
            $"Regularity: {CurrentRegularity:F2} -> {RegularityScore:F2}";
    }
}

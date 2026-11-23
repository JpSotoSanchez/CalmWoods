using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// Tracks the user's breathing through the headset microphone while Feedback mode is active.
///
/// Pipeline:
///   microphone -> per-frame signal level (std dev, so DC offset is ignored)
///   -> smoothed envelope -> adaptive thresholds (percentiles of the last N seconds)
///   -> breath detection with hysteresis -> breaths per minute + regularity.
///
/// Starts automatically when TriggerModeSwitcher enters Feedback mode and stops when it leaves it.
/// Other systems (weather, calm score) can read the public properties or subscribe to OnBreathDetected.
/// </summary>
public class MicrophoneBreathingTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TriggerModeSwitcher modeSwitcher;

    [Tooltip("Optional on-screen label to tune the detection inside the headset.")]
    [SerializeField] private TMP_Text debugLabel;

    [Header("Microphone")]
    [SerializeField] private int sampleRate = 16000;
    [SerializeField] private float permissionTimeoutSeconds = 30f;

    [Header("Envelope")]
    [Tooltip("Smoothing time in seconds. Larger = smoother but slower to react.")]
    [SerializeField] private float envelopeSmoothing = 0.25f;

    [Tooltip("How many envelope samples per second are stored for the adaptive thresholds.")]
    [SerializeField] private float envelopeRate = 20f;

    [Tooltip("Seconds of history used to estimate the noise floor and the breath peaks.")]
    [SerializeField] private float analysisWindowSeconds = 20f;

    [Header("Breath detection")]
    [Tooltip("Where the detection threshold sits between the noise floor (0) and the breath peaks (1).")]
    [SerializeField, Range(0.2f, 0.8f)] private float thresholdFraction = 0.5f;

    [Tooltip("Minimum difference between peak and floor to consider the signal usable.")]
    [SerializeField] private float minSignalRange = 0.002f;

    [Tooltip("Minimum seconds between two detected bursts.")]
    [SerializeField] private float minBurstInterval = 1.5f;

    [Tooltip("1 if only one sound burst per breath is picked up, 2 if inhale and exhale are both audible.")]
    [SerializeField, Range(1, 2)] private int burstsPerBreath = 1;

    [Tooltip("How many recent intervals are averaged to estimate the rate.")]
    [SerializeField] private int intervalsToAverage = 2;

    [Tooltip("If no breath is detected for this long, the estimate is discarded.")]
    [SerializeField] private float staleSeconds = 20f;

    [SerializeField] private bool logDebug;

    // ---- Public state ----
    public bool IsTracking { get; private set; }

    /// <summary>Smoothed microphone level (raw units).</summary>
    public float Level { get; private set; }

    /// <summary>Level mapped to 0..1 between the noise floor and the breath peaks.</summary>
    public float NormalizedLevel { get; private set; }

    public float BreathsPerMinute { get; private set; }

    /// <summary>0..1. Higher means more regular intervals between breaths.</summary>
    public float Regularity { get; private set; }

    /// <summary>True once enough breaths have been detected to give a rate.</summary>
    public bool HasEstimate { get; private set; }

    public event Action OnBreathDetected;

    // ---- Microphone ----
    private AudioClip micClip;
    private float[] clipBuffer;
    private int lastMicPosition;
    private Coroutine startRoutine;

    // ---- Envelope history (ring buffer) ----
    private float[] history;
    private float[] sortScratch;
    private int historyIndex;
    private int historyCount;
    private float envelopeTimer;
    private float analysisTimer;
    private float floorLevel;
    private float peakLevel;
    private bool thresholdsValid;

    // ---- Breath detection ----
    private bool aboveThreshold;
    private float lastBreathTime = float.NegativeInfinity;
    private readonly List<float> breathTimes = new List<float>();

    private float debugTimer;

    private void Awake()
    {
        int capacity = Mathf.Max(10, Mathf.RoundToInt(analysisWindowSeconds * envelopeRate));
        history = new float[capacity];
        sortScratch = new float[capacity];
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

        StopTracking();
    }

    private void HandleModeChanged(AppMode mode)
    {
        if (mode == AppMode.Feedback)
        {
            StartTracking();
        }
        else
        {
            StopTracking();
        }
    }

    // ------------------------------------------------------------------
    // Start / stop
    // ------------------------------------------------------------------

    public void StartTracking()
    {
        if (IsTracking || startRoutine != null) return;
        startRoutine = StartCoroutine(StartRoutine());
    }

    public void StopTracking()
    {
        if (startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }

        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        micClip = null;
        IsTracking = false;
        ResetState();

        if (debugLabel != null)
        {
            debugLabel.text = "Mic: off";
        }
    }

    private IEnumerator StartRoutine()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // On Quest the microphone needs a runtime permission.
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);

            float waited = 0f;
            while (!Permission.HasUserAuthorizedPermission(Permission.Microphone) && waited < permissionTimeoutSeconds)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.LogWarning("[MicrophoneBreathingTracker] Microphone permission was not granted.");
                startRoutine = null;
                yield break;
            }
        }
#endif

        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[MicrophoneBreathingTracker] No microphone found.");
            startRoutine = null;
            yield break;
        }

        // 1-second looping buffer; we read the new samples every frame.
        micClip = Microphone.Start(null, true, 1, sampleRate);

        // The microphone can take a moment to start delivering samples.
        float startTimeout = 2f;
        while (Microphone.GetPosition(null) <= 0 && startTimeout > 0f)
        {
            startTimeout -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (Microphone.GetPosition(null) <= 0)
        {
            Debug.LogWarning("[MicrophoneBreathingTracker] Microphone did not start.");
            Microphone.End(null);
            micClip = null;
            startRoutine = null;
            yield break;
        }

        clipBuffer = new float[micClip.samples];
        lastMicPosition = Microphone.GetPosition(null);

        ResetState();
        IsTracking = true;
        startRoutine = null;

        if (logDebug)
        {
            Debug.Log("[MicrophoneBreathingTracker] Tracking started.");
        }
    }

    private void ResetState()
    {
        Level = 0f;
        NormalizedLevel = 0f;
        BreathsPerMinute = 0f;
        Regularity = 0f;
        HasEstimate = false;

        historyIndex = 0;
        historyCount = 0;
        envelopeTimer = 0f;
        analysisTimer = 0f;
        floorLevel = 0f;
        peakLevel = 0f;
        thresholdsValid = false;

        aboveThreshold = false;
        lastBreathTime = float.NegativeInfinity;
        breathTimes.Clear();
    }

    // ------------------------------------------------------------------
    // Per-frame processing
    // ------------------------------------------------------------------

    private void Update()
    {
        if (!IsTracking) return;

        float dt = Time.unscaledDeltaTime;

        if (TryReadLevel(out float frameLevel))
        {
            // Exponential smoothing, independent of the frame rate.
            float alpha = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, envelopeSmoothing));
            Level = Mathf.Lerp(Level, frameLevel, alpha);
        }

        // Store the envelope at a fixed rate for the adaptive thresholds.
        envelopeTimer += dt;
        float envelopeStep = 1f / envelopeRate;
        if (envelopeTimer >= envelopeStep)
        {
            envelopeTimer -= envelopeStep;
            PushHistory(Level);
        }

        analysisTimer += dt;
        if (analysisTimer >= 0.5f)
        {
            analysisTimer = 0f;
            UpdateThresholds();
        }

        float range = peakLevel - floorLevel;
        NormalizedLevel = thresholdsValid ? Mathf.Clamp01((Level - floorLevel) / range) : 0f;

        DetectBreath();

        if (HasEstimate && Time.unscaledTime - lastBreathTime > staleSeconds)
        {
            HasEstimate = false;
            BreathsPerMinute = 0f;
            Regularity = 0f;
            breathTimes.Clear();
        }

        UpdateDebugLabel(dt);
    }

    /// <summary>
    /// Reads the samples recorded since the last call and returns their standard deviation
    /// (a DC-offset-free measure of loudness).
    /// </summary>
    private bool TryReadLevel(out float level)
    {
        level = 0f;

        int position = Microphone.GetPosition(null);
        if (position < 0 || position == lastMicPosition) return false;

        micClip.GetData(clipBuffer, 0);

        int length = clipBuffer.Length;
        int count = position - lastMicPosition;
        if (count < 0)
        {
            count += length; // The ring buffer wrapped around.
        }

        double sum = 0.0;
        double sumSquares = 0.0;
        int index = lastMicPosition;

        for (int i = 0; i < count; i++)
        {
            float sample = clipBuffer[index];
            sum += sample;
            sumSquares += sample * sample;

            index++;
            if (index >= length) index = 0;
        }

        lastMicPosition = position;

        double mean = sum / count;
        double variance = Math.Max(0.0, sumSquares / count - mean * mean);
        level = (float)Math.Sqrt(variance);
        return true;
    }

    private void PushHistory(float value)
    {
        history[historyIndex] = value;
        historyIndex = (historyIndex + 1) % history.Length;
        historyCount = Mathf.Min(historyCount + 1, history.Length);
    }

    /// <summary>
    /// Noise floor = 20th percentile, breath peak = 95th percentile of the recent envelope.
    /// This adapts to different microphones, distances and breath volumes without calibration.
    /// </summary>
    private void UpdateThresholds()
    {
        int minSamples = Mathf.RoundToInt(envelopeRate * 5f);
        if (historyCount < minSamples)
        {
            thresholdsValid = false;
            return;
        }

        // Order doesn't matter for percentiles, so the ring buffer can be copied as is.
        Array.Copy(history, sortScratch, historyCount);
        Array.Sort(sortScratch, 0, historyCount);

        floorLevel = sortScratch[(int)(historyCount * 0.2f)];
        peakLevel = sortScratch[Mathf.Min(historyCount - 1, (int)(historyCount * 0.95f))];

        thresholdsValid = (peakLevel - floorLevel) >= minSignalRange;
    }

    private void DetectBreath()
    {
        if (!thresholdsValid) return;

        float range = peakLevel - floorLevel;
        float upper = floorLevel + range * thresholdFraction;
        float lower = floorLevel + range * thresholdFraction * 0.6f; // Hysteresis.
        float now = Time.unscaledTime;

        if (!aboveThreshold && Level > upper)
        {
            aboveThreshold = true;

            if (now - lastBreathTime >= minBurstInterval)
            {
                RegisterBreath(now);
            }
        }
        else if (aboveThreshold && Level < lower)
        {
            aboveThreshold = false;
        }
    }

    private void RegisterBreath(float now)
    {
        // Drop old data if the user stopped breathing audibly for a while.
        if (breathTimes.Count > 0 && now - lastBreathTime > staleSeconds)
        {
            breathTimes.Clear();
        }

        lastBreathTime = now;
        breathTimes.Add(now);

        while (breathTimes.Count > intervalsToAverage + 1)
        {
            breathTimes.RemoveAt(0);
        }

        UpdateEstimate();
        OnBreathDetected?.Invoke();

        if (logDebug)
        {
            Debug.Log($"[MicrophoneBreathingTracker] Breath detected | rate: {(HasEstimate ? BreathsPerMinute.ToString("F1") : "--")}/min | regularity: {Regularity:F2}");
        }
    }

    private void UpdateEstimate()
    {
        int intervalCount = breathTimes.Count - 1;
        if (intervalCount < 2)
        {
            HasEstimate = false;
            return;
        }

        float sum = 0f;
        for (int i = 0; i < intervalCount; i++)
        {
            sum += breathTimes[i + 1] - breathTimes[i];
        }
        float mean = sum / intervalCount;

        float squaredDiffs = 0f;
        for (int i = 0; i < intervalCount; i++)
        {
            float diff = (breathTimes[i + 1] - breathTimes[i]) - mean;
            squaredDiffs += diff * diff;
        }
        float standardDeviation = Mathf.Sqrt(squaredDiffs / intervalCount);

        BreathsPerMinute = 60f / (mean * burstsPerBreath);

        // Coefficient of variation of the intervals: 0 = perfectly regular.
        Regularity = 1f - Mathf.Clamp01(standardDeviation / mean);
        HasEstimate = true;
    }

    private void UpdateDebugLabel(float dt)
    {
        if (debugLabel == null) return;

        debugTimer += dt;
        if (debugTimer < 0.25f) return;
        debugTimer = 0f;

        debugLabel.text =
            $"Level: {Level:F4}\n" +
            $"Floor / Peak: {floorLevel:F4} / {peakLevel:F4}\n" +
            $"Signal: {(thresholdsValid ? "OK" : "weak")}\n" +
            $"Rate: {(HasEstimate ? BreathsPerMinute.ToString("F1") : "--")} /min\n" +
            $"Regularity: {(HasEstimate ? Regularity.ToString("F2") : "--")}";
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Tracks the user's breathing through a controller resting on the chest or abdomen.
///
/// Pipeline:
///   controller local position -> low-pass (hand/sensor jitter)
///   -> subtract a slow baseline (drift) -> displacement vector
///   -> project onto the main movement axis (found automatically, so it works sitting or lying)
///   -> 1D breathing signal -> breath detection with hysteresis
///   -> breaths per minute, regularity and amplitude.
///
/// Starts when TriggerModeSwitcher enters Feedback mode and stops when it leaves it.
/// The first "warmupSeconds" are ignored; this matches the start delay of the BreathingGuide,
/// so the user has time to place the controller before the 4-2-4-2 pattern begins.
/// </summary>
public class ControllerBreathingTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TriggerModeSwitcher modeSwitcher;

    [Tooltip("Controller resting on the chest (e.g. the Left Controller under the XR Origin). " +
             "Its LOCAL position is used, so moving the rig doesn't affect the signal.")]
    [SerializeField] private Transform controller;

    [Tooltip("Head transform (Main Camera). Only used to decide which direction of the movement axis counts as 'inhale'.")]
    [SerializeField] private Transform head;

    [Tooltip("Optional on-screen label to tune the detection inside the headset.")]
    [SerializeField] private TMP_Text debugLabel;

    [Header("Timing")]
    [Tooltip("Seconds ignored after entering Feedback mode. Matches the BreathingGuide start delay.")]
    [SerializeField] private float warmupSeconds = 5f;

    [Header("Filtering")]
    [Tooltip("Low-pass time constant (s). Removes jitter.")]
    [SerializeField] private float jitterSmoothing = 0.3f;

    [Tooltip("Baseline time constant (s). Removes slow drift. Should be well above one breath cycle.")]
    [SerializeField] private float baselineSeconds = 10f;

    [Tooltip("Memory (s) used to find the main movement axis and the signal level.")]
    [SerializeField] private float axisSeconds = 15f;

    [Tooltip("If the controller moves more than this from its baseline, tracking restarts (controller was moved).")]
    [SerializeField] private float movementResetMeters = 0.05f;

    [Header("Breath detection")]
    [Tooltip("Minimum signal level (RMS, meters) to consider that the chest is moving. 0.001 = 1 mm.")]
    [SerializeField] private float minSignalMeters = 0.001f;

    [Tooltip("Hysteresis as a fraction of the signal RMS.")]
    [SerializeField, Range(0.1f, 0.8f)] private float thresholdFraction = 0.35f;

    [SerializeField] private float minBreathInterval = 2f;
    [SerializeField] private int intervalsToAverage = 4;

    [Tooltip("If no breath is detected for this long, the estimate is discarded.")]
    [SerializeField] private float staleSeconds = 25f;

    [SerializeField] private bool logDebug;

    // ---- Public state ----
    public bool IsTracking { get; private set; }

    /// <summary>Breathing signal in meters along the main axis (positive = chest moving outward).</summary>
    public float Signal { get; private set; }

    /// <summary>True while the signal is rising (inhale) and false while it is falling (exhale).</summary>
    public bool IsInhaling { get; private set; }

    /// <summary>True when the chest movement is large enough to be a breathing signal.</summary>
    public bool HasSignal { get; private set; }

    public float BreathsPerMinute { get; private set; }

    /// <summary>0..1. Higher means more regular intervals between breaths.</summary>
    public float Regularity { get; private set; }

    /// <summary>Peak-to-peak chest movement of the last breath, in meters.</summary>
    public float LastBreathAmplitude { get; private set; }

    public bool HasEstimate { get; private set; }

    public event Action OnBreathDetected;

    // ---- Filtering state ----
    private Vector3 smoothedPosition;
    private Vector3 baseline;
    private Vector3 axis;
    private bool axisReady;
    private float cxx, cxy, cxz, cyy, cyz, czz; // Running covariance of the displacement.
    private float timeSinceAnchor;
    private float slope;
    private float signalMeanSquare;

    // ---- Detection state ----
    private bool detectionStarted;
    private bool high;
    private float lastBreathTime = float.NegativeInfinity;
    private float cycleMax = float.NegativeInfinity;
    private float cycleMin = float.PositiveInfinity;
    private readonly List<float> breathTimes = new List<float>();

    private float debugTimer;

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
        if (IsTracking) return;

        if (controller == null)
        {
            Debug.LogWarning("[ControllerBreathingTracker] No controller assigned.");
            return;
        }

        Anchor(controller.localPosition);
        IsTracking = true;

        if (logDebug)
        {
            Debug.Log("[ControllerBreathingTracker] Tracking started.");
        }
    }

    public void StopTracking()
    {
        IsTracking = false;

        Signal = 0f;
        IsInhaling = false;
        HasSignal = false;
        LastBreathAmplitude = 0f;
        ResetEstimate();

        if (debugLabel != null)
        {
            debugLabel.text = "Controller: off";
        }
    }

    /// <summary>
    /// Restarts the filters around a new rest position. Called when tracking starts and
    /// whenever the controller is moved far away (e.g. the user repositions it).
    /// </summary>
    private void Anchor(Vector3 position)
    {
        smoothedPosition = position;
        baseline = position;

        axisReady = false;
        cxx = cxy = cxz = cyy = cyz = czz = 0f;
        signalMeanSquare = 0f;
        slope = 0f;
        timeSinceAnchor = 0f;

        detectionStarted = false;
        high = false;
        cycleMax = float.NegativeInfinity;
        cycleMin = float.PositiveInfinity;

        ResetEstimate();
    }

    private void ResetEstimate()
    {
        HasEstimate = false;
        BreathsPerMinute = 0f;
        Regularity = 0f;
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
        if (dt <= 0f) return;

        timeSinceAnchor += dt;

        Vector3 raw = controller.localPosition;

        // 1) Low-pass to remove jitter.
        float lowPassAlpha = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, jitterSmoothing));
        smoothedPosition = Vector3.Lerp(smoothedPosition, raw, lowPassAlpha);

        // 2) Slow baseline. It behaves like a running mean at first and becomes an
        //    exponential average once "baselineSeconds" have passed.
        float baselineTau = Mathf.Clamp(timeSinceAnchor, dt, baselineSeconds);
        baseline = Vector3.Lerp(baseline, smoothedPosition, dt / baselineTau);

        Vector3 displacement = smoothedPosition - baseline;

        // The controller was moved a lot: start over around the new position.
        if (displacement.sqrMagnitude > movementResetMeters * movementResetMeters)
        {
            Anchor(raw);
            return;
        }

        // 3) Find the main movement axis (chest moves along different directions
        //    depending on posture, so it is estimated instead of assumed).
        float memoryAlpha = 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, axisSeconds));
        cxx = Mathf.Lerp(cxx, displacement.x * displacement.x, memoryAlpha);
        cxy = Mathf.Lerp(cxy, displacement.x * displacement.y, memoryAlpha);
        cxz = Mathf.Lerp(cxz, displacement.x * displacement.z, memoryAlpha);
        cyy = Mathf.Lerp(cyy, displacement.y * displacement.y, memoryAlpha);
        cyz = Mathf.Lerp(cyz, displacement.y * displacement.z, memoryAlpha);
        czz = Mathf.Lerp(czz, displacement.z * displacement.z, memoryAlpha);
        UpdateAxis();

        if (!axisReady) return;

        // 4) 1D breathing signal.
        float previousSignal = Signal;
        Signal = Vector3.Dot(displacement, axis);

        slope = Mathf.Lerp(slope, (Signal - previousSignal) / dt, lowPassAlpha);
        IsInhaling = slope > 0f;

        signalMeanSquare = Mathf.Lerp(signalMeanSquare, Signal * Signal, memoryAlpha);
        float rms = Mathf.Sqrt(signalMeanSquare);
        HasSignal = rms >= minSignalMeters;

        // 5) Breath detection (ignored during the warm-up / settling time).
        if (timeSinceAnchor >= warmupSeconds)
        {
            DetectBreath(rms);
        }

        if (HasEstimate && Time.unscaledTime - lastBreathTime > staleSeconds)
        {
            ResetEstimate();
        }

        UpdateDebugLabel(dt, rms);
    }

    /// <summary>
    /// One power-iteration step per frame on the running covariance matrix. Since the axis
    /// changes slowly, this converges to the dominant direction of movement.
    /// </summary>
    private void UpdateAxis()
    {
        Vector3 seed = axisReady
            ? axis
            : (head != null ? head.forward : Vector3.forward);

        Vector3 v = new Vector3(
            cxx * seed.x + cxy * seed.y + cxz * seed.z,
            cxy * seed.x + cyy * seed.y + cyz * seed.z,
            cxz * seed.x + cyz * seed.y + czz * seed.z);

        if (v.sqrMagnitude < 1e-14f) return; // Not enough movement yet.

        v.Normalize();

        if (timeSinceAnchor < warmupSeconds && head != null)
        {
            // While warming up, choose the sign so that "positive" points away from the body,
            // assuming the user looks roughly ahead (sitting: forward, lying down: up).
            if (Vector3.Dot(v, head.forward) < 0f) v = -v;
        }
        else if (axisReady && Vector3.Dot(v, axis) < 0f)
        {
            v = -v; // Keep the sign stable afterwards.
        }

        axis = v;
        axisReady = true;
    }

    private void DetectBreath(float rms)
    {
        if (!HasSignal) return;

        float hysteresis = Mathf.Max(minSignalMeters, rms * thresholdFraction);

        // Track the peak-to-peak range of the current cycle.
        cycleMax = Mathf.Max(cycleMax, Signal);
        cycleMin = Mathf.Min(cycleMin, Signal);

        if (!detectionStarted)
        {
            // Start in whatever half of the cycle we are, so the first breath is not counted
            // at a random point of the cycle.
            detectionStarted = true;
            high = Signal > 0f;
            return;
        }

        float now = Time.unscaledTime;

        if (!high && Signal > hysteresis)
        {
            high = true;

            if (now - lastBreathTime >= minBreathInterval)
            {
                RegisterBreath(now);
            }
        }
        else if (high && Signal < -hysteresis)
        {
            high = false;
        }
    }

    private void RegisterBreath(float now)
    {
        if (cycleMax > cycleMin)
        {
            LastBreathAmplitude = cycleMax - cycleMin;
        }
        cycleMax = Signal;
        cycleMin = Signal;

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
            Debug.Log($"[ControllerBreathingTracker] Breath detected | rate: {(HasEstimate ? BreathsPerMinute.ToString("F1") : "--")}/min | regularity: {Regularity:F2} | amplitude: {LastBreathAmplitude * 1000f:F1} mm");
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

        BreathsPerMinute = 60f / mean;

        // Coefficient of variation of the intervals: 0 = perfectly regular.
        Regularity = 1f - Mathf.Clamp01(standardDeviation / mean);
        HasEstimate = true;
    }

    private void UpdateDebugLabel(float dt, float rms)
    {
        if (debugLabel == null) return;

        debugTimer += dt;
        if (debugTimer < 0.25f) return;
        debugTimer = 0f;

        string state = timeSinceAnchor < warmupSeconds
            ? $"settling ({warmupSeconds - timeSinceAnchor:F0}s)"
            : (HasSignal ? "OK" : "weak");

        debugLabel.text =
            $"Signal: {Signal * 1000f:F1} mm ({(IsInhaling ? "in" : "out")})\n" +
            $"Level (RMS): {rms * 1000f:F1} mm | {state}\n" +
            $"Rate: {(HasEstimate ? BreathsPerMinute.ToString("F1") : "--")} /min\n" +
            $"Regularity: {(HasEstimate ? Regularity.ToString("F2") : "--")}\n" +
            $"Amplitude: {LastBreathAmplitude * 1000f:F1} mm";
    }
}

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CalmWoods.Breathing
{
    // Breathing wheel: a ring split into one arc per phase (inhale / hold / exhale / hold),
    // a marker that travels around it, a center circle that grows on inhale and shrinks
    // on exhale, and a label that says what to do.
    public class BreathingGuide : MonoBehaviour
    {
        static readonly BreathPhase[] Order =
            { BreathPhase.Inhale, BreathPhase.HoldIn, BreathPhase.Exhale, BreathPhase.HoldOut };

        [SerializeField] BreathPattern pattern = new BreathPattern();
        [SerializeField] bool playOnEnable = true;
        [Tooltip("Seconds showing 'Get ready' before the first inhale.")]
        [SerializeField, Min(0f)] float startDelay = 3f;

        [Header("Ring")]
        [Tooltip("Empty, stretched RectTransform. The phase arcs are created inside it.")]
        [SerializeField] RectTransform segmentsRoot;
        [SerializeField] Sprite ringSprite;
        [SerializeField] Color inhaleColor = new Color(0.93f, 0.58f, 0.32f);
        [SerializeField] Color holdColor = new Color(0.70f, 0.48f, 0.96f);
        [SerializeField] Color exhaleColor = new Color(0.38f, 0.76f, 0.86f);
        [SerializeField, Range(0f, 15f)] float gapDegrees = 4f;
        [Tooltip("Alpha of the arcs that are not the current phase.")]
        [SerializeField, Range(0f, 1f)] float inactiveAlpha = 0.35f;

        [Header("Marker")]
        [Tooltip("Stretched RectTransform centered on the ring; the dot is its child at the top.")]
        [SerializeField] RectTransform markerPivot;

        [Header("Center circle")]
        [SerializeField] RectTransform breathCircle;
        [SerializeField] float minScale = 0.45f;
        [SerializeField] float maxScale = 1f;

        [Header("Text")]
        [SerializeField] TMP_Text phaseLabel;
        [Tooltip("Optional seconds left in the current phase.")]
        [SerializeField] TMP_Text countdownLabel;
        [SerializeField] string readyText = "GET READY";
        [SerializeField] string inhaleText = "INHALE";
        [SerializeField] string holdText = "HOLD";
        [SerializeField] string exhaleText = "EXHALE";
        [SerializeField, Min(0.01f)] float labelFadeSeconds = 0.35f;

        public event Action<BreathPhase> PhaseChanged;
        public event Action CycleCompleted;

        public BreathPattern Pattern => pattern;
        public bool IsPlaying { get; private set; }
        public bool IsWaitingToStart => IsPlaying && delayLeft > 0f;
        public BreathPhase CurrentPhase { get; private set; }
        public float PhaseProgress { get; private set; }   // 0..1 within the current phase
        public float CycleProgress { get; private set; }   // 0..1 around the whole ring
        public int CompletedCycles { get; private set; }

        readonly Dictionary<BreathPhase, Image> arcs = new Dictionary<BreathPhase, Image>();
        float cycleTime;
        float delayLeft;
        float labelTimer;

        void Awake() => BuildRing();

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        void OnDisable() => IsPlaying = false;

        public void Play()
        {
            cycleTime = 0f;
            CompletedCycles = 0;
            delayLeft = startDelay;
            IsPlaying = true;
            CurrentPhase = FirstPhase();
            Evaluate(0f);
            SetLabel(delayLeft > 0f ? readyText : TextFor(CurrentPhase));
            if (delayLeft <= 0f) PhaseChanged?.Invoke(CurrentPhase);
            Render();
        }

        public void Stop() => IsPlaying = false;

        public void SetPattern(BreathPattern newPattern)
        {
            if (newPattern == null || newPattern.CycleSeconds <= 0f) return;
            pattern = newPattern;
            BuildRing();
            if (IsPlaying) Play();
        }

        void Update()
        {
            if (!IsPlaying || pattern.CycleSeconds <= 0f) return;

            labelTimer += Time.deltaTime;

            if (delayLeft > 0f)
            {
                delayLeft -= Time.deltaTime;
                if (countdownLabel != null) countdownLabel.text = Mathf.CeilToInt(Mathf.Max(delayLeft, 0f)).ToString();
                if (delayLeft <= 0f)
                {
                    SetLabel(TextFor(CurrentPhase));
                    PhaseChanged?.Invoke(CurrentPhase);
                }
                Render();
                return;
            }

            cycleTime += Time.deltaTime;
            if (cycleTime >= pattern.CycleSeconds)
            {
                cycleTime -= pattern.CycleSeconds;
                CompletedCycles++;
                CycleCompleted?.Invoke();
            }

            BreathPhase previous = CurrentPhase;
            Evaluate(cycleTime);
            if (CurrentPhase != previous)
            {
                SetLabel(TextFor(CurrentPhase));
                PhaseChanged?.Invoke(CurrentPhase);
            }

            if (countdownLabel != null)
            {
                float left = pattern.Duration(CurrentPhase) * (1f - PhaseProgress);
                countdownLabel.text = Mathf.CeilToInt(left).ToString();
            }

            Render();
        }

        // Works out phase and progress from the time inside the cycle.
        void Evaluate(float time)
        {
            CycleProgress = time / pattern.CycleSeconds;
            float start = 0f;
            foreach (BreathPhase phase in Order)
            {
                float duration = pattern.Duration(phase);
                if (duration <= 0f) continue;
                if (time < start + duration)
                {
                    CurrentPhase = phase;
                    PhaseProgress = (time - start) / duration;
                    return;
                }
                start += duration;
            }
            PhaseProgress = 1f;
        }

        void Render()
        {
            if (markerPivot != null)
                markerPivot.localRotation = Quaternion.Euler(0f, 0f, -CycleProgress * 360f);

            if (breathCircle != null)
                breathCircle.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, CircleFill());

            foreach (KeyValuePair<BreathPhase, Image> arc in arcs)
            {
                Color c = arc.Value.color;
                c.a = arc.Key == CurrentPhase && !IsWaitingToStart ? 1f : inactiveAlpha;
                arc.Value.color = c;
            }

            if (phaseLabel != null)
                phaseLabel.alpha = Mathf.Clamp01(labelTimer / labelFadeSeconds);
        }

        // 0 = lungs empty (small circle), 1 = lungs full (big circle). Eased so it feels organic.
        float CircleFill()
        {
            if (IsWaitingToStart) return 0f;
            float eased = Mathf.SmoothStep(0f, 1f, PhaseProgress);
            return CurrentPhase switch
            {
                BreathPhase.Inhale => eased,
                BreathPhase.HoldIn => 1f,
                BreathPhase.Exhale => 1f - eased,
                _ => 0f
            };
        }

        // One filled-radial Image per phase, rotated so the arcs sit one after the other.
        void BuildRing()
        {
            if (segmentsRoot == null || ringSprite == null) return;

            for (int i = segmentsRoot.childCount - 1; i >= 0; i--)
                Destroy(segmentsRoot.GetChild(i).gameObject);
            arcs.Clear();

            float total = pattern.CycleSeconds;
            if (total <= 0f) return;

            float startDegrees = 0f;
            foreach (BreathPhase phase in Order)
            {
                float duration = pattern.Duration(phase);
                if (duration <= 0f) continue;

                float sweep = duration / total * 360f;
                float gap = Mathf.Min(gapDegrees, sweep * 0.5f);

                var go = new GameObject($"Arc_{phase}", typeof(RectTransform), typeof(Image));
                var rect = (RectTransform)go.transform;
                rect.SetParent(segmentsRoot, false);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                rect.localRotation = Quaternion.Euler(0f, 0f, -(startDegrees + gap * 0.5f));

                var image = go.GetComponent<Image>();
                image.sprite = ringSprite;
                image.raycastTarget = false;
                image.type = Image.Type.Filled;
                image.fillMethod = Image.FillMethod.Radial360;
                image.fillOrigin = (int)Image.Origin360.Top;
                image.fillClockwise = true;
                image.fillAmount = (sweep - gap) / 360f;
                image.color = ColorFor(phase);

                arcs[phase] = image;
                startDegrees += sweep;
            }
        }

        BreathPhase FirstPhase()
        {
            foreach (BreathPhase phase in Order)
                if (pattern.Duration(phase) > 0f) return phase;
            return BreathPhase.Inhale;
        }

        Color ColorFor(BreathPhase phase) => phase switch
        {
            BreathPhase.Inhale => inhaleColor,
            BreathPhase.Exhale => exhaleColor,
            _ => holdColor
        };

        string TextFor(BreathPhase phase) => phase switch
        {
            BreathPhase.Inhale => inhaleText,
            BreathPhase.Exhale => exhaleText,
            _ => holdText
        };

        void SetLabel(string text)
        {
            labelTimer = 0f;
            if (phaseLabel != null) phaseLabel.text = text;
        }
    }
}

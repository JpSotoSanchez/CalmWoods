using CalmWoods.Environment;
using UnityEngine;
using UnityEngine.UI;

namespace CalmWoods.Audio
{
    // Forest ambience for the session scene:
    //  - Day clip (Dawn, Afternoon, Sunset) or night clip (Night), cross-faded when the time changes.
    //  - Water clip that fades in as the player gets close to the water, coming from its direction.
    //  - Everything scaled by the Ambient Sound slider (SessionSettings.AmbientVolume).
    // Put it on an empty GameObject; it creates its own AudioSources.
    public class AmbientSoundscape : MonoBehaviour
    {
        [Header("Clips")]
        [SerializeField] AudioClip dayClip;      // Woods
        [SerializeField] AudioClip nightClip;    // NightWoods
        [SerializeField] AudioClip waterClip;    // River

        [Header("Volume")]
        [Tooltip("Optional. The Ambient Sound slider of this scene's settings panel.")]
        [SerializeField] Slider ambientSlider;
        [SerializeField, Range(0f, 1f)] float forestVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] float waterVolume = 1f;
        [Tooltip("Seconds to cross-fade between the day and night forest sounds.")]
        [SerializeField, Min(0.1f)] float dayNightFadeSeconds = 3f;

        [Header("Water")]
        [Tooltip("Optional. If empty, finds the object named below (the generated lake).")]
        [SerializeField] Renderer water;
        [SerializeField] string waterObjectName = "Water";
        [Tooltip("Closer than this (m) the water sound is at full volume.")]
        [SerializeField, Min(0f)] float fullVolumeDistance = 4f;
        [Tooltip("Farther than this (m) the water sound is silent.")]
        [SerializeField, Min(0.1f)] float silentDistance = 30f;
        [Tooltip("Seconds to smooth volume changes while walking (avoids jumps).")]
        [SerializeField, Min(0.01f)] float waterSmoothing = 0.75f;
        [Tooltip("How much quieter the forest gets right next to the water (0 = not at all).")]
        [SerializeField, Range(0f, 1f)] float forestDuckNearWater = 0.35f;

        [Header("Listener")]
        [Tooltip("The XR head camera. Falls back to Camera.main.")]
        [SerializeField] Transform listener;

        AudioSource daySource, nightSource, waterSource;
        float nightMix;      // 0 = day sound, 1 = night sound
        float waterMix;      // 0 = far from water, 1 = at the shore
        float nextWaterSearch;

        void Awake()
        {
            daySource = CreateSource("Ambient_Day", dayClip, spatial: false);
            nightSource = CreateSource("Ambient_Night", nightClip, spatial: false);
            waterSource = CreateSource("Ambient_Water", waterClip, spatial: true);
        }

        void Start()
        {
            if (listener == null && Camera.main != null) listener = Camera.main.transform;

            if (ambientSlider != null)
            {
                ambientSlider.minValue = 0f;
                ambientSlider.maxValue = 1f;
                ambientSlider.SetValueWithoutNotify(SessionSettings.AmbientVolume);
                ambientSlider.onValueChanged.AddListener(v => SessionSettings.AmbientVolume = v);
            }

            // Start on the right sound, without fading from the other one.
            nightMix = IsNight() ? 1f : 0f;
            UpdateVolumes();

            daySource.Play();
            nightSource.Play();
            waterSource.Play();
        }

        void Update()
        {
            float target = IsNight() ? 1f : 0f;
            nightMix = Mathf.MoveTowards(nightMix, target, Time.deltaTime / dayNightFadeSeconds);

            UpdateWater();
            UpdateVolumes();
        }

        void UpdateWater()
        {
            if (water == null && Time.time >= nextWaterSearch)
            {
                // The lake is created at runtime (and recreated if the world regenerates).
                nextWaterSearch = Time.time + 1f;
                GameObject found = GameObject.Find(waterObjectName);
                if (found != null) water = found.GetComponent<Renderer>();
            }

            float targetMix = 0f;
            if (water != null && listener != null)
            {
                // Nearest point of the water area, on the water surface.
                Bounds bounds = water.bounds;
                Vector3 nearest = bounds.ClosestPoint(listener.position);
                nearest.y = bounds.center.y;

                Vector2 flat = new Vector2(nearest.x - listener.position.x, nearest.z - listener.position.z);
                float distance = flat.magnitude;

                float t = Mathf.InverseLerp(silentDistance, fullVolumeDistance, distance);
                targetMix = Mathf.SmoothStep(0f, 1f, t);

                // The sound comes from the direction of the water.
                waterSource.transform.position = nearest;
            }

            float k = 1f - Mathf.Exp(-Time.deltaTime / waterSmoothing);
            waterMix = Mathf.Lerp(waterMix, targetMix, k);
        }

        void UpdateVolumes()
        {
            // Squared slider value feels closer to how loud people perceive it.
            float master = SessionSettings.AmbientVolume;
            master *= master;

            // Equal-power cross-fade: no loudness dip in the middle of the change.
            float day = Mathf.Cos(nightMix * Mathf.PI * 0.5f);
            float night = Mathf.Sin(nightMix * Mathf.PI * 0.5f);
            float forest = master * forestVolume * (1f - forestDuckNearWater * waterMix);

            daySource.volume = forest * day;
            nightSource.volume = forest * night;
            waterSource.volume = master * waterVolume * waterMix;
        }

        static bool IsNight() => SessionSettings.SelectedTimeOfDay == TimeOfDay.Night;

        AudioSource CreateSource(string sourceName, AudioClip clip, bool spatial)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);

            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;

            if (spatial)
            {
                // Direction from the 3D position; loudness is handled by this script,
                // so the built-in distance rolloff is kept flat.
                source.spatialBlend = 1f;
                source.dopplerLevel = 0f;
                source.spread = 90f;
                source.rolloffMode = AudioRolloffMode.Custom;
                source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Constant(0f, 1f, 1f));
                source.maxDistance = 1000f;
            }
            else
            {
                source.spatialBlend = 0f;
            }

            return source;
        }
    }
}

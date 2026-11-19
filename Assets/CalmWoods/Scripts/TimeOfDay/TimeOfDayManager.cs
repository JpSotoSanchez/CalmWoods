using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace CalmWoods.Environment
{
    // Applies a time-of-day preset to the scene (skybox, sun, ambient light, fog).
    // Put one in the menu scene (live preview) and one in the forest scene.
    public class TimeOfDayManager : MonoBehaviour
    {
        [SerializeField] TimeOfDayPreset[] presets;
        [Tooltip("Directional light used as sun/moon. Falls back to the Lighting window's Sun Source.")]
        [SerializeField] Light sun;
        [SerializeField] bool applyOnStart = true;

        public static TimeOfDayManager Instance { get; private set; }
        public TimeOfDay Current { get; private set; }
        public event Action<TimeOfDay> Changed;

        void Awake()
        {
            Instance = this;
            if (sun == null) sun = RenderSettings.sun;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            if (applyOnStart) Apply(SessionSettings.SelectedTimeOfDay);
        }

        public bool IsAvailable(TimeOfDay timeOfDay)
        {
            TimeOfDayPreset preset = Find(timeOfDay);
            return preset != null && preset.IsUsable;
        }

        // For Button.onClick in the inspector (enums are not listed there):
        // 0 = Dawn, 1 = Afternoon, 2 = Sunset, 3 = Night.
        public void SetTimeOfDay(int timeOfDay) => Apply((TimeOfDay)timeOfDay);

        public void Apply(TimeOfDay timeOfDay)
        {
            TimeOfDayPreset preset = Find(timeOfDay);
            if (preset == null || !preset.IsUsable)
            {
                Debug.LogWarning($"[TimeOfDay] '{timeOfDay}' is not available, using {SessionSettings.DefaultTimeOfDay}.", this);
                preset = Find(SessionSettings.DefaultTimeOfDay);
                if (preset == null || !preset.IsUsable) return;
            }

            RenderSettings.skybox = preset.skybox;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = preset.ambientSky;
            RenderSettings.ambientEquatorColor = preset.ambientEquator;
            RenderSettings.ambientGroundColor = preset.ambientGround;

            RenderSettings.fog = preset.fogEnabled;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = preset.fogColor;
            RenderSettings.fogStartDistance = preset.fogStart;
            RenderSettings.fogEndDistance = Mathf.Max(preset.fogEnd, preset.fogStart + 1f);

            if (sun != null)
            {
                sun.color = preset.sunColor;
                sun.intensity = preset.sunIntensity;
                sun.transform.rotation = Quaternion.Euler(preset.sunRotation);
            }

            // Refresh sky reflections once; this is not done every frame.
            DynamicGI.UpdateEnvironment();

            Current = preset.timeOfDay;
            SessionSettings.SelectedTimeOfDay = Current;
            Changed?.Invoke(Current);
        }

        TimeOfDayPreset Find(TimeOfDay timeOfDay)
        {
            if (presets == null) return null;
            foreach (TimeOfDayPreset preset in presets)
                if (preset != null && preset.timeOfDay == timeOfDay) return preset;
            return null;
        }
    }
}

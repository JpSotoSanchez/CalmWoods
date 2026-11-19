using UnityEngine;

namespace CalmWoods.Environment
{
    // Choices made in the menu that the forest scene reads when it loads.
    // Time of day resets to the default on every launch; volumes are remembered.
    public static class SessionSettings
    {
        public const TimeOfDay DefaultTimeOfDay = TimeOfDay.Afternoon;
        public const float DefaultVolume = 0.7f;

        const string MusicKey = "CalmWoods.MusicVolume";
        const string AmbientKey = "CalmWoods.AmbientVolume";

        public static TimeOfDay SelectedTimeOfDay { get; set; } = DefaultTimeOfDay;

        // 0..1, linear slider value. Convert to dB when applying to an AudioMixer.
        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicKey, DefaultVolume);
            set => PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value));
        }

        public static float AmbientVolume
        {
            get => PlayerPrefs.GetFloat(AmbientKey, DefaultVolume);
            set => PlayerPrefs.SetFloat(AmbientKey, Mathf.Clamp01(value));
        }

        public static void Save() => PlayerPrefs.Save();
    }
}

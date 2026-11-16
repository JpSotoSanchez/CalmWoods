using UnityEngine;

namespace CalmWoods.Environment
{
    // Look of one time of day: sky, sun light, ambient light and fog color.
    [CreateAssetMenu(menuName = "Calm Woods/Time Of Day Preset", fileName = "TimeOfDayPreset")]
    public class TimeOfDayPreset : ScriptableObject
    {
        public TimeOfDay timeOfDay;

        [Tooltip("Turn off while the preset has no assets yet. Its button gets disabled.")]
        public bool isAvailable = true;

        [Header("Sky")]
        public Material skybox;

        [Header("Sun / Moon")]
        public Color sunColor = Color.white;
        [Min(0f)] public float sunIntensity = 1f;
        [Tooltip("X = height above the horizon (degrees), Y = compass direction.")]
        public Vector3 sunRotation = new Vector3(40f, -40f, 0f);

        [Header("Ambient light (gradient)")]
        public Color ambientSky = new Color(0.55f, 0.65f, 0.8f);
        public Color ambientEquator = new Color(0.45f, 0.5f, 0.5f);
        public Color ambientGround = new Color(0.2f, 0.18f, 0.15f);

        [Header("Fog")]
        [Tooltip("Scene fog: distant trees and mountains fade into the sky.")]
        public bool fogEnabled = true;
        [Tooltip("Also tints the haze at the horizon of the skybox. Match it to the sky's horizon color.")]
        public Color fogColor = new Color(0.75f, 0.85f, 0.95f);
        [Tooltip("Linear fog: distance (m) where it starts and where it is fully opaque.")]
        [Min(0f)] public float fogStart = 15f;
        [Min(0f)] public float fogEnd = 90f;

        public bool IsUsable => isAvailable && skybox != null;
    }
}

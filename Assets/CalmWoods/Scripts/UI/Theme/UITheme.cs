using TMPro;
using UnityEngine;

namespace CalmWoods.UI
{
    // The look of the whole UI in one place: font, colors, sprites and text sizes.
    // Change a value here and press "Apply Theme" on each canvas to update everything.
    [CreateAssetMenu(menuName = "Calm Woods/UI Theme", fileName = "CalmWoodsUITheme")]
    public class UITheme : ScriptableObject
    {
        [Header("Font")]
        public TMP_FontAsset font;

        [Header("Text sizes (canvas units; canvas scale 0.001, seen from ~1.5 m)")]
        public float titleSize = 72f;
        public float headingSize = 48f;
        public float bodySize = 36f;
        public float captionSize = 28f;
        public float buttonSize = 40f;
        [Tooltip("Extra letter spacing for titles (TMP character spacing).")]
        public float titleSpacing = 4f;

        [Header("Colors")]
        public Color panel = new Color(0.118f, 0.169f, 0.133f, 0.88f);        // #1E2B22, 88 %
        public Color surface = new Color(0.180f, 0.247f, 0.200f, 1f);         // #2E3F33  secondary buttons
        public Color primary = new Color(0.878f, 0.627f, 0.251f, 1f);         // #E0A040  main action / selected
        public Color textPrimary = new Color(0.953f, 0.929f, 0.878f, 1f);     // #F3EDE0  cream
        public Color textSecondary = new Color(0.788f, 0.761f, 0.690f, 1f);   // #C9C2B0
        public Color textOnPrimary = new Color(0.118f, 0.169f, 0.133f, 1f);   // dark text on amber
        public Color sliderTrack = new Color(0.231f, 0.302f, 0.251f, 1f);     // #3B4D40
        public Color sliderFill = new Color(0.561f, 0.749f, 0.541f, 1f);      // #8FBF8A  sage green
        public Color sliderHandle = new Color(0.953f, 0.929f, 0.878f, 1f);

        [Header("Interaction tints (multiplied over the colors above)")]
        public Color normalTint = new Color(0.88f, 0.88f, 0.88f, 1f);
        public Color highlightedTint = Color.white;
        public Color pressedTint = new Color(0.72f, 0.72f, 0.72f, 1f);
        public Color disabledTint = new Color(0.55f, 0.55f, 0.55f, 0.5f);
        [Min(0f)] public float fadeDuration = 0.08f;

        [Header("Sprites (9-sliced)")]
        public Sprite buttonSprite;
        public Sprite panelSprite;
        public Sprite pillSprite;
        public Sprite handleSprite;

        [Header("Slider")]
        [Tooltip("How much bigger than the slider's height the round handle is.")]
        public float handleOversize = 24f;
    }
}

using System.Collections.Generic;
using CalmWoods.Environment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CalmWoods.UI
{
    // Applies a UITheme to every button, slider, text and panel under this object.
    // Use it in the editor: right-click the component > "Apply Theme", then save the scene.
    // Tag special elements with a UIStyle component (Title, PrimaryButton, Panel...).
    public class UIThemeApplier : MonoBehaviour
    {
        [SerializeField] UITheme theme;
        [Tooltip("Also apply when the scene starts (not needed if you applied it in the editor and saved).")]
        [SerializeField] bool applyOnAwake;

        public UITheme Theme
        {
            get => theme;
            set => theme = value;
        }

        void Awake()
        {
            if (applyOnAwake) Apply();
        }

        [ContextMenu("Apply Theme")]
        public void Apply()
        {
            if (theme == null)
            {
                Debug.LogWarning("[UITheme] No theme assigned.", this);
                return;
            }

            var styledTexts = new HashSet<TMP_Text>();
            int buttons = 0, sliders = 0, texts = 0, panels = 0;

            foreach (UIStyle style in GetComponentsInChildren<UIStyle>(true))
            {
                if (style.role != UIStyle.Role.Panel) continue;
                if (style.TryGetComponent(out Image image))
                {
                    Record(image);
                    SetSliced(image, theme.panelSprite);
                    image.color = theme.panel;
                    panels++;
                }
            }

            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                UIStyle.Role role = RoleOf(button);
                if (role == UIStyle.Role.Ignore) continue;
                StyleButton(button, role == UIStyle.Role.PrimaryButton, styledTexts);
                buttons++;
            }

            foreach (Slider slider in GetComponentsInChildren<Slider>(true))
            {
                if (RoleOf(slider) == UIStyle.Role.Ignore) continue;
                StyleSlider(slider);
                sliders++;
            }

            foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
            {
                if (styledTexts.Contains(text)) continue;
                UIStyle.Role role = RoleOf(text);
                if (role == UIStyle.Role.Ignore) continue;
                StyleText(text, role);
                texts++;
            }

            Debug.Log($"[UITheme] Applied to {buttons} buttons, {sliders} sliders, {texts} texts, {panels} panels.", this);
        }

        void StyleButton(Button button, bool primary, HashSet<TMP_Text> styledTexts)
        {
            Record(button);
            button.transition = Selectable.Transition.ColorTint;
            button.colors = Tints();

            if (button.targetGraphic is Image image)
            {
                Record(image);
                SetSliced(image, theme.buttonSprite);
                image.color = primary ? theme.primary : theme.surface;
            }

            Color labelColor = primary ? theme.textOnPrimary : theme.textPrimary;
            foreach (TMP_Text label in button.GetComponentsInChildren<TMP_Text>(true))
            {
                Record(label);
                label.font = theme.font;
                label.fontSize = theme.buttonSize;
                label.enableAutoSizing = false;
                label.fontStyle = FontStyles.Bold;
                label.color = labelColor;
                styledTexts.Add(label);
            }

            // Time-of-day buttons recolor themselves when selected: give them the theme colors.
            if (button.TryGetComponent(out TimeOfDayButton timeButton))
            {
                Record(timeButton);
                timeButton.SetColors(theme.surface, theme.primary, theme.textPrimary, theme.textOnPrimary);
            }
        }

        void StyleSlider(Slider slider)
        {
            Record(slider);
            slider.transition = Selectable.Transition.ColorTint;
            slider.colors = Tints();

            Transform background = slider.transform.Find("Background");
            if (background != null && background.TryGetComponent(out Image track))
            {
                Record(track);
                SetSliced(track, theme.pillSprite);
                track.color = theme.sliderTrack;
            }

            if (slider.fillRect != null && slider.fillRect.TryGetComponent(out Image fill))
            {
                Record(fill);
                SetSliced(fill, theme.pillSprite);
                fill.color = theme.sliderFill;
            }

            if (slider.handleRect != null)
            {
                if (slider.handleRect.TryGetComponent(out Image handle))
                {
                    Record(handle);
                    handle.sprite = theme.handleSprite;
                    handle.type = Image.Type.Simple;
                    handle.preserveAspect = true;
                    handle.color = theme.sliderHandle;
                    slider.targetGraphic = handle;
                }

                // Round handle a bit bigger than the bar: easy to grab with the ray.
                Record(slider.handleRect);
                float height = ((RectTransform)slider.transform).rect.height;
                slider.handleRect.sizeDelta = new Vector2(height + theme.handleOversize, theme.handleOversize);
            }
        }

        void StyleText(TMP_Text text, UIStyle.Role role)
        {
            Record(text);
            text.font = theme.font;
            text.color = theme.textPrimary;

            switch (role)
            {
                case UIStyle.Role.Title:
                    text.fontSize = theme.titleSize;
                    text.fontStyle = FontStyles.Bold;
                    text.characterSpacing = theme.titleSpacing;
                    text.enableAutoSizing = false;
                    break;
                case UIStyle.Role.Heading:
                    text.fontSize = theme.headingSize;
                    text.fontStyle = FontStyles.Bold;
                    text.enableAutoSizing = false;
                    break;
                case UIStyle.Role.Body:
                    text.fontSize = theme.bodySize;
                    text.fontStyle = FontStyles.Normal;
                    text.enableAutoSizing = false;
                    break;
                case UIStyle.Role.Caption:
                    text.fontSize = theme.captionSize;
                    text.fontStyle = FontStyles.Normal;
                    text.color = theme.textSecondary;
                    text.enableAutoSizing = false;
                    break;
            }
        }

        ColorBlock Tints()
        {
            ColorBlock block = ColorBlock.defaultColorBlock;
            block.normalColor = theme.normalTint;
            block.highlightedColor = theme.highlightedTint;
            block.pressedColor = theme.pressedTint;
            block.selectedColor = theme.normalTint;
            block.disabledColor = theme.disabledTint;
            block.colorMultiplier = 1f;
            block.fadeDuration = theme.fadeDuration;
            return block;
        }

        static void SetSliced(Image image, Sprite sprite)
        {
            if (sprite == null) return;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
        }

        static UIStyle.Role RoleOf(Component component) =>
            component.TryGetComponent(out UIStyle style) ? style.role : UIStyle.Role.Auto;

        static void Record(Object target)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Undo.RecordObject(target, "Apply UI Theme");
                UnityEditor.EditorUtility.SetDirty(target);
            }
#endif
        }
    }
}

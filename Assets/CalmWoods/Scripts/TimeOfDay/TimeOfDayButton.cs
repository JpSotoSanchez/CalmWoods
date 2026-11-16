using UnityEngine;
using UnityEngine.UI;

namespace CalmWoods.Environment
{
    // Put on each time-of-day button. Applies its option on click, highlights itself
    // while selected, and disables itself if its preset is not available yet.
    [RequireComponent(typeof(Button))]
    public class TimeOfDayButton : MonoBehaviour
    {
        [SerializeField] TimeOfDay option;
        [Tooltip("Optional. Uses TimeOfDayManager.Instance when empty.")]
        [SerializeField] TimeOfDayManager manager;

        [Header("Selected look")]
        [Tooltip("Image recolored when selected. Defaults to the button's Target Graphic.")]
        [SerializeField] Image background;
        [SerializeField] Color normalColor = new Color(0.94f, 0.94f, 0.94f);
        [SerializeField] Color selectedColor = new Color(0.878f, 0.627f, 0.251f); // #E0A040
        [Tooltip("Optional object shown only while selected (outline, check mark...).")]
        [SerializeField] GameObject selectedIndicator;

        Button button;

        void Awake()
        {
            button = GetComponent<Button>();
            if (background == null) background = button.targetGraphic as Image;
        }

        void OnEnable()
        {
            button.onClick.AddListener(OnClicked);
            if (Manager != null) Manager.Changed += Refresh;
        }

        void OnDisable()
        {
            button.onClick.RemoveListener(OnClicked);
            if (Manager != null) Manager.Changed -= Refresh;
        }

        void Start()
        {
            // Subscribe again in case the manager did not exist yet during OnEnable.
            if (Manager != null)
            {
                Manager.Changed -= Refresh;
                Manager.Changed += Refresh;
                button.interactable = Manager.IsAvailable(option);
            }
            Refresh(SessionSettings.SelectedTimeOfDay);
        }

        TimeOfDayManager Manager => manager != null ? manager : TimeOfDayManager.Instance;

        void OnClicked()
        {
            if (Manager != null) Manager.Apply(option);
            else SessionSettings.SelectedTimeOfDay = option;
        }

        void Refresh(TimeOfDay current)
        {
            bool selected = current == option;
            // Button color tint multiplies on top of this, so hover/pressed still show.
            if (background != null) background.color = selected ? selectedColor : normalColor;
            if (selectedIndicator != null) selectedIndicator.SetActive(selected);
        }
    }
}

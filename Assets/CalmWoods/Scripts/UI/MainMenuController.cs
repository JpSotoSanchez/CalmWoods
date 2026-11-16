using System.Collections;
using CalmWoods.Environment;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CalmWoods.UI
{
    // Navigation of the main menu: Main <-> About, Main <-> Setup, Start session, Exit.
    // Button listeners are added from code, so leave the buttons' OnClick lists empty.
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] GameObject mainPanel;
        [SerializeField] GameObject aboutPanel;
        [SerializeField] GameObject setupPanel;

        [Header("Main panel buttons")]
        [SerializeField] Button startButton;
        [SerializeField] Button aboutButton;
        [SerializeField] Button exitButton;

        [Header("About panel buttons")]
        [SerializeField] Button aboutBackButton;

        [Header("Setup panel")]
        [SerializeField] Button beginSessionButton;
        [Tooltip("Optional until the panel has a Back button.")]
        [SerializeField] Button setupBackButton;
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider ambientSlider;

        [Header("Session")]
        [Tooltip("Must be listed in File > Build Profiles > Scene List.")]
        [SerializeField] string sessionSceneName = "CalmWoodsScene";

        [Header("Transitions")]
        [SerializeField, Min(0f)] float fadeSeconds = 0.2f;

        GameObject currentPanel;
        Coroutine transition;
        bool loading;

        void Awake()
        {
            AddListener(startButton, ShowSetup);
            AddListener(aboutButton, ShowAbout);
            AddListener(exitButton, ExitApp);
            AddListener(aboutBackButton, ShowMain);
            AddListener(beginSessionButton, BeginSession);
            AddListener(setupBackButton, ShowMain);

            SetupSlider(musicSlider, SessionSettings.MusicVolume, v => SessionSettings.MusicVolume = v);
            SetupSlider(ambientSlider, SessionSettings.AmbientVolume, v => SessionSettings.AmbientVolume = v);
        }

        void Start()
        {
            // Always open on the main panel, whatever was left active in the editor.
            foreach (GameObject panel in Panels()) SetVisible(panel, false);
            SetVisible(mainPanel, true);
            currentPanel = mainPanel;
        }

        public void ShowMain() => SwitchTo(mainPanel);
        public void ShowAbout() => SwitchTo(aboutPanel);
        public void ShowSetup() => SwitchTo(setupPanel);

        public void BeginSession()
        {
            if (loading) return;
            if (!Application.CanStreamedLevelBeLoaded(sessionSceneName))
            {
                Debug.LogError($"[Menu] Scene '{sessionSceneName}' is not in the build scene list.", this);
                return;
            }

            loading = true;
            SessionSettings.Save();
            SetInteractable(currentPanel, false);
            SceneManager.LoadSceneAsync(sessionSceneName);
        }

        public void ExitApp()
        {
            SessionSettings.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void SwitchTo(GameObject target)
        {
            // Ignore clicks during a transition so panels never end half-faded.
            if (target == null || target == currentPanel || loading || transition != null) return;
            transition = StartCoroutine(Fade(currentPanel, target));
        }

        IEnumerator Fade(GameObject from, GameObject to)
        {
            currentPanel = to;

            if (from != null)
            {
                SetInteractable(from, false);
                yield return FadeAlpha(from, 1f, 0f);
                from.SetActive(false);
            }

            SetVisible(to, true, 0f);
            SetInteractable(to, false);
            yield return FadeAlpha(to, 0f, 1f);
            SetInteractable(to, true);
            transition = null;
        }

        IEnumerator FadeAlpha(GameObject panel, float from, float to)
        {
            CanvasGroup group = GroupOf(panel);
            for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / fadeSeconds);
                yield return null;
            }
            group.alpha = to;
        }

        void SetVisible(GameObject panel, bool visible, float alpha = 1f)
        {
            if (panel == null) return;
            panel.SetActive(visible);
            CanvasGroup group = GroupOf(panel);
            group.alpha = visible ? alpha : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        void SetInteractable(GameObject panel, bool interactable)
        {
            if (panel == null) return;
            CanvasGroup group = GroupOf(panel);
            group.interactable = interactable;
            group.blocksRaycasts = interactable;
        }

        static CanvasGroup GroupOf(GameObject panel)
        {
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            return group != null ? group : panel.AddComponent<CanvasGroup>();
        }

        IEnumerable Panels()
        {
            if (mainPanel != null) yield return mainPanel;
            if (aboutPanel != null) yield return aboutPanel;
            if (setupPanel != null) yield return setupPanel;
        }

        static void AddListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null) button.onClick.AddListener(action);
        }

        static void SetupSlider(Slider slider, float value, UnityEngine.Events.UnityAction<float> onChanged)
        {
            if (slider == null) return;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(onChanged);
        }
    }
}

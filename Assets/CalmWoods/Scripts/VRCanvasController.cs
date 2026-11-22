using System.Collections;
using UnityEngine;

/// <summary>
/// Shows and hides a world-space UI canvas when the app enters/leaves a given mode.
/// Places the canvas in front of the user's head when it opens and fades it in/out.
///
/// IMPORTANT: put this script on an object that stays ACTIVE (e.g. an empty "UIRoot"),
/// and make the canvas a child of it. This script toggles the child, so if it lived on
/// the canvas itself it would deactivate itself and stop listening for events.
/// </summary>
public class VRCanvasController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TriggerModeSwitcher modeSwitcher;

    [Tooltip("The mode in which this canvas is visible (Feedback for the breathing UI, Options for the menu).")]
    [SerializeField] private AppMode showInMode = AppMode.Feedback;

    [Tooltip("The World Space canvas GameObject (child of this object).")]
    [SerializeField] private GameObject canvasRoot;

    [Tooltip("CanvasGroup on the canvas root, used for the fade and to block clicks while hidden.")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Tooltip("Head transform (usually the Main Camera inside the XR Origin). Falls back to Camera.main.")]
    [SerializeField] private Transform head;

    [Header("Placement")]
    [SerializeField] private float distance = 1.2f;
    [SerializeField] private float heightOffset = -0.1f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.25f;

    private Coroutine fadeRoutine;

    /// <summary>True from Open() until Close() is called.</summary>
    public bool IsOpen { get; private set; }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    private void Awake()
    {
        if (head == null && Camera.main != null)
        {
            head = Camera.main.transform;
        }

        // Start hidden.
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasRoot.SetActive(false);
    }

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
    }

    private void HandleModeChanged(AppMode mode)
    {
        if (mode == showInMode)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    public void Open()
    {
        IsOpen = true;
        PlaceInFrontOfHead();
        canvasRoot.SetActive(true);
        StartFade(targetAlpha: 1f, deactivateAtEnd: false);
    }

    public void Close()
    {
        // Already hidden and not fading: nothing to do (happens when the OTHER canvas changes mode).
        if (!canvasRoot.activeSelf && fadeRoutine == null) return;

        // Block clicks immediately so the user can't press buttons while it fades out.
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        StartFade(targetAlpha: 0f, deactivateAtEnd: true);
    }

    private void StartFade(float targetAlpha, bool deactivateAtEnd)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, deactivateAtEnd));
    }

    private IEnumerator FadeRoutine(float targetAlpha, bool deactivateAtEnd)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            // Unscaled time so the fade still works if the game is paused (timeScale = 0).
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (deactivateAtEnd)
        {
            canvasRoot.SetActive(false);
        }
        else
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        fadeRoutine = null;
    }

    private void PlaceInFrontOfHead()
    {
        if (head == null) return;

        // Use only the horizontal direction so the canvas doesn't tilt with the user's gaze.
        Vector3 flatForward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.0001f)
        {
            flatForward = head.up; // Looking straight up/down: fall back to a sane direction.
        }
        flatForward.Normalize();

        Vector3 position = head.position + flatForward * distance;
        position.y = head.position.y + heightOffset;

        canvasRoot.transform.position = position;

        // A world-space canvas is readable when the user looks along its +Z axis.
        Vector3 lookDirection = position - head.position;
        lookDirection.y = 0f;
        canvasRoot.transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}
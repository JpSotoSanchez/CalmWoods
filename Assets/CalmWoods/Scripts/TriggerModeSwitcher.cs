using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Modes the app can be in. Exploration is the default; Feedback means the UI
/// is open and the breathing feedback should be running.
/// </summary>
public enum AppMode
{
    Exploration,
    Feedback
}

/// <summary>
/// Owns the current app mode. The right-hand trigger only OPENS Feedback mode,
/// because the same trigger is used to click UI buttons with the controller ray;
/// closing is done through ExitFeedback() (e.g. from a Close button in the UI).
/// Other systems subscribe to OnModeChanged and react on their own.
/// </summary>
public class TriggerModeSwitcher : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Button action bound to the right-hand trigger (e.g. <XRController>{RightHand}/triggerPressed).")]
    [SerializeField] private InputActionReference rightTriggerAction;

    public AppMode CurrentMode { get; private set; } = AppMode.Exploration;

    /// <summary>Raised every time the mode changes.</summary>
    public event Action<AppMode> OnModeChanged;

    private void OnEnable()
    {
        if (rightTriggerAction == null) return;

        rightTriggerAction.action.Enable();
        rightTriggerAction.action.performed += OnTriggerPerformed;
    }

    private void OnDisable()
    {
        if (rightTriggerAction == null) return;

        rightTriggerAction.action.performed -= OnTriggerPerformed;
    }

    private void OnTriggerPerformed(InputAction.CallbackContext context)
    {
        // Only open: while the UI is open the trigger is used to press its buttons.
        if (CurrentMode == AppMode.Exploration)
        {
            EnterFeedback();
        }
    }

    // Parameterless public methods so they can be wired to UI Button onClick events.
    public void EnterFeedback() => SetMode(AppMode.Feedback);
    public void ExitFeedback() => SetMode(AppMode.Exploration);

    public void SetMode(AppMode newMode)
    {
        if (newMode == CurrentMode) return;

        CurrentMode = newMode;
        OnModeChanged?.Invoke(newMode);
    }
}

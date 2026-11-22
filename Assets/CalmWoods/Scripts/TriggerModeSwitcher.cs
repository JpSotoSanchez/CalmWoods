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
/// Owns the current app mode. Every press of the right-hand trigger toggles
/// between Exploration and Feedback. Other systems subscribe to OnModeChanged
/// and react on their own.
/// </summary>
public class TriggerModeSwitcher : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Button action bound to the right-hand trigger (e.g. <XRController>{RightHand}/{TriggerButton}).")]
    [SerializeField] private InputActionReference rightTriggerAction;

    /// <summary>False = Exploration, true = Feedback (breathing) mode.</summary>
    public bool IsFeedbackActive { get; private set; }

    public AppMode CurrentMode => IsFeedbackActive ? AppMode.Feedback : AppMode.Exploration;

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
        // Flip the boolean on every press.
        SetFeedbackActive(!IsFeedbackActive);
    }

    // Parameterless public methods so they can be wired to UI Button onClick events.
    public void EnterFeedback() => SetFeedbackActive(true);
    public void ExitFeedback() => SetFeedbackActive(false);

    public void SetFeedbackActive(bool active)
    {
        if (active == IsFeedbackActive) return;

        IsFeedbackActive = active;
        OnModeChanged?.Invoke(CurrentMode);
    }

    public void SetMode(AppMode newMode) => SetFeedbackActive(newMode == AppMode.Feedback);
}
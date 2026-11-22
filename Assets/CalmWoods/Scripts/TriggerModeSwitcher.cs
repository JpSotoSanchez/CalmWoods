using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// App modes. Only one is active at a time:
/// Exploration = just the forest, Feedback = breathing UI + feedback, Options = options menu.
/// </summary>
public enum AppMode
{
    Exploration,
    Feedback,
    Options
}

/// <summary>
/// Owns the current app mode. The right B button toggles Feedback mode and the
/// left Menu button toggles Options. Pressing the button of the active mode goes
/// back to Exploration; pressing the other one switches directly to that mode.
/// Other systems subscribe to OnModeChanged and react on their own.
/// </summary>
public class TriggerModeSwitcher : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Right-hand B button (<XRController>{RightHand}/{SecondaryButton}).")]
    [SerializeField] private InputActionReference toggleBreathingAction;

    [Tooltip("Left-hand Menu button, the three lines (<XRController>{LeftHand}/{Menu}).")]
    [SerializeField] private InputActionReference toggleOptionsAction;

    public AppMode CurrentMode { get; private set; } = AppMode.Exploration;

    /// <summary>True while the breathing / feedback mode is running.</summary>
    public bool IsFeedbackActive => CurrentMode == AppMode.Feedback;

    /// <summary>Raised every time the mode changes.</summary>
    public event Action<AppMode> OnModeChanged;

    private void OnEnable()
    {
        Subscribe(toggleBreathingAction, OnBreathingPressed);
        Subscribe(toggleOptionsAction, OnOptionsPressed);
    }

    private void OnDisable()
    {
        Unsubscribe(toggleBreathingAction, OnBreathingPressed);
        Unsubscribe(toggleOptionsAction, OnOptionsPressed);
    }

    private static void Subscribe(InputActionReference reference, Action<InputAction.CallbackContext> callback)
    {
        if (reference == null) return;

        reference.action.Enable();
        reference.action.performed += callback;
    }

    private static void Unsubscribe(InputActionReference reference, Action<InputAction.CallbackContext> callback)
    {
        if (reference == null) return;

        reference.action.performed -= callback;
    }

    private void OnBreathingPressed(InputAction.CallbackContext context) => Toggle(AppMode.Feedback);
    private void OnOptionsPressed(InputAction.CallbackContext context) => Toggle(AppMode.Options);

    /// <summary>If the target mode is already active, go back to Exploration; otherwise switch to it.</summary>
    public void Toggle(AppMode target)
    {
        SetMode(CurrentMode == target ? AppMode.Exploration : target);
    }

    // Parameterless public methods so they can be wired to UI Button onClick events.
    public void EnterFeedback() => SetMode(AppMode.Feedback);
    public void ExitFeedback() => SetMode(AppMode.Exploration);
    public void OpenOptions() => SetMode(AppMode.Options);
    public void CloseOptions() => SetMode(AppMode.Exploration);

    public void SetMode(AppMode newMode)
    {
        if (newMode == CurrentMode) return;

        CurrentMode = newMode;
        OnModeChanged?.Invoke(newMode);
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
/// Owns the current app mode. The right-hand trigger toggles Feedback mode:
/// it opens the UI, and closes it again as long as the controller ray is NOT
/// pointing at the UI (otherwise the trigger is just clicking a UI button).
/// Other systems subscribe to OnModeChanged and react on their own.
/// </summary>
public class TriggerModeSwitcher : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Button action bound to the right-hand trigger (e.g. <XRController>{RightHand}/{TriggerButton}).")]
    [SerializeField] private InputActionReference rightTriggerAction;

    [Tooltip("Right-hand Near-Far interactor, used to know if the trigger is clicking UI.")]
    [SerializeField] private NearFarInteractor rightInteractor;

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
        if (CurrentMode == AppMode.Exploration)
        {
            EnterFeedback();
            return;
        }

        // Feedback mode: if the ray is over the UI, the trigger is a button click, so ignore it.
        if (IsPointingAtUI()) return;

        ExitFeedback();
    }

    private bool IsPointingAtUI()
    {
        if (rightInteractor == null) return false;
        return rightInteractor.TryGetCurrentUIRaycastResult(out _);
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
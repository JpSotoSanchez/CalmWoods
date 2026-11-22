using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Opens and closes a VRCanvasController with its own controller button
/// (for example the settings menu on the left Menu button).
/// If a TriggerModeSwitcher is assigned, this menu and the breathing UI never
/// overlap: opening this one leaves breathing mode, and entering breathing mode
/// closes this one.
///
/// Put it on the same always-active root object as its VRCanvasController.
/// </summary>
public class CanvasToggleInput : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("Button action that opens/closes this canvas (e.g. CalmWoodsInput > Mode > ToggleSettings).")]
    [SerializeField] private InputActionReference toggleAction;

    [Header("References")]
    [SerializeField] private VRCanvasController canvasController;

    [Tooltip("Optional. The breathing UI's switcher, so both menus are never open at once.")]
    [SerializeField] private TriggerModeSwitcher modeSwitcher;

    private void Reset()
    {
        canvasController = GetComponent<VRCanvasController>();
    }

    private void OnEnable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.Enable();
            toggleAction.action.performed += OnTogglePerformed;
        }

        if (modeSwitcher != null)
        {
            modeSwitcher.OnModeChanged += HandleModeChanged;
        }
    }

    private void OnDisable()
    {
        if (toggleAction != null)
        {
            toggleAction.action.performed -= OnTogglePerformed;
        }

        if (modeSwitcher != null)
        {
            modeSwitcher.OnModeChanged -= HandleModeChanged;
        }
    }

    private void OnTogglePerformed(InputAction.CallbackContext context) => Toggle();

    // Parameterless public methods so they can also be wired to UI Button onClick events.
    public void Toggle()
    {
        if (canvasController == null) return;

        if (canvasController.IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (canvasController == null) return;

        // Leave breathing mode first so the two canvases don't spawn on top of each other.
        if (modeSwitcher != null) modeSwitcher.ExitFeedback();
        canvasController.Open();
    }

    public void Close()
    {
        if (canvasController != null) canvasController.Close();
    }

    private void HandleModeChanged(AppMode mode)
    {
        if (mode == AppMode.Feedback && canvasController != null && canvasController.IsOpen)
        {
            canvasController.Close();
        }
    }
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CalmWoods.UI
{
    // Interactive controls tutorial shown in world space at the start of the session.
    // Each step waits for the user to actually do the action (move, turn, open the breathing
    // guide, open the options). Text-only steps continue with the trigger of either controller.
    // The panel hides while another mode (breathing / options) is open and comes back after.
    //
    // Put this on an always-active root object; the tutorial canvas is a child of it.
    public class TutorialGuide : MonoBehaviour
    {
        public enum StepGoal
        {
            ReadAndWait,            // Text only: continues with the trigger
            MoveStick,              // Left joystick
            TurnStick,              // Right joystick, left/right
            OpenAndCloseBreathing,  // B twice
            OpenAndCloseOptions     // Y twice
        }

        [Serializable]
        public class Step
        {
            public string title;
            [TextArea(2, 5)] public string body;
            public StepGoal goal;
            [Tooltip("Optional picture of the controller / button.")]
            public Sprite icon;
        }

        [Header("References")]
        [SerializeField] TriggerModeSwitcher modeSwitcher;
        [Tooltip("The World Space canvas GameObject (child of this object).")]
        [SerializeField] GameObject panel;
        [SerializeField] CanvasGroup canvasGroup;
        [Tooltip("Main Camera of the XR Origin. Falls back to Camera.main.")]
        [SerializeField] Transform head;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text bodyLabel;
        [Tooltip("Optional, e.g. '2 / 6'.")]
        [SerializeField] TMP_Text progressLabel;
        [Tooltip("Optional. Shows 'Done!' when a step is completed.")]
        [SerializeField] TMP_Text hintLabel;
        [SerializeField] Image iconImage;

        [Header("Placement")]
        [SerializeField] float distance = 1.4f;
        [SerializeField] float heightOffset = -0.15f;

        [Header("Input (default bindings for Quest Touch controllers)")]
        [SerializeField] InputAction moveInput =
            new InputAction("Move", InputActionType.Value, "<XRController>{LeftHand}/{Primary2DAxis}");
        [SerializeField] InputAction turnInput =
            new InputAction("Turn", InputActionType.Value, "<XRController>{RightHand}/{Primary2DAxis}");
        [Tooltip("Continues text-only steps. Trigger of either controller (A is jump, so it isn't used).")]
        [SerializeField] InputAction continueInput = TriggerAction();
        [Tooltip("Seconds the left joystick must be pushed to complete the Move step.")]
        [SerializeField] float moveHoldSeconds = 0.8f;

        [Header("Flow")]
        [SerializeField] bool showOnStart = true;
        [SerializeField, Min(0f)] float startDelay = 1.5f;
        [SerializeField, Min(0.01f)] float fadeSeconds = 0.3f;
        [Tooltip("Seconds before the trigger can continue a text step (avoids skipping by accident).")]
        [SerializeField, Min(0f)] float minReadSeconds = 1f;
        [Tooltip("Holding the trigger this long skips the whole tutorial. A short pull only continues text steps.")]
        [SerializeField, Min(0.5f)] float skipHoldSeconds = 2f;
        [Tooltip("Pause on 'Done!' before the next step.")]
        [SerializeField, Min(0f)] float stepDoneDelay = 1f;

        [Header("Hints (bottom line)")]
        [SerializeField] string continueHint = "Pull the trigger to continue  ·  Hold it to skip the tutorial";
        [SerializeField] string tryItHint = "Try it now  ·  Hold the trigger to skip the tutorial";
        [Tooltip("{0} = seconds left.")]
        [SerializeField] string skippingHint = "Keep holding to skip the tutorial... {0}";
        [SerializeField] string doneHint = "Done!";

        [SerializeField] Step[] steps =
        {
            new Step { goal = StepGoal.ReadAndWait, title = "Welcome to Calm Woods",
                body = "Let's learn the controls. It only takes a minute." },
            new Step { goal = StepGoal.MoveStick, title = "Walk",
                body = "Push the LEFT joystick to walk around the forest." },
            new Step { goal = StepGoal.TurnStick, title = "Turn",
                body = "Push the RIGHT joystick left or right to turn." },
            new Step { goal = StepGoal.OpenAndCloseBreathing, title = "Breathing guide",
                body = "Press B on the right controller to open the breathing guide.\n" +
                       "Rest your left controller on your chest and follow the wheel.\n" +
                       "Press B again to close it." },
            new Step { goal = StepGoal.OpenAndCloseOptions, title = "Options",
                body = "Press Y on the left controller to open the options:\n" +
                       "time of day, music and ambient sound.\n" +
                       "Press Y again to close them." },
            new Step { goal = StepGoal.ReadAndWait, title = "You're ready",
                body = "Breathe slowly and watch the sky clear up. Enjoy the forest." }
        };

        public bool IsRunning { get; private set; }
        public event Action Finished;

        int index;
        float stepTimer;
        float holdTimer;
        bool holdStartedVisible;
        bool sawGoalMode;
        bool stepDone;
        bool hiddenByMode;
        Coroutine fade;

        void Awake()
        {
            if (head == null && Camera.main != null) head = Camera.main.transform;
            SetVisible(false, instant: true);
        }

        static InputAction TriggerAction()
        {
            var action = new InputAction("Continue", InputActionType.Button, "<XRController>{RightHand}/{TriggerButton}");
            action.AddBinding("<XRController>{LeftHand}/{TriggerButton}");
            return action;
        }

        void OnEnable()
        {
            moveInput.Enable();
            turnInput.Enable();
            continueInput.Enable();
        }

        void OnDisable()
        {
            moveInput.Disable();
            turnInput.Disable();
            continueInput.Disable();
        }

        IEnumerator Start()
        {
            if (!showOnStart) yield break;
            yield return new WaitForSecondsRealtime(startDelay);
            Begin();
        }

        // Can also be wired to a "Help" button in the options menu.
        public void Begin()
        {
            if (steps == null || steps.Length == 0) return;
            IsRunning = true;
            index = 0;
            hiddenByMode = false;
            ShowStep();
            PlaceInFrontOfHead();
            SetVisible(true, instant: false);
        }

        public void Stop()
        {
            IsRunning = false;
            SetVisible(false, instant: false);
        }

        void Update()
        {
            if (!IsRunning) return;

            AppMode mode = modeSwitcher != null ? modeSwitcher.CurrentMode : AppMode.Exploration;
            Step step = steps[index];

            // Hide while breathing / options are open, so the panels never overlap.
            if (mode != AppMode.Exploration && !hiddenByMode)
            {
                hiddenByMode = true;
                SetVisible(false, instant: false);
            }
            else if (mode == AppMode.Exploration && hiddenByMode)
            {
                hiddenByMode = false;
                PlaceInFrontOfHead();
                SetVisible(true, instant: false);
            }

            if (UpdateSkipHold(step)) return;
            if (stepDone) return;

            switch (step.goal)
            {
                case StepGoal.ReadAndWait:
                    // Text steps continue on a SHORT pull (on release), once the panel has been visible a moment.
                    if (!hiddenByMode) stepTimer += Time.unscaledDeltaTime;
                    if (!hiddenByMode && stepTimer >= minReadSeconds && continueInput.WasReleasedThisFrame()
                        && holdStartedVisible && lastHoldSeconds < skipHoldSeconds)
                    {
                        NextStep();
                    }
                    break;

                case StepGoal.MoveStick:
                    if (moveInput.ReadValue<Vector2>().magnitude > 0.5f) stepTimer += Time.unscaledDeltaTime;
                    if (stepTimer >= moveHoldSeconds) CompleteStep();
                    break;

                case StepGoal.TurnStick:
                    if (Mathf.Abs(turnInput.ReadValue<Vector2>().x) > 0.6f) CompleteStep();
                    break;

                case StepGoal.OpenAndCloseBreathing:
                    TrackOpenAndClose(mode, AppMode.Feedback);
                    break;

                case StepGoal.OpenAndCloseOptions:
                    TrackOpenAndClose(mode, AppMode.Options);
                    break;
            }
        }

        float lastHoldSeconds;

        // Holding the trigger skips the whole tutorial. Returns true if the tutorial was skipped.
        bool UpdateSkipHold(Step step)
        {
            if (continueInput.WasPressedThisFrame())
            {
                holdTimer = 0f;
                holdStartedVisible = !hiddenByMode;
            }

            if (continueInput.IsPressed() && holdStartedVisible && !hiddenByMode)
            {
                holdTimer += Time.unscaledDeltaTime;

                // Short pulls look normal; after a moment, show the countdown.
                if (hintLabel != null && !stepDone && holdTimer > 0.4f)
                    hintLabel.text = string.Format(skippingHint, Mathf.CeilToInt(skipHoldSeconds - holdTimer));

                if (holdTimer >= skipHoldSeconds)
                {
                    Skip();
                    return true;
                }
            }
            else if (continueInput.WasReleasedThisFrame())
            {
                lastHoldSeconds = holdTimer;
                holdTimer = 0f;
                // Released before skipping: put the normal hint back.
                if (hintLabel != null && !stepDone) hintLabel.text = HintFor(step);
            }

            return false;
        }

        // Ends the tutorial right away.
        public void Skip()
        {
            if (!IsRunning) return;
            StopAllCoroutines();
            fade = null;
            IsRunning = false;
            SetVisible(false, instant: false);
            Finished?.Invoke();
        }

        string HintFor(Step step) => step.goal == StepGoal.ReadAndWait ? continueHint : tryItHint;

        // Completes once the user has opened the target mode and come back to exploration.
        void TrackOpenAndClose(AppMode mode, AppMode target)
        {
            if (mode == target) sawGoalMode = true;
            else if (mode == AppMode.Exploration && sawGoalMode) CompleteStep();
        }

        // An action was done: show "Done!" for a moment, then move on.
        void CompleteStep()
        {
            stepDone = true;
            if (hintLabel != null) hintLabel.text = doneHint;
            StartCoroutine(NextStepAfterDelay());
        }

        IEnumerator NextStepAfterDelay()
        {
            yield return new WaitForSecondsRealtime(stepDoneDelay);
            NextStep();
        }

        void NextStep()
        {
            index++;
            if (index >= steps.Length)
            {
                IsRunning = false;
                SetVisible(false, instant: false);
                Finished?.Invoke();
                return;
            }

            ShowStep();
        }

        void ShowStep()
        {
            Step step = steps[index];
            stepDone = false;
            sawGoalMode = false;
            stepTimer = 0f;

            if (titleLabel != null) titleLabel.text = step.title;
            if (bodyLabel != null) bodyLabel.text = step.body;
            if (progressLabel != null) progressLabel.text = $"{index + 1} / {steps.Length}";
            if (hintLabel != null) hintLabel.text = HintFor(step);

            if (iconImage != null)
            {
                iconImage.sprite = step.icon;
                iconImage.gameObject.SetActive(step.icon != null);
            }
        }

        void SetVisible(bool visible, bool instant)
        {
            if (panel == null || canvasGroup == null) return;
            if (fade != null) StopCoroutine(fade);

            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;

            if (instant)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                panel.SetActive(visible);
                return;
            }

            panel.SetActive(true);
            fade = StartCoroutine(Fade(visible ? 1f : 0f, deactivateAtEnd: !visible));
        }

        IEnumerator Fade(float target, bool deactivateAtEnd)
        {
            float start = canvasGroup.alpha;
            for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeSeconds);
                yield return null;
            }
            canvasGroup.alpha = target;
            if (deactivateAtEnd) panel.SetActive(false);
            fade = null;
        }

        void PlaceInFrontOfHead()
        {
            if (head == null || panel == null) return;

            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
            forward.Normalize();

            Vector3 position = head.position + forward * distance;
            position.y = head.position.y + heightOffset;

            panel.transform.position = position;
            panel.transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class MachineSequenceController : MonoBehaviour
{
    [Header("External References")]
    public TweenController tweenController; // Reference to the TweenController for managing animations

    [Header("Sequence Data")]
    public List<OperationStep> manualSteps; // List of manual operation steps in the sequence
    public List<OperationStep> autoSteps; // List of automated operation steps in the sequence
    public List<OperationStep> currentSequence; // Currently active sequence (manual or automated)

    private int currentStepIndex = 1; // Tracks the current step index in the sequence
    private OperationStep currentStep => manualSteps.Count > 0 ? manualSteps[currentStepIndex - 1] : null; // Current step data

    private List<SelectableObject> activeHighlights = new List<SelectableObject>(); // List of currently highlighted objects

    public int CurrentStepIndex => currentStepIndex; // Public getter for the current step index
    public int TotalManualSteps => manualSteps.Count; // Total number of steps in the manual sequence
    public int TotalAutoSteps => autoSteps.Count; // Total number of steps in the automated sequence
    public OperationStep CurrentStep => currentStep; // Public getter for the current step data

    private void OnEnable()
    {
        EventBus.OnOperationStepChanged += OnOperationStepChanged; // Subscribe to step change events
        EventBus.OnUIPageChanged += OnUIPageChanged; // Subscribe to UI page change events
    }

    private void OnDisable()
    {
        EventBus.OnOperationStepChanged -= OnOperationStepChanged; // Unsubscribe from step change events
        EventBus.OnUIPageChanged -= OnUIPageChanged; // Unsubscribe from UI page change events
    }

    private void OnUIPageChanged(UIManager.UIPage pageName)
    {
        if (pageName == UIManager.UIPage.OperationView)
        return;

        tweenController.ResetAllToOrigin();
        ClearActiveHighlights();
    }

    public void RequestNextStep()
    {
        if (currentStepIndex < manualSteps.Count)
        {
            currentStepIndex++;
            UpdateStepIndex(currentStepIndex);
        }
    }

    public void RequestPreviousStep()
    {
        if (currentStepIndex > 1)
        {
            currentStepIndex--;
            UpdateStepIndex(currentStepIndex);
        }
    }

    private void UpdateStepIndex(int stepIndex)
    {
        EventBus.PublishOperationStepChanged(stepIndex);
    }

    private void OnOperationStepChanged(int stepIndex)
    {
        tweenController.PlayStepTweens(currentStep.stepAnimationList); // Play the animations associated with the new step
        UpdateHighlightObjects(); // Update the highlighted objects for the new step
    }

    private void UpdateHighlightObjects()
    {
        ClearActiveHighlights(); // Clear any currently active highlights

        if (currentStep.highlightObjects != null)
        {
            foreach (SelectableObject obj in currentStep.highlightObjects)
            {
                obj.Select(); // Highlight the object
                activeHighlights.Add(obj); // Add to the list of active highlights
            }
        }
    }

    private void ClearActiveHighlights()
    {
        foreach (SelectableObject obj in activeHighlights)
        {
            obj.Deselect(); // Deselect each currently highlighted object
        }
        activeHighlights.Clear(); // Clear the list of highlighted objects
    }
}

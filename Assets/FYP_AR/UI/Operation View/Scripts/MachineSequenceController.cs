using System.Collections.Generic;
using UnityEngine;

public class MachineSequenceController : MonoBehaviour
{
    [Header("External References")]
    public TweenController tweenController;

    [Header("Sequence Data")]
    public List<OperationStep> manualSteps;
    public List<OperationStep> autoSteps;

    private List<OperationStep> currentSequence;
    private int currentStepIndex = 1;
    private List<SelectableObject> activeHighlights = new List<SelectableObject>();

    public OperationStep CurrentStep => 
        currentSequence != null && currentSequence.Count > 0 
            ? currentSequence[currentStepIndex - 1] 
            : null;

    public int CurrentStepIndex => currentStepIndex;
    public int TotalSteps => currentSequence?.Count ?? 0;

    void OnEnable()
    {
        EventBus.OnOperationStepChanged       += OnOperationStepChanged;
        EventBus.OnUIPageChanged              += OnUIPageChanged;
        EventBus.OnSequenceModeChangeRequested += OnSequenceModeChangeRequested;
    }

    void OnDisable()
    {
        EventBus.OnOperationStepChanged       -= OnOperationStepChanged;
        EventBus.OnUIPageChanged              -= OnUIPageChanged;
        EventBus.OnSequenceModeChangeRequested -= OnSequenceModeChangeRequested;
    }

    void Start()
    {
        // Load manual sequence by default
        LoadSequence(EventBus.SequenceMode.Manual);
    }

    // ================================================================
    //  SEQUENCE LOADING
    // ================================================================

    private void LoadSequence(EventBus.SequenceMode mode)
    {
        currentSequence = mode == EventBus.SequenceMode.Manual ? manualSteps : autoSteps;
        currentStepIndex = 1;
        ClearActiveHighlights();
        tweenController.ResetAllToOrigin();
    }

    // ================================================================
    //  STEP NAVIGATION
    // ================================================================

    public void RequestNextStep()
    {
        if (currentSequence == null) return;
        if (currentStepIndex < currentSequence.Count)
            EventBus.PublishOperationStepChanged(++currentStepIndex);
        
        Debug.Log("Requested Next");
    }

    public void RequestPreviousStep()
    {
        if (currentSequence == null) return;
        if (currentStepIndex > 1)
            EventBus.PublishOperationStepChanged(--currentStepIndex);
        
        Debug.Log("Requested Previous");
    }

    public void RequestCurrentStep(int stepIndex)
    {
        if (currentSequence == null) return;
        if (stepIndex >= 1 && stepIndex <= currentSequence.Count)
            EventBus.PublishOperationStepChanged(stepIndex);
        
        Debug.Log("Requested Current");
    }

    // ================================================================
    //  EVENT HANDLERS
    // ================================================================

    private void OnOperationStepChanged(int stepIndex)
    {
        currentStepIndex = stepIndex;
        tweenController.PlayStepTweens(CurrentStep.stepAnimationList);
        UpdateHighlightObjects();
    }

    private void OnSequenceModeChangeRequested(EventBus.SequenceMode mode)
    {
        LoadSequence(mode);
    }

    private void OnUIPageChanged(UIManager.UIPage page)
    {
        if (page == UIManager.UIPage.OperationView) return;
        tweenController.ResetAllToOrigin();
        ClearActiveHighlights();
    }

    // ================================================================
    //  HIGHLIGHTS
    // ================================================================

    private void UpdateHighlightObjects()
    {
        ClearActiveHighlights();
        if (CurrentStep?.highlightObjects == null) return;

        foreach (var obj in CurrentStep.highlightObjects)
        {
            obj.Select();
            activeHighlights.Add(obj);
        }
    }

    private void ClearActiveHighlights()
    {
        foreach (var obj in activeHighlights)
            obj.Deselect();
        activeHighlights.Clear();
    }
}
using UnityEngine;
using UnityEngine.UIElements;

public class OperationView : MonoBehaviour
{
    // UI Elements
    private UIDocument uiDocument;
    private MachineSequenceController activeSequenceController;

    private int currentStepIndex = 1;
    private int maxStepCount = 1;

    private Button nextStepBtn;
    private Button prevStepBtn;
    private TextField stepInputField;
    private Label maxStepCountLabel;
    private Label instructionDescriptionLabel;

    void Start()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError("OperationView requires a UIDocument reference or a UIDocument component on the same GameObject.");
            return;
        }

        #region Subscribe to Events
        EventBus.OnARObjectSpawned += (spawnedObject) =>
        {
            var sequenceController = spawnedObject.GetComponent<MachineSequenceController>();
            if (sequenceController != null)
            {
                UpdateActiveSequenceController(sequenceController);
                Debug.Log($"OperationView: Detected new AR object with MachineSequenceController. Updated active sequence controller.");
            }
        };

        EventBus.OnOperationStepChanged += HandleOperationStepChanged;
        #endregion

        var root = uiDocument.rootVisualElement;
        nextStepBtn = root.Q<Button>("nextStepBtn");
        prevStepBtn = root.Q<Button>("prevStepBtn");
        stepInputField = root.Q<TextField>("stepInput");
        maxStepCountLabel = root.Q<Label>("maxStepCountLabel");
        instructionDescriptionLabel = root.Q<Label>("instructionText");

        if (nextStepBtn != null)
        {
            nextStepBtn.clicked += OnNextStep;
        }

        if (prevStepBtn != null)
        {
            prevStepBtn.clicked += OnPrevStep;
        }
    }

    void UpdateActiveSequenceController(MachineSequenceController newController)
    {
        activeSequenceController = newController;
        if (activeSequenceController != null)
        {
            maxStepCount = activeSequenceController.TotalManualSteps;
            currentStepIndex = 1; // Reset to the first step when a new sequence is set
            UpdateStepUI();
        }
    }

    void OnNextStep()
    {
        if (activeSequenceController != null)
            activeSequenceController.RequestNextStep();

    }

    void OnPrevStep()
    {
        if (activeSequenceController != null)
            activeSequenceController.RequestPreviousStep();
    }

    void UpdateStepUI()
    {
        if (stepInputField != null)
        {
            stepInputField.value = currentStepIndex.ToString();
        }

        if (maxStepCountLabel != null)
        {
            maxStepCountLabel.text = $"/ {maxStepCount}";
        }

        if (instructionDescriptionLabel != null)
        {
            instructionDescriptionLabel.text = activeSequenceController.CurrentStep.instructionText;
        }
    }

    #region Event Handlers
    private void HandleOperationStepChanged(int stepIndex)
    {
        currentStepIndex = stepIndex;
        UpdateStepUI();
    }
    #endregion
}

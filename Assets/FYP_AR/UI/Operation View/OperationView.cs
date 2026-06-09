using UnityEngine;
using UnityEngine.UIElements;

public class OperationView : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private MachineSequenceController activeSequenceController;

    private EventBus.SequenceMode currentMode = EventBus.SequenceMode.Manual;
    private bool isHelpVisible = false;

    // Top bar
    private Button backButton;
    private Button helpButton;

    // Help panel
    private VisualElement helpPanel;
    private Button helpCloseButton;

    // Operation box
    private Button sequenceToggleBtn;
    private Label modeTitle;
    private Button nextStepBtn;
    private Button prevStepBtn;
    private TextField stepInputField;
    private Label maxStepCountLabel;
    private Label instructionText;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("OperationView: UIDocument not found.");
            return;
        }
    }

    void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        // Top bar
        backButton  = root.Q<Button>("backButton");
        helpButton  = root.Q<Button>("helpButton");

        // Help panel
        helpPanel       = root.Q<VisualElement>("helpPanel");
        helpCloseButton = root.Q<Button>("helpCloseButton");

        // Operation box
        sequenceToggleBtn  = root.Q<Button>("sequenceToggleBtn");
        modeTitle          = root.Q<Label>("modeTitle");
        nextStepBtn        = root.Q<Button>("nextStepBtn");
        prevStepBtn        = root.Q<Button>("prevStepBtn");
        stepInputField     = root.Q<TextField>("stepInput");
        maxStepCountLabel  = root.Q<Label>("maxStepCountLabel");
        instructionText    = root.Q<Label>("instructionText");

        if (backButton != null)        backButton.clicked        += OnBackPressed;
        if (helpButton != null)        helpButton.clicked        += OnHelpPressed;
        if (helpCloseButton != null)   helpCloseButton.clicked   += OnHelpClosePressed;
        if (sequenceToggleBtn != null) sequenceToggleBtn.clicked += OnSequenceTogglePressed;
        if (nextStepBtn != null)       nextStepBtn.clicked       += OnNextStep;
        if (prevStepBtn != null)       prevStepBtn.clicked       += OnPrevStep;

        EventBus.OnARObjectSpawned      += OnARObjectSpawned;
        EventBus.OnOperationStepChanged += OnOperationStepChanged;
        EventBus.OnUIPageChanged        += OnUIPageChanged;

        // Pointer event for closing help panel when clicking outside
        root.RegisterCallback<PointerDownEvent>(OnRootPointerDown);

        SetHelpVisible(false);
    }

    void OnDisable()
    {
        if (backButton != null)        backButton.clicked        -= OnBackPressed;
        if (helpButton != null)        helpButton.clicked        -= OnHelpPressed;
        if (helpCloseButton != null)   helpCloseButton.clicked   -= OnHelpClosePressed;
        if (sequenceToggleBtn != null) sequenceToggleBtn.clicked -= OnSequenceTogglePressed;
        if (nextStepBtn != null)       nextStepBtn.clicked       -= OnNextStep;
        if (prevStepBtn != null)       prevStepBtn.clicked       -= OnPrevStep;

        EventBus.OnARObjectSpawned      -= OnARObjectSpawned;
        EventBus.OnOperationStepChanged -= OnOperationStepChanged;
        EventBus.OnUIPageChanged        -= OnUIPageChanged;

        if (root != null)
            root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown);
    }

    // ================================================================
    //  EVENT HANDLERS
    // ================================================================

    private void OnARObjectSpawned(GameObject spawnedObject)
    {
        var controller = spawnedObject.GetComponent<MachineSequenceController>();
        if (controller == null) return;

        activeSequenceController = controller;
        currentMode = EventBus.SequenceMode.Manual;
    }

    private void OnUIPageChanged(UIManager.UIPage page)
    {
        // Every time Operation View becomes the active page,
        // refresh UI from whatever the controller's current state is
        if (page != UIManager.UIPage.OperationView) return;
        if (activeSequenceController == null) return;
        Debug.Log("Here");
        activeSequenceController.RequestCurrentStep(activeSequenceController.CurrentStepIndex);

        UpdateModeUI();
        UpdateStepUI();
    }

    private void OnOperationStepChanged(int stepIndex)
    {
        UpdateStepUI();
    }

    // ================================================================
    //  BUTTON HANDLERS
    // ================================================================

    private void OnBackPressed()  => EventBus.PublishBackButtonClicked();
    private void OnHelpPressed()  => SetHelpVisible(!isHelpVisible);
    private void OnHelpClosePressed() => SetHelpVisible(false);
    private void OnNextStep()     => activeSequenceController?.RequestNextStep();
    private void OnPrevStep()     => activeSequenceController?.RequestPreviousStep();

    private void OnSequenceTogglePressed()
    {
        currentMode = currentMode == EventBus.SequenceMode.Manual ? EventBus.SequenceMode.Auto : EventBus.SequenceMode.Manual;
        EventBus.PublishSequenceModeChangeRequested(currentMode);
        UpdateModeUI();
        UpdateStepUI();
    }

    // ================================================================
    //  UI UPDATES
    // ================================================================

    private void SetHelpVisible(bool visible)
    {
        isHelpVisible = visible;
        if (helpPanel != null)
            helpPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateModeUI()
    {
        bool isManual = currentMode == EventBus.SequenceMode.Manual;

        if (modeTitle != null)
            modeTitle.text = isManual ? "Manual Mode" : "Auto Mode";

        if (sequenceToggleBtn != null)
            sequenceToggleBtn.text = isManual ? "Switch to Auto" : "Switch to Manual";
    }

    private void UpdateStepUI()
    {
        if (activeSequenceController == null) return;

        if (stepInputField != null)
            stepInputField.value = activeSequenceController.CurrentStepIndex.ToString();

        if (maxStepCountLabel != null)
            maxStepCountLabel.text = $"/ {activeSequenceController.TotalSteps}";

        if (instructionText != null && activeSequenceController.CurrentStep != null)
            instructionText.text = activeSequenceController.CurrentStep.instructionText;
    }

    // ================================================================
    //  POINTER EVENT HANDLER
    // ================================================================

    private void OnRootPointerDown(PointerDownEvent evt)
    {
        if (!isHelpVisible || helpPanel == null) return;

        var target = evt.target as VisualElement;
        if (target == null || !helpPanel.Contains(target))
            SetHelpVisible(false);
    }
}
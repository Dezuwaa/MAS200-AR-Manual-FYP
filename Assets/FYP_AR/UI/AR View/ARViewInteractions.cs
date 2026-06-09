using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ARViewInteractions : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;

    // External References
    private MachineContext currentMachineContext;

    // Top-bar buttons
    private Button backButton;
    private Button helpButton;
    private Button operationManual;
    private Button toggleLabel;
    private Button toggleOverlay;
    private Button componentDetail;
    private Button pdfButton;

    // Help Panel
    private VisualElement helpPanel;
    private Button helpCloseButton;
    private bool isHelpPanelVisible = false;

    // Component Detail Panel elements
    private VisualElement componentDetailPanel;
    private Button panelListButton;
    private Button panelCloseButton;
    private Label panelTitle;
    private ScrollView componentDetailScroll;
    private ScrollView componentListScroll;
    private Label componentDetailText;
    private VisualElement componentListContainer;
    private Button componentButtonTemplate;

    // Panel state
    private bool isPanelVisible = false;
    private bool isListViewActive = true;

    // ---------------------------------------------------------------

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component not found on ARViewInteractions GameObject");
            return;
        }
        root = uiDocument.rootVisualElement;
    }

    void OnEnable()
    {
        if (root == null) return;

        // Top-bar buttons
        backButton      = root.Q<Button>("backButton");
        helpButton      = root.Q<Button>("helpButton");
        operationManual = root.Q<Button>("operationManual");
        toggleLabel     = root.Q<Button>("toggleLabel");
        toggleOverlay   = root.Q<Button>("toggleOverlay");
        componentDetail = root.Q<Button>("componentDetail");
        pdfButton       = root.Q<Button>("pdfButton");

        // Help panel
        helpPanel        = root.Q<VisualElement>("helpPanel");
        helpCloseButton  = root.Q<Button>("helpCloseButton");

        // Component detail panel elements
        componentDetailPanel    = root.Q<VisualElement>("componentDetailPanel");
        panelListButton         = root.Q<Button>("panelListButton");
        panelCloseButton        = root.Q<Button>("panelCloseButton");
        panelTitle              = root.Q<Label>("panelTitle");
        componentDetailScroll   = root.Q<ScrollView>("componentDetailScroll");
        componentListScroll     = root.Q<ScrollView>("componentListScroll");
        componentDetailText     = root.Q<Label>("componentDetailText");
        componentListContainer  = root.Q<VisualElement>("componentListContainer");
        componentButtonTemplate = root.Q<Button>("componentButtonTemplate");

        // Button callbacks
        if (backButton != null)       backButton.clicked       += OnBackButtonPressed;
        if (helpButton != null)       helpButton.clicked       += OnHelpButtonPressed;
        if (helpCloseButton != null)  helpCloseButton.clicked  += OnHelpCloseButtonPressed;
        if (operationManual != null)  operationManual.clicked  += OnOperationManualPressed;
        if (toggleLabel != null)      toggleLabel.clicked      += OnToggleLabelPressed;
        if (toggleOverlay != null)    toggleOverlay.clicked    += OnToggleOverlayPressed;
        if (componentDetail != null)  componentDetail.clicked  += OnComponentDetailPressed;
        if (panelListButton != null)  panelListButton.clicked  += OnPanelListButtonPressed;
        if (panelCloseButton != null) panelCloseButton.clicked += OnPanelCloseButtonPressed;
        if (pdfButton != null)        pdfButton.clicked        += OnPdfButtonPressed;

        // EventBus subscriptions
        EventBus.OnMachineContextAvailable += OnMachineContextAvailable;
        EventBus.OnSelectedObjectChanged   += OnSelectedObjectChanged;

        // Pointer event for closing help panel when clicking outside
        root.RegisterCallback<PointerDownEvent>(OnRootPointerDown);

        SetPanelVisible(false);
        SetHelpPanelVisible(false);
    }

    void OnDisable()
    {
        if (backButton != null)       backButton.clicked       -= OnBackButtonPressed;
        if (helpButton != null)       helpButton.clicked       -= OnHelpButtonPressed;
        if (helpCloseButton != null)  helpCloseButton.clicked  -= OnHelpCloseButtonPressed;
        if (operationManual != null)  operationManual.clicked  -= OnOperationManualPressed;
        if (toggleLabel != null)      toggleLabel.clicked      -= OnToggleLabelPressed;
        if (toggleOverlay != null)    toggleOverlay.clicked    -= OnToggleOverlayPressed;
        if (componentDetail != null)  componentDetail.clicked  -= OnComponentDetailPressed;
        if (panelListButton != null)  panelListButton.clicked  -= OnPanelListButtonPressed;
        if (panelCloseButton != null) panelCloseButton.clicked -= OnPanelCloseButtonPressed;
        if (pdfButton != null)        pdfButton.clicked        -= OnPdfButtonPressed;

        EventBus.OnMachineContextAvailable -= OnMachineContextAvailable;
        EventBus.OnSelectedObjectChanged   -= OnSelectedObjectChanged;

        if (root != null)
            root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown);
    }

    // ================================================================
    //  EVENTBUS HANDLERS
    // ================================================================

    private void OnMachineContextAvailable(MachineContext context)
    {
        currentMachineContext = context;

        var machineLabel = root.Q<Label>("machineName");
        if (machineLabel != null)
            machineLabel.text = context.machineName;

        RebuildComponentListUI(context.GetComponents());
    }

    private void OnSelectedObjectChanged(GameObject selectedGO)
    {
        if (selectedGO == null)
        {
            ShowListView();
            return;
        }

        if (currentMachineContext == null) return;

        var entry = currentMachineContext.GetComponents()
            .Find(e => e.componentObject == selectedGO);

        if (entry.data != null)
        {
            ShowDetailView(entry);
            SetPanelVisible(true);
        }
        else
            ShowListView();
    }

    // ================================================================
    //  HELP PANEL
    // ================================================================

    private void SetHelpPanelVisible(bool visible)
    {
        isHelpPanelVisible = visible;
        if (helpPanel != null)
            helpPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ================================================================
    //  COMPONENT PANEL VISIBILITY & VIEW SWITCHING
    // ================================================================

    private void SetPanelVisible(bool visible)
    {
        isPanelVisible = visible;
        if (componentDetailPanel != null)
            componentDetailPanel.style.display = visible
                ? DisplayStyle.Flex
                : DisplayStyle.None;
    }

    private void ShowDetailView(MachineContext.ComponentEntry entry)
    {
        isListViewActive = false;

        if (panelTitle != null)
            panelTitle.text = entry.data.componentName;

        if (componentDetailText != null)
            componentDetailText.text =
                $"{entry.data.description}\n";

        if (componentDetailScroll != null)
            componentDetailScroll.style.display = DisplayStyle.Flex;
        if (componentListScroll != null)
            componentListScroll.style.display = DisplayStyle.None;
    }

    private void ShowListView()
    {
        isListViewActive = true;

        if (panelTitle != null)
            panelTitle.text = "Component List";

        if (componentDetailScroll != null)
            componentDetailScroll.style.display = DisplayStyle.None;
        if (componentListScroll != null)
            componentListScroll.style.display = DisplayStyle.Flex;
    }

    // ================================================================
    //  COMPONENT LIST BUILDER
    // ================================================================

    private void RebuildComponentListUI(List<MachineContext.ComponentEntry> entries)
    {
        if (componentListContainer == null || componentButtonTemplate == null) return;

        var toRemove = new List<VisualElement>();
        foreach (var child in componentListContainer.Children())
        {
            if (child != componentButtonTemplate)
                toRemove.Add(child);
        }
        foreach (var child in toRemove)
            componentListContainer.Remove(child);

        foreach (var entry in entries)
        {
            var captured = entry;
            var btn = new Button(() => EventBus.PublishComponentSelected(captured.componentObject));
            btn.text = captured.data.componentName;
            btn.AddToClassList("ar-list-item");
            componentListContainer.Add(btn);
        }
    }

    // ================================================================
    //  BUTTON HANDLERS
    // ================================================================

    private void OnBackButtonPressed()      => EventBus.PublishBackButtonClicked();
    private void OnOperationManualPressed() => EventBus.PublishUIPageChangeRequested(UIManager.UIPage.OperationView);
    private void OnToggleLabelPressed()
    {
        if (currentMachineContext.GetVisibility())
            EventBus.PublishToggleLabelButtonClicked();
    }
    private void OnToggleOverlayPressed()   => EventBus.PublishToggleOverlayButtonClicked();

    private void OnHelpButtonPressed()      => SetHelpPanelVisible(!isHelpPanelVisible);
    private void OnHelpCloseButtonPressed() => SetHelpPanelVisible(false);

    private void OnComponentDetailPressed() => SetPanelVisible(!isPanelVisible);
    private void OnPanelCloseButtonPressed() => SetPanelVisible(false);

    private void OnPanelListButtonPressed()
    {
        if (!isListViewActive)
            ShowListView();
    }

    private void OnPdfButtonPressed()       => EventBus.PublishPdfButtonClicked();

    // ================================================================
    //  POINTER EVENT HANDLER
    // ================================================================

    private void OnRootPointerDown(PointerDownEvent evt)
    {
        if (!isHelpPanelVisible || helpPanel == null) return;

        var target = evt.target as VisualElement;
        if (target == null || !helpPanel.Contains(target))
            SetHelpPanelVisible(false);
    }
}
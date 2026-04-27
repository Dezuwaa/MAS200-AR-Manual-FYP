using UnityEngine;
using UnityEngine.UIElements;

public class ARViewInteractions : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;

    // Button references
    private Button backButton;
    private Button helpButton;
    private Button operationManual;
    private Button indexSearch;
    private Button toggleLabel;
    private Button toggleOverlay;

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

        // Get button references
        backButton = root.Q<Button>("backButton");
        helpButton = root.Q<Button>("helpButton");
        operationManual = root.Q<Button>("operationManual");
        indexSearch = root.Q<Button>("indexSearch");
        toggleLabel = root.Q<Button>("toggleLabel");
        toggleOverlay = root.Q<Button>("toggleOverlay");

        // Register button callbacks
        if (backButton != null)
            backButton.clicked += OnBackButtonPressed;

        if (operationManual != null)
            operationManual.clicked += OnOperationManualPressed;

        if (indexSearch != null)
            indexSearch.clicked += OnIndexSearchPressed;

        if (toggleLabel != null)
            toggleLabel.clicked += OnToggleLabelPressed;

        if (toggleOverlay != null)
            toggleOverlay.clicked += OnToggleOverlayPressed;

        if (helpButton != null)
            helpButton.clicked += OnHelpButtonPressed;
    }

    void OnDisable()
    {
        // Unregister button callbacks
        if (backButton != null)
            backButton.clicked -= OnBackButtonPressed;

        if (operationManual != null)
            operationManual.clicked -= OnOperationManualPressed;

        if (indexSearch != null)
            indexSearch.clicked -= OnIndexSearchPressed;

        if (toggleLabel != null)
            toggleLabel.clicked -= OnToggleLabelPressed;

        if (toggleOverlay != null)
            toggleOverlay.clicked -= OnToggleOverlayPressed;

        if (helpButton != null)
            helpButton.clicked -= OnHelpButtonPressed;
    }

    /// <summary>
    /// Handles the back button press - returns to previous UI page
    /// </summary>
    private void OnBackButtonPressed()
    {
        EventBus.PublishBackButtonClicked();
        // Leave back navigation behavior empty here for now.
    }

    /// <summary>
    /// Handles the Operation Manual button press - switches to OperationView UI
    /// </summary>
    private void OnOperationManualPressed()
    {
        EventBus.PublishUIPageChangeRequested(UIManager.UIPage.OperationView);
    }

    /// <summary>
    /// Handles the Index Search button press - TODO: implement index search panel
    /// Future: Pop out a panel with a list of available components
    /// When tapped, highlights the component in AR view with info panel
    /// </summary>
    private void OnIndexSearchPressed()
    {
        EventBus.PublishIndexSearchButtonClicked();
        Debug.Log("Index Search button pressed - Feature coming soon");
        // TODO: Implement index search functionality
    }

    /// <summary>
    /// Handles the Toggle Label button press - TODO: implement label visibility toggle
    /// Future: Toggles visibility of worldspace UI panels showing name, PLC name, PLC address
    /// of 3D objects in the AR view
    /// </summary>
    private void OnToggleLabelPressed()
    {
        EventBus.PublishToggleLabelButtonClicked();
        Debug.Log("Toggle Label button pressed - Feature coming soon");
        // TODO: Implement label visibility toggle functionality
    }

    /// <summary>
    /// Handles the Toggle Overlay button press - TODO: implement overlay controls
    /// Future: Pop out minimalistic controls for adjusting model opacity and visibility
    /// (requires tags on models for filtering)
    /// </summary>
    private void OnToggleOverlayPressed()
    {
        EventBus.PublishToggleOverlayButtonClicked();
        Debug.Log("Toggle Overlay button pressed - Feature coming soon");
        // TODO: Implement overlay controls functionality
    }

    /// <summary>
    /// Handles the Help button press - TODO: implement help functionality
    /// </summary>
    private void OnHelpButtonPressed()
    {
        EventBus.PublishHelpButtonClicked();
        Debug.Log("Help button pressed");
        // TODO: Implement help functionality
    }
}

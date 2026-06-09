using UnityEngine;
using UnityEngine.InputSystem;

public class ARTapInteraction : MonoBehaviour
{
    public Camera arCamera;
    private ARControls controls;
    private GameObject selectedObject = null;
    [SerializeField] private UIManager uIManager;

    void Awake()
    {
        controls = new ARControls();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Touch.Tap.performed += OnTap;
        EventBus.OnUIPageChanged     += OnUIPageChanged;
        EventBus.OnComponentSelected += OnComponentSelectedFromPanel;
    }

    void OnDisable()
    {
        controls.Touch.Tap.performed -= OnTap;
        EventBus.OnUIPageChanged     -= OnUIPageChanged;
        EventBus.OnComponentSelected -= OnComponentSelectedFromPanel;
        controls.Disable();
    }

    // ================================================================
    //  SELECTION CORE — single place that owns selectedObject mutation
    // ================================================================

    private void SetSelectedObject(GameObject incoming)
    {
        if (selectedObject == incoming)
        {
            // Same object tapped/pressed again → deselect
            selectedObject.GetComponent<SelectableObject>()?.Deselect();
            selectedObject = null;
        }
        else
        {
            // Deselect previous
            selectedObject?.GetComponent<SelectableObject>()?.Deselect();

            // Select new
            selectedObject = incoming;
            selectedObject?.GetComponent<SelectableObject>()?.Select();
        }

        // Notify all listeners (ARViewInteractions updates the panel)
        EventBus.PublishSelectedObjectChanged(selectedObject);
    }

    // ================================================================
    //  INPUT HANDLERS
    // ================================================================

    private void OnTap(InputAction.CallbackContext context)
    {
        if (uIManager.currentPage != UIManager.UIPage.ARView) return;
        
        Vector2 position = Pointer.current.position.ReadValue();
        Ray ray = arCamera.ScreenPointToRay(position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject tappedObject = hit.collider.gameObject;

            if (tappedObject.GetComponent<SelectableObject>() != null)
                SetSelectedObject(tappedObject);
        }
    }

    /// <summary>
    /// Received when the user presses a component button in the panel list.
    /// Feeds into the same selection pipeline as a physical tap.
    /// </summary>
    private void OnComponentSelectedFromPanel(GameObject go)
    {
        SetSelectedObject(go);
    }

    private void OnUIPageChanged(UIManager.UIPage pageName)
    {
        if (pageName == UIManager.UIPage.ARView) return;
        DeselectAll();
    }

    private void DeselectAll()
    {
        if (selectedObject == null) return;

        selectedObject.GetComponent<SelectableObject>()?.Deselect();
        selectedObject = null;

        EventBus.PublishSelectedObjectChanged(null);
    }
}
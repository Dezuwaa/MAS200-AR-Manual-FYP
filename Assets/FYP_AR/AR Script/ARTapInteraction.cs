using UnityEngine;
using UnityEngine.InputSystem;


public class ARTapInteraction : MonoBehaviour
{
    public Camera arCamera;
    private ARControls controls;
    private GameObject selectedObject = null;

    void Awake()
    {
        controls = new ARControls();
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Touch.Tap.performed += OnTap;
        EventBus.OnUIPageChanged += OnUIPageChanged;
    }

    void OnDisable()
    {
        controls.Touch.Tap.performed -= OnTap;
        EventBus.OnUIPageChanged -= OnUIPageChanged;
        controls.Disable();
    }

    private void OnTap(InputAction.CallbackContext context)
    {
        Vector2 position = Pointer.current.position.ReadValue();
        Ray ray = arCamera.ScreenPointToRay(position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject tappedObject = hit.collider.gameObject;
            SelectableObject selectable = tappedObject.GetComponent<SelectableObject>(); // To get sekecteable object

            if (selectable != null)
            {
                if (selectedObject == null) // No object is selected
                {
                    selectedObject = selectable.gameObject;
                    selectedObject.GetComponent<SelectableObject>().Select();
                }
                else if (selectedObject != selectable.gameObject) // Object selected, Different object is tapped
                {
                    selectedObject.GetComponent<SelectableObject>().Deselect();
                    selectedObject = selectable.gameObject;
                    selectedObject.GetComponent<SelectableObject>().Select();
                }
                else // Object selected, Same object is tapped
                {
                    selectedObject.GetComponent<SelectableObject>().Deselect();
                    selectedObject = null;
                }   
            }
        }
    }

    private void OnUIPageChanged(UIManager.UIPage pageName)
    {
        if (pageName == UIManager.UIPage.ARView)
            return;

        DeselectAll();
    }

    private void DeselectAll()
    {
        if (selectedObject == null)
            return;
        
        selectedObject.GetComponent<SelectableObject>().Deselect();
        selectedObject = null;
    }
}

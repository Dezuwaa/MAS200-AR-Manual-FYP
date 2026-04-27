using UnityEngine;

public class AndroidBackHandler : MonoBehaviour
{
    private ARControls controls;

    public UIManager uiManager;

    void Awake()
    {
        controls = new ARControls();
    }

    void OnEnable()
    {
        controls.Enable();

        controls.Touch.Back.performed += OnBackPressed;
    }

    void OnDisable()
    {
        controls.Touch.Back.performed -= OnBackPressed;

        controls.Disable();
    }

    private void OnBackPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Debug.Log("Back Pressed (Input System)");

        uiManager.GoBack();
    }
}

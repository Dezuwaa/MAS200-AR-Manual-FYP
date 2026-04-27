using UnityEngine;
using TMPro;

public class ComponentLabel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject LabelPanel;

    [SerializeField] private TextMeshProUGUI componentNameText;

    [SerializeField] private TextMeshProUGUI componentDetailsText;

    [Header("Component Data")]
    [SerializeField]
    private ComponentData componentData;

    [Header("Look At Camera")]
    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    private bool smoothRotation = false;

    [SerializeField]
    private float rotationSpeed = 10f;

    private void Awake()
    {
        if (componentData == null)
        {
            componentData = GetComponent<ComponentData>();
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        EventBus.OnToggleLabelButtonClicked += ToggleLabelVisibility;
    }

    private void OnDisable()
    {
        EventBus.OnToggleLabelButtonClicked -= ToggleLabelVisibility;
    }

    private void Start()
    {
        UpdateLabel();
    }

    private void LateUpdate()
    {
        FaceCamera();
    }

    public void UpdateLabel()
    {
        if (componentData == null)
        {
            Debug.LogWarning($"ComponentData not found on '{gameObject.name}'. Attach ComponentData or assign it in the inspector.");
            return;
        }

        if (componentNameText != null)
        {
            componentNameText.text = componentData.componentName;
        }

        if (componentDetailsText != null)
        {
            componentDetailsText.text = GetFormattedDetails(componentData);
        }
    }

    private void FaceCamera()
    {
        if (cameraTransform == null)
        {
            return;
        }

        Vector3 direction = (transform.position - cameraTransform.position).normalized;
        if (direction.sqrMagnitude <= 0f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        if (smoothRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    private string GetFormattedDetails(ComponentData data)
    {
        var details = string.Empty;

        if (!string.IsNullOrEmpty(data.plcName))
        {
            details += $"PLC: {data.plcName}";
        }

        if (!string.IsNullOrEmpty(data.plcAddress))
        {
            if (details.Length > 0)
            {
                details += "\n";
            }

            details += $"Address: {data.plcAddress}";
        }

        return details;
    }

    private void ToggleLabelVisibility()
    {
        if (LabelPanel != null)
        {
            LabelPanel.SetActive(!LabelPanel.activeSelf);
        }
    }
}

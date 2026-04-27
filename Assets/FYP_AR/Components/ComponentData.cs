using UnityEngine;

[CreateAssetMenu(fileName = "ComponentData", menuName = "AR/AR Component Data")]
public class ComponentData : ScriptableObject
{    
    [Header("Component Details")]
    [Tooltip("The readable name of the component")]
    public string componentName;

    [Tooltip("The PLC label (e.g., Y1, Y2)")]
    public string plcName;

    [Tooltip("The PLC output/input address (e.g., 000, 100)")]
    public string plcAddress;
}
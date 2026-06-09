using UnityEngine;
using System.Collections.Generic;

public class MachineContext : MonoBehaviour
{
    [System.Serializable]
    public struct ComponentEntry
    {
        public GameObject componentObject;
        public ComponentData data;
    }

    [Header("Machine Identity")]
    public string machineName = "MAS200 Base Supply";

    [Header("Components")]
    [Tooltip("Assign each child GameObject and its matching SO here.")]
    public List<ComponentEntry> components = new List<ComponentEntry>();

    // Overlay state — starts visible
    private bool isOverlayVisible = true;

    public List<ComponentEntry> GetComponents() => components;
    public bool GetVisibility() => isOverlayVisible;

    void OnEnable()
    {
        EventBus.OnToggleOverlayButtonClicked += ToggleOverlay;
        EventBus.OnUIPageChanged += OnUIPageChanged;
    }

    void OnDisable()
    {
        EventBus.OnToggleOverlayButtonClicked -= ToggleOverlay;
    }

    void Start()
    {
        EventBus.PublishMachineContextAvailable(this);
    }

    private void OnUIPageChanged(UIManager.UIPage page)
    {
        isOverlayVisible = true;
        SetOverlayVisibility(isOverlayVisible);
    }

    // ================================================================
    //  OVERLAY TOGGLE
    // ================================================================

    private void ToggleOverlay()
    {
        isOverlayVisible = !isOverlayVisible;
        SetOverlayVisibility(isOverlayVisible);
    }

    /// <summary>
    /// Sets the first child of each component GameObject active or inactive.
    /// The first child is assumed to be the visual overlay mesh.
    /// </summary>
    private void SetOverlayVisibility(bool visible)
    {
        foreach (var entry in components)
        {
            if (entry.componentObject == null) continue;

            entry.componentObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Public getter so UI can reflect current overlay state if needed
    /// e.g. changing the button icon or tint.
    /// </summary>
    public bool IsOverlayVisible() => isOverlayVisible;
}
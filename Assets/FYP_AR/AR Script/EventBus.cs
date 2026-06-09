using System;
using UnityEngine;

public static class EventBus
{
    // Enum
    public enum SequenceMode { Manual, Auto }

    // UI Events
    public static event Action<GameObject> OnARObjectSpawned;
    public static event Action<UIManager.UIPage> OnUIPageChanged;
    public static event Action<UIManager.UIPage> OnUIPageChangeRequested;

    // User Interaction Events
    public static event Action OnBackButtonClicked;
    public static event Action OnIndexSearchButtonClicked;
    public static event Action OnToggleLabelButtonClicked;
    public static event Action OnToggleOverlayButtonClicked;
    public static event Action OnHelpButtonClicked;
    public static event Action OnPdfButtonClicked;

    // Operation Sequence Events
    public static event Action<int> OnOperationStepChanged;
    public static event Action<int> OnOperationGoToStepRequested;
    public static event Action<SequenceMode> OnSequenceModeChangeRequested;

    // AR Scanning Events
    public static event Action OnScanStarted;
    public static event Action OnScanSuccess;
    public static event Action OnScanEnded;
    public static event Action OnAnchorLocked;

    // Others
    public static event Action<MachineContext> OnMachineContextAvailable;
    public static event Action<GameObject> OnComponentSelected;
    public static event Action<GameObject> OnSelectedObjectChanged;

    #region AR Events
    public static void PublishARObjectSpawned(GameObject spawnedObject)
    {
        OnARObjectSpawned?.Invoke(spawnedObject);
    }

    public static void PublishScanStarted()
    {
        OnScanStarted?.Invoke();
    }

    public static void PublishScanSuccess()
    {
        OnScanSuccess?.Invoke();
    }

    public static void PublishScanEnded()
    {
        OnScanEnded?.Invoke();
    }

    public static void PublishAnchorLocked()
    {
        OnAnchorLocked?.Invoke();
    }

    public static void PublishMachineContextAvailable(MachineContext context)
    {
        OnMachineContextAvailable?.Invoke(context);
    }

    public static void PublishComponentSelected(GameObject go)
    {
        OnComponentSelected?.Invoke(go);
    }

    public static void PublishSelectedObjectChanged(GameObject go) // null = deselected
    {
        OnSelectedObjectChanged?.Invoke(go);
    }
    #endregion

    #region UI Events
    public static void PublishUIPageChanged(UIManager.UIPage page)
    {
        OnUIPageChanged?.Invoke(page);
    }

    public static void PublishUIPageChangeRequested(UIManager.UIPage page)
    {
        OnUIPageChangeRequested?.Invoke(page);
    }

    public static void PublishBackButtonClicked()
    {
        OnBackButtonClicked?.Invoke();
    }

    public static void PublishIndexSearchButtonClicked()
    {
        OnIndexSearchButtonClicked?.Invoke();
    }

    public static void PublishToggleLabelButtonClicked()
    {
        OnToggleLabelButtonClicked?.Invoke();
    }

    public static void PublishToggleOverlayButtonClicked()
    {
        OnToggleOverlayButtonClicked?.Invoke();
    }

    public static void PublishHelpButtonClicked()
    {
        OnHelpButtonClicked?.Invoke();
    }

    public static void PublishPdfButtonClicked()
    {
        OnPdfButtonClicked?.Invoke();
    }
    #endregion


    #region Operation Sequence Methods
    public static void PublishOperationStepChanged(int stepIndex)
    {
        OnOperationStepChanged?.Invoke(stepIndex);
    }

    public static void PublishOperationGoToStepRequested(int stepIndex)
    {
        OnOperationGoToStepRequested?.Invoke(stepIndex);
    }

    public static void PublishSequenceModeChangeRequested(SequenceMode mode)
    {
        OnSequenceModeChangeRequested?.Invoke(mode);
    }
    #endregion
}

using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// ARScan_Backup — AR Foundation 6.3.3
///
/// Listens for OnUIPageChanged events via EventBus.
/// When the active page is UIPage.Scan it will:
///   1. Remove any active 3-D object visuals.
///   2. Reset and restart AR image-reference scanning.
///   3. Once the target image is tracked, use its pose as the scene origin.
///   4. Spawn a new 3-D object and let its position stabilise.
///   5. Lock the object in world space and disable image tracking.
///   6. Publish EventBus.PublishARObjectSpawned(activeObject).
/// </summary>
public class ARScan_Backup : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------
    [Header("AR Components")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Prefab")]
    [Tooltip("The 3-D prefab that will be spawned on top of the tracked image.")]
    [SerializeField] private GameObject objectPrefab;

    [Header("Stabilisation")]
    [Tooltip("How many consecutive frames the tracked pose must be stable before locking.")]
    [SerializeField] private int stabilisationFrames = 30;

    [Tooltip("Maximum positional drift (metres) allowed per frame to be considered stable.")]
    [SerializeField] private float stabilisationThreshold = 0.005f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private GameObject activeObject;

    private bool isScanning;
    private bool isStabilising;
    private bool isLocked;

    private ARTrackedImage currentTrackedImage;
    private int stableFrameCount;
    private Vector3 lastTrackedPosition;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        EventBus.OnUIPageChanged += HandleUIPageChanged;
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        EventBus.OnUIPageChanged -= HandleUIPageChanged;
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void Update()
    {
        if (isStabilising && !isLocked)
        {
            StabiliseObjectPosition();
        }
    }

    // -------------------------------------------------------------------------
    // Event handlers
    // -------------------------------------------------------------------------

    /// <summary>Reacts to UIPage changes broadcast by EventBus.</summary>
    private void HandleUIPageChanged(UIManager.UIPage page)
    {
        if (page != UIManager.UIPage.Scan) return;

        RemoveActiveObject();
        ResetARScanning();
        BeginScanning();
    }

    /// <summary>Called by ARTrackedImageManager whenever tracked images change.</summary>
    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        if (!isScanning) return;

        // Handle newly added trackables
        foreach (ARTrackedImage trackedImage in args.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                OnImageFound(trackedImage);
                return;
            }
        }

        // Handle trackables whose state updated to Tracking
        foreach (ARTrackedImage trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking
                && currentTrackedImage == null)
            {
                OnImageFound(trackedImage);
                return;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Core scanning flow
    // -------------------------------------------------------------------------

    /// <summary>Step 1 — Destroy any previously spawned 3-D visuals.</summary>
    private void RemoveActiveObject()
    {
        if (activeObject != null)
        {
            Destroy(activeObject);
            activeObject = null;
        }

        currentTrackedImage = null;
        isStabilising = false;
        isLocked = false;
        stableFrameCount = 0;
    }

    /// <summary>Step 2 — Reset the ARTrackedImageManager subsystem.</summary>
    private void ResetARScanning()
    {
        trackedImageManager.enabled = false;
        trackedImageManager.enabled = true;

        isScanning = false;
        Debug.Log("[ARScanning] AR scanning has been reset.");
    }

    /// <summary>Step 3 — Enable scanning to detect the image reference.</summary>
    private void BeginScanning()
    {
        isScanning = true;
        Debug.Log("[ARScanning] Scanning for image reference…");
    }

    /// <summary>Step 4 — Image reference found; use its pose as the world origin and spawn the object.</summary>
    private void OnImageFound(ARTrackedImage trackedImage)
    {
        isScanning = false;
        currentTrackedImage = trackedImage;

        Debug.Log($"[ARScanning] Image reference found: {trackedImage.referenceImage.name}");

        // Use the tracked image's pose as the origin for the spawned object.
        Vector3    spawnPosition = trackedImage.transform.position;
        Quaternion spawnRotation = trackedImage.transform.rotation * Quaternion.Euler(90f, 0f, 0f);

        activeObject = Instantiate(objectPrefab, spawnPosition, spawnRotation);

        // Begin stabilisation.
        lastTrackedPosition = spawnPosition;
        stableFrameCount    = 0;
        isStabilising       = true;
        isLocked            = false;

        Debug.Log("[ARScanning] Object spawned — beginning stabilisation…");
    }

    /// <summary>
    /// Step 5 — Called every frame while stabilising.
    /// Tracks the image-reference pose and waits until it is steady enough.
    /// </summary>
    private void StabiliseObjectPosition()
    {
        if (currentTrackedImage == null || activeObject == null) return;

        // Keep the object aligned with the image reference while stabilising.
        Vector3    trackedPosition = currentTrackedImage.transform.position;
        Quaternion trackedRotation = currentTrackedImage.transform.rotation;

        activeObject.transform.SetPositionAndRotation(trackedPosition, trackedRotation * Quaternion.Euler(90f, 0f, 0f));

        float drift = Vector3.Distance(trackedPosition, lastTrackedPosition);

        if (drift < stabilisationThreshold)
        {
            stableFrameCount++;
        }
        else
        {
            stableFrameCount = 0;
        }

        lastTrackedPosition = trackedPosition;

        if (stableFrameCount >= stabilisationFrames)
        {
            LockObjectToImageReference();
        }
    }

    /// <summary>Step 6 — Pose is stable; lock the object in world space and publish the event.</summary>
    private void LockObjectToImageReference()
    {
        isStabilising = false;
        isLocked      = true;

        // Disable tracking so the object stays fixed in world space,
        // completely unaffected by any further image tracking updates.
        trackedImageManager.enabled = false;

        Debug.Log("[ARScanning] Object position locked in world space. Image tracking disabled.");

        // Publish the spawned object via EventBus.
        EventBus.PublishARObjectSpawned(activeObject);

        Debug.Log("[ARScanning] ARObjectSpawned event published.");
    }
}

/*
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARScan : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("Spawned Object Reference")]
    [SerializeField] private GameObject activeObject;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    // -------------------------------------------------------------------------
    // Tracking listener
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fired by ARTrackedImageManager whenever a tracked image is added, updated, or removed.
    /// When a new image is successfully tracked, retrieve its spawned prefab instance
    /// and register it.
    /// </summary>
    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // ARTrackedImageManager spawns the prefab as a child of the ARTrackedImage.
                // GetComponentInChildren finds that spawned instance.
                GameObject spawnedObject = trackedImage.gameObject;

                RegisterActiveObject(spawnedObject);
                return;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void ResetScan()
    {
        RemoveActiveObject();
        RestartImageTracking();
    }

    public void RegisterActiveObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[ARScan] RegisterActiveObject called with a null object — signal not sent.");
            return;
        }

        activeObject = obj;
        Debug.Log($"[ARScan] Active object registered: {obj.name}");

        EventBus.PublishARObjectSpawned(activeObject);
        Debug.Log("[ARScan] ARObjectSpawned event published.");
    }

    // -------------------------------------------------------------------------
    // Internal steps
    // -------------------------------------------------------------------------

    private void RemoveActiveObject()
    {
        if (activeObject != null)
        {
            Destroy(activeObject);
            activeObject = null;
            Debug.Log("[ARScan] Active object destroyed.");
        }
        else
        {
            Debug.Log("[ARScan] No active object to destroy.");
        }
    }

    private void RestartImageTracking()
    {
        if (trackedImageManager == null)
        {
            Debug.LogWarning("[ARScan] ARTrackedImageManager is not assigned.");
            return;
        }

        trackedImageManager.enabled = false;
        trackedImageManager.enabled = true;

        Debug.Log("[ARScan] ARTrackedImageManager restarted — scanning for image reference.");
    }
}
*/
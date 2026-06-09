using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// ARScan — AR Foundation 6.3.3
///
/// Listens for OnUIPageChanged events via EventBus.
/// When the active page is UIPage.Scan it will:
///   1. Remove any active 3-D object visuals.
///   2. Reset and restart AR image-reference scanning.
///   3. Once the target image is tracked, use its pose as the scene origin.
///   4. Spawn a new 3-D object and let its position stabilise.
///   5. Attach a free ARAnchor at the stabilised pose to resist X/Z drift.
///   6. Publish EventBus.PublishARObjectSpawned(activeObject).
/// </summary>
public class ARScan2 : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------
    [Header("AR Components")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    [SerializeField] private ARAnchorManager anchorManager;

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
        ResetARScan();
        BeginScanning();
    }

    /// <summary>Called by ARTrackedImageManager whenever tracked images change.</summary>
    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        if (!isScanning) return;

        foreach (ARTrackedImage trackedImage in args.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                OnImageFound(trackedImage);
                return;
            }
        }

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
    private void ResetARScan()
    {
        trackedImageManager.enabled = false;
        trackedImageManager.enabled = true;

        isScanning = false;
        Debug.Log("[ARScan] AR scanning has been reset.");
    }

    /// <summary>Step 3 — Enable scanning to detect the image reference.</summary>
    private void BeginScanning()
    {
        isScanning = true;
        Debug.Log("[ARScan] Scanning for image reference…");
    }

    /// <summary>Step 4 — Image reference found; use its pose as the world origin and spawn the object.</summary>
    private void OnImageFound(ARTrackedImage trackedImage)
    {
        isScanning = false;
        currentTrackedImage = trackedImage;

        Debug.Log($"[ARScan] Image reference found: {trackedImage.referenceImage.name}");

        Vector3    spawnPosition = trackedImage.transform.position;
        Quaternion spawnRotation = trackedImage.transform.rotation * Quaternion.Euler(90f, 0f, 0f);

        activeObject = Instantiate(objectPrefab, spawnPosition, spawnRotation);

        lastTrackedPosition = spawnPosition;
        stableFrameCount    = 0;
        isStabilising       = true;
        isLocked            = false;

        Debug.Log("[ARScan] Object spawned — beginning stabilisation…");
    }

    /// <summary>
    /// Step 5 — Called every frame while stabilising.
    /// Follows the live image pose until drift is below threshold for enough consecutive frames.
    /// </summary>
    private void StabiliseObjectPosition()
    {
        if (currentTrackedImage == null || activeObject == null) return;

        Vector3    trackedPosition = currentTrackedImage.transform.position;
        Quaternion trackedRotation = currentTrackedImage.transform.rotation;

        activeObject.transform.SetPositionAndRotation(trackedPosition, trackedRotation * Quaternion.Euler(90f, 0f, 0f));

        float drift = Vector3.Distance(trackedPosition, lastTrackedPosition);

        if (drift < stabilisationThreshold)
            stableFrameCount++;
        else
            stableFrameCount = 0;

        lastTrackedPosition = trackedPosition;

        if (stableFrameCount >= stabilisationFrames)
            LockObjectToImageReference();
    }

    /// <summary>
    /// Step 6 — Pose is stable; stop image tracking, attach a free ARAnchor at the
    /// stabilised world pose, then publish the event.
    /// Using TryAddAnchorAsync creates a platform-native anchor (no ARPlane required)
    /// which the AR runtime continuously corrects against its world map, minimising X/Z drift.
    /// </summary>
    private async void LockObjectToImageReference()
    {
        isStabilising = false;
        isLocked      = true;

        // Stop image tracking — the object is now owned by the anchor.
        trackedImageManager.enabled = false;

        Pose lockedPose = new Pose(activeObject.transform.position, activeObject.transform.rotation);

        Result<ARAnchor> result = await anchorManager.TryAddAnchorAsync(lockedPose);

        if (result.status.IsSuccess())
        {
            // Parent the object to the anchor so the runtime's world-map corrections
            // are applied to it automatically, countering X/Z perception drift.
            activeObject.transform.SetParent(result.value.transform, worldPositionStays: true);
            Debug.Log("[ARScan] Free ARAnchor attached — object stabilised against drift.");
        }
        else
        {
            // Fallback: object stays at its raw world-space pose.
            Debug.LogWarning("[ARScan] ARAnchor creation failed — object locked in raw world space.");
        }

        Debug.Log("[ARScan] Object position locked. Image tracking disabled.");

        EventBus.PublishARObjectSpawned(activeObject);
        Debug.Log("[ARScan] ARObjectSpawned event published.");
    }
}
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;

public class ARFlowManager : MonoBehaviour
{
    [Header("AR Components")]
    public ARSession arSession;
    public ARTrackedImageManager trackedImageManager;

    [Header("Scan Manifest")]
    public GameObject arObjectPrefab;

    [Header("Stabilization Settings")]
    public float stabilizationDuration = 1.5f;
    public float dampingFactor = 0.15f;

    private GameObject currentARObject;
    private GameObject worldAnchor;
    private bool scanActive;
    private bool scanSucceeded;
    private bool isAnchored;
    private Quaternion rotationOffset = Quaternion.identity;

    void Awake()
    {
        if (arSession == null)
            arSession = FindAnyObjectByType<ARSession>();

        if (trackedImageManager == null)
            trackedImageManager = FindAnyObjectByType<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
        
        EventBus.OnUIPageChanged += ResetScanSession;
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
        
        EventBus.OnUIPageChanged -= ResetScanSession;
    }

    public void StartScanSession()
    {
        if (scanActive)
            return;

        scanActive = true;
        scanSucceeded = false;
        ClearCurrentARObject();
        EnableARComponents(true);
        EventBus.PublishScanStarted();
        Debug.Log("ARFlowManager: Scan session started");
    }

    public void ResetScanSession(UIManager.UIPage page)
    {
        if (page != UIManager.UIPage.Scan)
            return;
        
        EndScanSession();
        StartScanSession();
    }

    public void EndScanSession()
    {
        if (!scanActive)
            return;

        scanActive = false;
        isAnchored = false;
        EnableARComponents(false);
        ClearCurrentARObject();
        ClearWorldAnchor();
        EventBus.PublishScanEnded();
        Debug.Log("ARFlowManager: Scan session ended");
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        if (!scanActive || scanSucceeded)
            return;

        foreach (var trackedImage in args.added)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                HandleScanSuccess(trackedImage);
                return;
            }
        }

        foreach (var trackedImage in args.updated)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                HandleScanSuccess(trackedImage);
                return;
            }
        }
    }

    private void HandleScanSuccess(ARTrackedImage trackedImage)
    {
        scanSucceeded = true;
        isAnchored = false;
        Debug.Log($"ARFlowManager: Scan successful for '{trackedImage.referenceImage.name}'");
        
        // Store the prefab's rotation offset to maintain perpendicularity
        rotationOffset = arObjectPrefab.transform.localRotation;
        
        // Create world anchor at image position
        Vector3 anchorPosition = trackedImage.transform.position;
        Quaternion anchorRotation = trackedImage.transform.rotation;
        
        // Apply prefab's rotation offset to maintain perpendicularity relative to the image
        Quaternion finalRotation = anchorRotation * rotationOffset;
        
        GameObject spawnedObject = SpawnARObject(anchorPosition, finalRotation);
        
        // Start stabilization and anchor locking
        StartCoroutine(StabilizeAndLockAnchor(spawnedObject, trackedImage));
        
        EventBus.PublishScanSuccess();
        EventBus.PublishARObjectSpawned(spawnedObject);
    }

    private IEnumerator StabilizeAndLockAnchor(GameObject arObject, ARTrackedImage trackedImage)
    {
        float elapsedTime = 0f;
        Vector3 targetPosition = trackedImage.transform.position;
        Quaternion targetRotation = trackedImage.transform.rotation * rotationOffset;

        // Smoothly move to stable position while still tracking
        while (elapsedTime < stabilizationDuration && trackedImage.trackingState == TrackingState.Tracking)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / stabilizationDuration;
            
            // Update target to track image while maintaining rotation offset for perpendicularity
            targetPosition = Vector3.Lerp(arObject.transform.position, trackedImage.transform.position, dampingFactor);
            targetRotation = Quaternion.Lerp(arObject.transform.rotation, trackedImage.transform.rotation * rotationOffset, dampingFactor);
            
            arObject.transform.position = targetPosition;
            arObject.transform.rotation = targetRotation;
            
            yield return null;
        }

        // Lock anchor in place
        LockAnchor(arObject);
    }

    private void LockAnchor(GameObject arObject)
    {
        if (isAnchored)
            return;

        isAnchored = true;
        
        // Detach from image tracking by creating a world anchor
        worldAnchor = new GameObject($"WorldAnchor_{arObject.name}");
        worldAnchor.transform.position = arObject.transform.position;
        worldAnchor.transform.rotation = arObject.transform.rotation;
        
        // Reparent AR object to world anchor instead of tracked image
        arObject.transform.SetParent(worldAnchor.transform);
        arObject.transform.localPosition = Vector3.zero;
        arObject.transform.localRotation = Quaternion.identity;
        
        // Disable image tracking after anchor is locked
        if (trackedImageManager != null)
            trackedImageManager.enabled = false;
        
        Debug.Log("ARFlowManager: Anchor locked. Image tracking disabled.");
        EventBus.PublishAnchorLocked();
    }

    private GameObject SpawnARObject(Vector3 position, Quaternion rotation)
    {
        if (arObjectPrefab == null)
            return null;

        ClearCurrentARObject();
        currentARObject = Instantiate(arObjectPrefab, position, rotation);
        return currentARObject;
    }

    private void ClearCurrentARObject()
    {
        if (currentARObject != null)
            Destroy(currentARObject);

        currentARObject = null;
    }

    private void ClearWorldAnchor()
    {
        if (worldAnchor != null)
            Destroy(worldAnchor);

        worldAnchor = null;
    }

    private void EnableARComponents(bool enabled)
    {
        if (arSession != null)
            arSession.enabled = enabled;

        if (trackedImageManager != null)
            trackedImageManager.enabled = enabled;
    }
}
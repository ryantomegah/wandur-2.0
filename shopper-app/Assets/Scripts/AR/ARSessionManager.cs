using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class ARSessionManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARSession arSession;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private Camera arCamera;
    
    [Header("Settings")]
    [SerializeField] private bool autoInitialize = true;
    [SerializeField] private float initializationTimeout = 10f;
    [SerializeField] private float requiredPlanesForInitialization = 3;
    [SerializeField] private bool enablePlaneVisualization = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private GameObject debugVisual;
    
    // Events
    public event Action OnARSessionInitialized;
    public event Action<ARPlane> OnPlaneDetected;
    public event Action<Vector3, Pose> OnTapDetected;
    public event Action<ARTrackingState> OnTrackingStateChanged;
    
    // State
    private bool isInitialized = false;
    private int detectedPlaneCount = 0;
    private ARTrackingState currentTrackingState = ARTrackingState.None;
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    
    private void Awake()
    {
        // Auto-find components if not assigned
        if (arSession == null) arSession = FindObjectOfType<ARSession>();
        if (arPlaneManager == null) arPlaneManager = FindObjectOfType<ARPlaneManager>();
        if (arRaycastManager == null) arRaycastManager = FindObjectOfType<ARRaycastManager>();
        if (arCameraManager == null) arCameraManager = FindObjectOfType<ARCameraManager>();
        if (arCamera == null && Camera.main != null) arCamera = Camera.main;
        
        // Validate essential components
        if (arSession == null || arPlaneManager == null || arRaycastManager == null || arCamera == null)
        {
            LogError("Missing required AR components. Please check the inspector.");
            enabled = false;
            return;
        }
    }
    
    private void Start()
    {
        if (autoInitialize)
        {
            StartCoroutine(InitializeARSession());
        }
        
        // Configure AR components
        if (arPlaneManager != null)
        {
            arPlaneManager.planesChanged += HandlePlanesChanged;
            
            // Set plane visualization
            arPlaneManager.planePrefab.GetComponent<MeshRenderer>().enabled = enablePlaneVisualization;
        }
        
        if (arCameraManager != null)
        {
            arCameraManager.frameReceived += HandleFrameReceived;
        }
        
        // Show/hide debug visual
        if (debugVisual != null)
        {
            debugVisual.SetActive(showDebugLogs);
        }
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        // Handle screen taps for raycast
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleScreenTap(Input.GetTouch(0).position);
        }
    }
    
    private IEnumerator InitializeARSession()
    {
        LogDebug("Initializing AR session...");
        
        // Reset AR session
        if (arSession != null)
        {
            arSession.Reset();
            yield return new WaitForSeconds(0.5f);
        }
        
        // Wait for tracking to stabilize
        float timeElapsed = 0f;
        detectedPlaneCount = 0;
        
        while (timeElapsed < initializationTimeout && detectedPlaneCount < requiredPlanesForInitialization)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        if (detectedPlaneCount >= requiredPlanesForInitialization)
        {
            isInitialized = true;
            LogDebug($"AR session initialized with {detectedPlaneCount} planes detected");
            OnARSessionInitialized?.Invoke();
        }
        else
        {
            LogWarning($"AR session initialization timed out after {initializationTimeout} seconds");
            // Try again with reduced requirements
            requiredPlanesForInitialization = 1;
            StartCoroutine(InitializeARSession());
        }
    }
    
    private void HandlePlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Track detected planes
        foreach (ARPlane plane in args.added)
        {
            detectedPlaneCount++;
            OnPlaneDetected?.Invoke(plane);
            LogDebug($"Plane detected: {plane.trackableId}, total: {detectedPlaneCount}");
        }
        
        // Update plane count when planes are removed
        foreach (ARPlane plane in args.removed)
        {
            detectedPlaneCount--;
            LogDebug($"Plane removed: {plane.trackableId}, total: {detectedPlaneCount}");
        }
    }
    
    private void HandleFrameReceived(ARCameraFrameEventArgs args)
    {
        if (args.trackingState != currentTrackingState)
        {
            currentTrackingState = args.trackingState;
            OnTrackingStateChanged?.Invoke(currentTrackingState);
            LogDebug($"Tracking state changed to: {currentTrackingState}");
        }
    }
    
    private void HandleScreenTap(Vector2 touchPosition)
    {
        if (arRaycastManager.Raycast(touchPosition, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            // Get the hit pose
            Pose hitPose = raycastHits[0].pose;
            
            // Get hit position in world space
            Vector3 hitPosition = hitPose.position;
            
            // Trigger tap event
            OnTapDetected?.Invoke(hitPosition, hitPose);
            LogDebug($"Tap detected at position: {hitPosition}");
        }
    }
    
    // Public methods
    public bool IsARSessionInitialized()
    {
        return isInitialized;
    }
    
    public ARTrackingState GetCurrentTrackingState()
    {
        return currentTrackingState;
    }
    
    public Camera GetARCamera()
    {
        return arCamera;
    }
    
    public bool TryGetPlaneHit(Vector2 screenPosition, out Pose pose)
    {
        pose = Pose.identity;
        
        if (arRaycastManager.Raycast(screenPosition, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            pose = raycastHits[0].pose;
            return true;
        }
        
        return false;
    }
    
    public bool TryGetGroundPlane(out Pose groundPose)
    {
        groundPose = Pose.identity;
        
        if (!isInitialized || arPlaneManager == null) return false;
        
        // Find a suitable ground plane (horizontal and below camera)
        foreach (ARPlane plane in arPlaneManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp && 
                plane.center.y < arCamera.transform.position.y)
            {
                groundPose = new Pose(plane.center, Quaternion.identity);
                return true;
            }
        }
        
        return false;
    }
    
    public void TogglePlaneVisualization(bool enable)
    {
        enablePlaneVisualization = enable;
        
        if (arPlaneManager != null && arPlaneManager.planePrefab != null)
        {
            foreach (ARPlane plane in arPlaneManager.trackables)
            {
                plane.GetComponent<MeshRenderer>().enabled = enable;
            }
        }
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ARSession] {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[ARSession] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[ARSession] {message}");
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (arPlaneManager != null)
        {
            arPlaneManager.planesChanged -= HandlePlanesChanged;
        }
        
        if (arCameraManager != null)
        {
            arCameraManager.frameReceived -= HandleFrameReceived;
        }
    }
} 
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections;
using Wandur.Navigation;

[RequireComponent(typeof(ARRaycastManager))]
public class ARNavigationManager : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARSession arSession;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Camera arCamera;

    [Header("Navigation Components")]
    [SerializeField] private DivineLineRenderer pathRenderer;
    [SerializeField] private GameObject destinationMarkerPrefab;
    [SerializeField] private GameObject waypointMarkerPrefab;
    [SerializeField] private float waypointSpacing = 2f; // Meters between waypoints
    [SerializeField] private float pathUpdateInterval = 0.5f; // Seconds between path updates

    [Header("Visual Settings")]
    [SerializeField] private Material divineMaterial;
    [SerializeField] private float pathWidth = 0.15f;
    [SerializeField] private float pathHeight = 0.05f;
    [SerializeField] private Color pathColor = new Color(0.5f, 0.8f, 1f, 0.8f);
    [SerializeField] private Texture2D noiseTexture;
    [SerializeField] private GameObject particleSystemPrefab;
    [SerializeField] private Gradient colorGradient;

    private List<Vector3> currentPath = new List<Vector3>();
    private GameObject destinationMarkerObject;
    private DestinationMarker destinationMarker;
    private List<GameObject> waypointMarkers = new List<GameObject>();
    private bool isNavigating = false;
    private Vector3 destination;
    private float lastPathUpdateTime;
    private Coroutine pathUpdateCoroutine;
    private float divineLineVisibleStartTime;
    private bool isDivineLineVisible = false;
    private string currentNavigationStoreId = "";
    private string currentNavigationStoreName = "";

    // Events for analytics tracking
    public event Action<string, string, Vector3, Vector3> OnNavigationStarted;
    public event Action<string, bool, float> OnNavigationEnded;
    public event Action<bool, float, string> OnDivineLineVisibilityChanged;
    public event Action<float> OnWaypointReached;
    public event Action<float> OnCloseToDestination;

    // Positioning system reference
    private OriientSDKManager positioningSystem;
    
    // Distance thresholds
    private const float CLOSE_TO_DESTINATION_THRESHOLD = 5.0f; // meters
    private const float DESTINATION_REACHED_THRESHOLD = 1.5f; // meters
    private const float WAYPOINT_REACHED_THRESHOLD = 2.0f; // meters
    
    // State tracking
    private bool hasNotifiedCloseToDestination = false;
    private int lastReachedWaypointIndex = -1;

    private void Awake()
    {
        // Get required components
        if (!raycastManager) raycastManager = GetComponent<ARRaycastManager>();
        if (!arCamera) arCamera = Camera.main;

        // Initialize path renderer
        if (pathRenderer)
        {
            InitializeDivineLine();
        }
    }
    
    private void InitializeDivineLine()
    {
        // Configure material properties if needed
        if (divineMaterial != null)
        {
            // Set noise texture if available
            if (noiseTexture != null)
            {
                divineMaterial.SetTexture("_DetailNoiseTex", noiseTexture);
            }
        }
        
        if (pathRenderer != null)
        {
            // Configure enhanced settings
            if (particleSystemPrefab != null)
            {
                pathRenderer.useParticles = true;
                pathRenderer.particleSystemPrefab = particleSystemPrefab;
            }
        }
    }

    private void Start()
    {
        // Subscribe to plane detection events
        if (planeManager)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
    }

    private void Update()
    {
        if (!isNavigating) return;

        // Update path periodically
        if (Time.time - lastPathUpdateTime >= pathUpdateInterval)
        {
            UpdatePath();
            lastPathUpdateTime = Time.time;
        }
        
        // Check proximity to destination
        CheckDestinationProximity();
        
        // Check if waypoints have been reached
        CheckWaypointProgress();
    }
    
    private void CheckDestinationProximity()
    {
        if (!isNavigating) return;
        
        Vector3 currentPosition = GetCurrentPosition();
        float distanceToDestination = Vector3.Distance(currentPosition, destination);
        
        // Update shader parameter for proximity glow
        if (pathRenderer != null)
        {
            // This will be used by the shader for responsive effects
            pathRenderer.SetWaypoints(currentPath, null);
        }
        
        // Check if close to destination but not yet notified
        if (!hasNotifiedCloseToDestination && distanceToDestination <= CLOSE_TO_DESTINATION_THRESHOLD)
        {
            OnCloseToDestination?.Invoke(distanceToDestination);
            hasNotifiedCloseToDestination = true;
            
            // Increase visual intensity near destination
            if (pathRenderer != null)
            {
                pathRenderer.SetGlowIntensity(2.0f);
            }
        }
        
        // Check if destination reached
        if (distanceToDestination <= DESTINATION_REACHED_THRESHOLD)
        {
            StopNavigation(true);
        }
    }
    
    private void CheckWaypointProgress()
    {
        if (!isNavigating || currentPath.Count <= 2) return;
        
        Vector3 currentPosition = GetCurrentPosition();
        
        // Skip the first point (current position) and check intermediate waypoints
        for (int i = 1; i < currentPath.Count - 1; i++)
        {
            if (i <= lastReachedWaypointIndex) continue;
            
            float distanceToWaypoint = Vector3.Distance(currentPosition, currentPath[i]);
            if (distanceToWaypoint <= WAYPOINT_REACHED_THRESHOLD)
            {
                OnWaypointReached?.Invoke(distanceToWaypoint);
                lastReachedWaypointIndex = i;
                
                // Pulse effect at the waypoint
                PulseWaypointEffect(currentPath[i]);
                
                break;
            }
        }
    }
    
    private void PulseWaypointEffect(Vector3 position)
    {
        // Create a temporary visual effect at the waypoint
        if (waypointMarkerPrefab != null)
        {
            GameObject pulseEffect = Instantiate(waypointMarkerPrefab, position, Quaternion.identity);
            
            // Animate and destroy
            StartCoroutine(AnimateAndDestroyWaypointPulse(pulseEffect));
        }
    }
    
    private IEnumerator AnimateAndDestroyWaypointPulse(GameObject pulseObject)
    {
        // Simple scale animation
        float duration = 1.0f;
        float startTime = Time.time;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * 2.0f;
        
        pulseObject.transform.localScale = startScale;
        
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            pulseObject.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            
            // Fade out
            Renderer renderer = pulseObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = 1 - t;
                renderer.material.color = color;
            }
            
            yield return null;
        }
        
        Destroy(pulseObject);
    }

    public void Initialize(ARSession session, Camera camera)
    {
        arSession = session;
        arCamera = camera;
        
        // Get AR components from the session GameObject
        if (raycastManager == null)
        {
            raycastManager = arSession.GetComponentInChildren<ARRaycastManager>();
        }
        
        if (planeManager == null)
        {
            planeManager = arSession.GetComponentInChildren<ARPlaneManager>();
        }
        
        // Subscribe to plane changed event for path updates
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
        
        // Initialize divine line renderer
        InitializeDivineLine();
        
        Debug.Log("AR Navigation Manager initialized");
    }

    public void InitializeForVirtualMode(Camera virtualCamera)
    {
        arCamera = virtualCamera;
        
        // No AR components in virtual mode
        arSession = null;
        raycastManager = null;
        planeManager = null;
        
        // Initialize divine line renderer
        InitializeDivineLine();
        
        Debug.Log("AR Navigation Manager initialized for virtual mode");
    }

    public void SetPositioningSystem(OriientSDKManager system)
    {
        positioningSystem = system;
        Debug.Log("Positioning system set to: " + system.name);
    }

    public void StartNavigation(string storeId, string storeName, Vector3 destination)
    {
        if (isNavigating)
        {
            StopNavigation(false);
        }
        
        this.destination = destination;
        this.currentNavigationStoreId = storeId;
        this.currentNavigationStoreName = storeName;
        
        // Reset state tracking variables
        hasNotifiedCloseToDestination = false;
        lastReachedWaypointIndex = -1;
        
        // Place destination marker
        PlaceDestinationMarker(destination);
        
        // Calculate initial path
        Vector3 startPosition = GetCurrentPosition();
        CalculatePath(startPosition, destination);
        
        // Start path updates
        if (pathUpdateCoroutine != null)
        {
            StopCoroutine(pathUpdateCoroutine);
        }
        pathUpdateCoroutine = StartCoroutine(UpdatePathRoutine());
        
        isNavigating = true;
        divineLineVisibleStartTime = Time.time;
        isDivineLineVisible = true;
        
        // Trigger analytics event
        OnNavigationStarted?.Invoke(storeId, storeName, startPosition, destination);
        
        Debug.Log($"Started navigation to {storeName} (ID: {storeId})");
    }

    public void StopNavigation(bool completed)
    {
        if (!isNavigating) return;
        
        // Clean up path visualization
        if (pathRenderer)
        {
            pathRenderer.Hide();
        }
        
        // Clean up markers
        if (destinationMarker != null)
        {
            if (completed)
            {
                // Play completion animation
                StartCoroutine(AnimateDestinationReached());
            }
            else
            {
                // Just destroy the marker
                if (destinationMarkerObject != null)
                {
                    Destroy(destinationMarkerObject);
                    destinationMarkerObject = null;
                    destinationMarker = null;
                }
            }
        }
        
        foreach (var marker in waypointMarkers)
        {
            if (marker) Destroy(marker);
        }
        waypointMarkers.Clear();
        
        // Stop path updates
        if (pathUpdateCoroutine != null)
        {
            StopCoroutine(pathUpdateCoroutine);
            pathUpdateCoroutine = null;
        }
        
        float distanceToTarget = 0f;
        if (completed)
        {
            distanceToTarget = 0f;
        }
        else
        {
            Vector3 currentPos = GetCurrentPosition();
            distanceToTarget = Vector3.Distance(currentPos, destination);
        }
        
        // Track divine line visibility duration
        if (isDivineLineVisible)
        {
            float visibilityDuration = Time.time - divineLineVisibleStartTime;
            OnDivineLineVisibilityChanged?.Invoke(false, visibilityDuration, currentNavigationStoreId);
            isDivineLineVisible = false;
        }
        
        isNavigating = false;
        
        // Trigger analytics event
        OnNavigationEnded?.Invoke(currentNavigationStoreId, completed, distanceToTarget);
        
        currentNavigationStoreId = "";
        currentNavigationStoreName = "";
        
        Debug.Log("Navigation stopped");
    }
    
    private IEnumerator AnimateDestinationReached()
    {
        if (destinationMarker == null) yield break;
        
        // Enhance the destination marker on arrival
        destinationMarker.SetColor(Color.green);
        
        // Wait for animation to play
        yield return new WaitForSeconds(3.0f);
        
        // Destroy the marker
        if (destinationMarkerObject != null)
        {
            Destroy(destinationMarkerObject);
            destinationMarkerObject = null;
            destinationMarker = null;
        }
    }

    private void CalculatePath(Vector3 start, Vector3 end)
    {
        currentPath.Clear();
        currentPath.Add(start);

        // Calculate intermediate waypoints
        Vector3 direction = (end - start);
        float distance = direction.magnitude;
        int numWaypoints = Mathf.FloorToInt(distance / waypointSpacing);

        for (int i = 1; i <= numWaypoints; i++)
        {
            float t = i / (float)(numWaypoints + 1);
            Vector3 waypoint = Vector3.Lerp(start, end, t);

            // Raycast to find ground position
            if (RaycastToGround(waypoint, out Vector3 groundPos))
            {
                currentPath.Add(groundPos + Vector3.up * pathHeight);
                PlaceWaypointMarker(groundPos);
            }
            else
            {
                currentPath.Add(waypoint + Vector3.up * pathHeight);
            }
        }

        currentPath.Add(end);

        // Update divine line
        if (pathRenderer)
        {
            pathRenderer.UpdatePath(currentPath.ToArray());
        }

        // Update waypoint markers
        UpdateWaypointMarkers();
    }

    private void UpdatePath()
    {
        if (!isNavigating) return;
        
        Vector3 currentPosition = GetCurrentPosition();
        CalculatePath(currentPosition, destination);
    }

    private bool RaycastToGround(Vector3 position, out Vector3 groundPosition)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Ray ray = new Ray(position + Vector3.up * 2f, Vector3.down);

        if (raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
        {
            groundPosition = hits[0].pose.position;
            return true;
        }

        groundPosition = position;
        return false;
    }

    private void PlaceWaypointMarker(Vector3 position)
    {
        if (waypointMarkerPrefab == null) return;

        GameObject marker = Instantiate(waypointMarkerPrefab, position, Quaternion.identity);
        waypointMarkers.Add(marker);
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (isNavigating)
        {
            UpdatePath();
        }
    }

    private void OnDestroy()
    {
        if (planeManager)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
        
        StopAllCoroutines();
    }

    public bool IsNavigating() => isNavigating;
    public Vector3 GetDestination() => destination;

    public void UpdateDestination(Vector3 newDestination)
    {
        if (!isNavigating) return;
        
        destination = newDestination;
        
        // Update destination marker
        if (destinationMarker != null)
        {
            destinationMarker.UpdatePosition(destination);
        }
        else
        {
            PlaceDestinationMarker(destination);
        }
        
        // Recalculate path
        UpdatePath();
    }

    public float GetRemainingDistance()
    {
        if (!isNavigating) return 0f;
        
        Vector3 currentPosition = GetCurrentPosition();
        return Vector3.Distance(currentPosition, destination);
    }

    public Vector3 GetNextWaypoint()
    {
        if (!isNavigating || currentPath.Count < 2) return Vector3.zero;
        
        Vector3 currentPosition = GetCurrentPosition();
        float minDistance = float.MaxValue;
        int closestWaypointIndex = -1;
        
        // Find the closest waypoint on the path
        for (int i = 0; i < currentPath.Count; i++)
        {
            float distance = Vector3.Distance(currentPosition, currentPath[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestWaypointIndex = i;
            }
        }
        
        // Return the next waypoint after the closest one
        if (closestWaypointIndex >= 0 && closestWaypointIndex < currentPath.Count - 1)
        {
            return currentPath[closestWaypointIndex + 1];
        }
        
        // If no suitable waypoint found, return the destination
        return destination;
    }

    private IEnumerator UpdatePathRoutine()
    {
        while (isNavigating)
        {
            UpdatePath();
            yield return new WaitForSeconds(pathUpdateInterval);
        }
    }

    private Vector3 GetCurrentPosition()
    {
        if (positioningSystem != null && positioningSystem.IsTracking())
        {
            Vector3 position = positioningSystem.GetCurrentPosition();
            return position;
        }
        
        // Fallback to camera position if no positioning system
        if (arCamera != null)
        {
            return arCamera.transform.position;
        }
        
        return transform.position;
    }

    private void PlaceDestinationMarker(Vector3 position)
    {
        // Clean up existing marker
        if (destinationMarkerObject != null)
        {
            Destroy(destinationMarkerObject);
            destinationMarkerObject = null;
            destinationMarker = null;
        }
        
        // Create new marker
        if (destinationMarkerPrefab != null)
        {
            destinationMarkerObject = Instantiate(destinationMarkerPrefab, position, Quaternion.identity);
            destinationMarker = destinationMarkerObject.GetComponent<DestinationMarker>();
            
            if (destinationMarker != null)
            {
                // Initialize with the destination position
                destinationMarker.Initialize(position);
                
                // Set color to match the divine line
                destinationMarker.SetColor(pathColor);
            }
        }
    }

    private void UpdateWaypointMarkers()
    {
        // Clear existing markers
        foreach (var marker in waypointMarkers)
        {
            if (marker) Destroy(marker);
        }
        waypointMarkers.Clear();
        
        // Skip if no path or no waypoint prefab
        if (currentPath.Count <= 2 || waypointMarkerPrefab == null) return;
        
        // Place markers at each intermediate waypoint
        for (int i = 1; i < currentPath.Count - 1; i++)
        {
            PlaceWaypointMarker(currentPath[i]);
        }
    }
} 
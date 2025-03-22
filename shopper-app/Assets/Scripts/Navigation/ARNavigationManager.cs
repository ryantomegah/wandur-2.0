using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections;

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

    private List<Vector3> currentPath = new List<Vector3>();
    private GameObject destinationMarker;
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

    // Positioning system reference
    private OriientSDKManager positioningSystem;

    private void Awake()
    {
        // Get required components
        if (!raycastManager) raycastManager = GetComponent<ARRaycastManager>();
        if (!arCamera) arCamera = Camera.main;

        // Initialize path renderer
        if (pathRenderer)
        {
            pathRenderer.Initialize(divineMaterial, pathWidth, pathHeight, pathColor);
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

        // Update divine line effect
        if (pathRenderer && currentPath.Count > 0)
        {
            pathRenderer.UpdatePath(currentPath.ToArray());
        }
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
        if (pathRenderer != null && divineMaterial != null)
        {
            pathRenderer.Initialize(divineMaterial, pathWidth, pathHeight, pathColor);
        }
        
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
        if (pathRenderer != null && divineMaterial != null)
        {
            pathRenderer.Initialize(divineMaterial, pathWidth, pathHeight, pathColor);
        }
        
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
            pathRenderer.ClearPath();
        }
        
        // Clean up markers
        if (destinationMarker) Destroy(destinationMarker);
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

        Vector3 currentPosition = arCamera.transform.position;
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
        // Update path when new planes are detected
        if (isNavigating && args.added.Count > 0)
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
    }

    // Public methods for external control
    public bool IsNavigating => isNavigating;
    public Vector3 CurrentDestination => destination;

    public void UpdateDestination(Vector3 newDestination)
    {
        if (isNavigating)
        {
            destination = newDestination;
            if (destinationMarker)
            {
                destinationMarker.transform.position = newDestination;
            }
            UpdatePath();
        }
    }

    public float GetRemainingDistance()
    {
        if (!isNavigating || currentPath.Count < 2) return 0f;

        float distance = 0f;
        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            distance += Vector3.Distance(currentPath[i], currentPath[i + 1]);
        }
        return distance;
    }

    public Vector3 GetNextWaypoint()
    {
        if (!isNavigating || currentPath.Count < 2) return Vector3.zero;

        Vector3 currentPos = arCamera.transform.position;
        float minDist = float.MaxValue;
        int nearestIndex = 0;

        // Find the nearest point on the path
        for (int i = 0; i < currentPath.Count; i++)
        {
            float dist = Vector3.Distance(currentPos, currentPath[i]);
            if (dist < minDist)
            {
                minDist = dist;
                nearestIndex = i;
            }
        }

        // Return the next waypoint
        if (nearestIndex < currentPath.Count - 1)
        {
            return currentPath[nearestIndex + 1];
        }

        return destination;
    }

    private IEnumerator UpdatePathRoutine()
    {
        while (isNavigating)
        {
            yield return new WaitForSeconds(pathUpdateInterval);
            
            Vector3 currentPosition = GetCurrentPosition();
            CalculatePath(currentPosition, destination);
            
            // Check if we've reached the destination
            float distanceToDestination = Vector3.Distance(currentPosition, destination);
            if (distanceToDestination < 1.0f)
            {
                StopNavigation(true);
                yield break;
            }
        }
    }

    private Vector3 GetCurrentPosition()
    {
        if (positioningSystem != null && positioningSystem.IsTracking())
        {
            // Use indoor positioning when available
            return positioningSystem.GetCurrentPosition();
        }
        else if (arCamera != null)
        {
            // Fallback to camera position
            return new Vector3(arCamera.transform.position.x, 0, arCamera.transform.position.z);
        }
        
        return Vector3.zero;
    }

    private void PlaceDestinationMarker(Vector3 position)
    {
        if (destinationMarkerPrefab != null)
        {
            if (destinationMarker != null)
            {
                Destroy(destinationMarker);
            }
            
            Vector3 groundPosition = RaycastToGround(position);
            destinationMarker = Instantiate(destinationMarkerPrefab, groundPosition, Quaternion.identity);
            destinationMarker.transform.SetParent(transform);
        }
    }

    private void UpdateWaypointMarkers()
    {
        if (waypointMarkerPrefab == null) return;
        
        // Clean up existing markers
        foreach (var marker in waypointMarkers)
        {
            Destroy(marker);
        }
        waypointMarkers.Clear();
        
        // Only place waypoint markers at intervals
        for (int i = 1; i < currentPath.Count - 1; i += 3)
        {
            GameObject marker = Instantiate(waypointMarkerPrefab, currentPath[i], Quaternion.identity);
            marker.transform.SetParent(transform);
            waypointMarkers.Add(marker);
        }
    }
} 
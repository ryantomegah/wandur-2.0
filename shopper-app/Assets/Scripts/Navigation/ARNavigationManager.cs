using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public void StartNavigation(Vector3 targetPosition)
    {
        destination = targetPosition;
        isNavigating = true;

        // Place destination marker
        if (destinationMarker == null && destinationMarkerPrefab != null)
        {
            destinationMarker = Instantiate(destinationMarkerPrefab, targetPosition, Quaternion.identity);
        }
        else if (destinationMarker != null)
        {
            destinationMarker.transform.position = targetPosition;
        }

        // Initial path calculation
        CalculatePath(arCamera.transform.position, targetPosition);
        lastPathUpdateTime = Time.time;
    }

    public void StopNavigation()
    {
        isNavigating = false;
        currentPath.Clear();

        // Clean up markers
        if (destinationMarker) Destroy(destinationMarker);
        foreach (var marker in waypointMarkers)
        {
            if (marker) Destroy(marker);
        }
        waypointMarkers.Clear();

        // Clear divine line
        if (pathRenderer)
        {
            pathRenderer.ClearPath();
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
} 
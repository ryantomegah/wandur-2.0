using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the visualization of "Divine Lines" for AR navigation.
/// This component creates an attractive visual path that guides users to their destination.
/// </summary>
public class DivineLineRenderer : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private int cornerVertices = 4;
    [SerializeField] private int endCapVertices = 4;
    
    [Header("Animation")]
    [SerializeField] private bool animateLine = true;
    [SerializeField] private float flowSpeed = 1f;
    [SerializeField] private Color lineColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    [SerializeField] private Color secondaryColor = new Color(0.1f, 0.3f, 0.8f, 0.5f);
    
    [Header("Particles")]
    [SerializeField] private bool useParticles = true;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private float particleSpacing = 1.5f;
    [SerializeField] private float particleSpeed = 2f;
    
    // Line renderer component
    private LineRenderer lineRenderer;
    
    // Particle system objects
    private List<GameObject> particleSystems = new List<GameObject>();
    
    // Position data
    private Vector3[] positions;
    private int positionCount;
    
    // Material animation properties
    private float offset = 0f;
    
    // Curve for line trajectory
    [SerializeField] private AnimationCurve lineHeightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
    [SerializeField] private float maxHeight = 0.5f;

    private void Awake()
    {
        // Get or add line renderer component
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure line renderer
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.numCornerVertices = cornerVertices;
        lineRenderer.numCapVertices = endCapVertices;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.useWorldSpace = true;
        lineRenderer.alignment = LineAlignment.View; // Align to camera view
        
        // If we're using a custom shader with animation properties, set those up
        if (animateLine && lineMaterial != null)
        {
            lineMaterial.SetColor("_Color", lineColor);
            lineMaterial.SetColor("_SecondaryColor", secondaryColor);
            lineMaterial.SetFloat("_FlowSpeed", flowSpeed);
        }
    }

    /// <summary>
    /// Sets the line path to the provided waypoints
    /// </summary>
    /// <param name="waypoints">Array of positions defining the path</param>
    public void SetPath(Vector3[] waypoints)
    {
        ClearParticles();
        
        positions = ProcessWaypoints(waypoints);
        positionCount = positions.Length;
        
        // Update line renderer
        lineRenderer.positionCount = positionCount;
        lineRenderer.SetPositions(positions);
        
        // Create particles if enabled
        if (useParticles && particlePrefab != null)
        {
            CreateParticles();
        }
    }

    /// <summary>
    /// Processes waypoints to create a smooth, elevated path
    /// </summary>
    private Vector3[] ProcessWaypoints(Vector3[] waypoints)
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            return waypoints;
        }
        
        // Implement a more sophisticated path with some height variance
        // to make the line appear to "float" in AR space
        List<Vector3> processedPoints = new List<Vector3>();
        
        // For each segment between waypoints
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 start = waypoints[i];
            Vector3 end = waypoints[i + 1];
            float segmentLength = Vector3.Distance(start, end);
            
            // Calculate how many points to use for this segment
            // More points for longer segments
            int segmentPoints = Mathf.Max(2, Mathf.CeilToInt(segmentLength * 2));
            
            for (int j = 0; j < segmentPoints; j++)
            {
                float t = j / (float)(segmentPoints - 1);
                Vector3 point = Vector3.Lerp(start, end, t);
                
                // Add height variation based on curve
                float heightFactor = lineHeightCurve.Evaluate(t);
                point.y += heightFactor * maxHeight;
                
                processedPoints.Add(point);
            }
        }
        
        return processedPoints.ToArray();
    }

    /// <summary>
    /// Creates particle effects along the line path
    /// </summary>
    private void CreateParticles()
    {
        if (particlePrefab == null || positions.Length < 2)
            return;
            
        float totalDistance = 0f;
        
        // Calculate total path length
        for (int i = 1; i < positions.Length; i++)
        {
            totalDistance += Vector3.Distance(positions[i - 1], positions[i]);
        }
        
        // Calculate number of particles
        int particleCount = Mathf.FloorToInt(totalDistance / particleSpacing);
        
        // Place particles evenly along the path
        float currentDistance = 0f;
        float distancePerParticle = totalDistance / particleCount;
        
        for (int i = 0; i < particleCount; i++)
        {
            currentDistance += distancePerParticle;
            
            // Find the position along the path at the current distance
            Vector3 position = GetPositionAlongPath(currentDistance);
            
            // Create particle
            GameObject particle = Instantiate(particlePrefab, position, Quaternion.identity);
            particle.transform.parent = transform;
            
            // Store for later cleanup
            particleSystems.Add(particle);
        }
    }

    /// <summary>
    /// Gets a position along the path at a specified distance
    /// </summary>
    private Vector3 GetPositionAlongPath(float distance)
    {
        float totalDistance = 0f;
        
        for (int i = 1; i < positions.Length; i++)
        {
            float segmentDistance = Vector3.Distance(positions[i - 1], positions[i]);
            
            if (totalDistance + segmentDistance >= distance)
            {
                // This is the segment where our point lies
                float t = (distance - totalDistance) / segmentDistance;
                return Vector3.Lerp(positions[i - 1], positions[i], t);
            }
            
            totalDistance += segmentDistance;
        }
        
        // If we've gone past the end, return the last point
        return positions[positions.Length - 1];
    }

    /// <summary>
    /// Clears all particle effects
    /// </summary>
    private void ClearParticles()
    {
        foreach (GameObject particle in particleSystems)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        
        particleSystems.Clear();
    }

    private void Update()
    {
        if (animateLine && lineMaterial != null)
        {
            // Update animation offset for flowing effect
            offset += Time.deltaTime * flowSpeed;
            lineMaterial.SetFloat("_Offset", offset);
        }
        
        if (useParticles && particleSystems.Count > 0)
        {
            // Animate particles along the path
            AnimateParticles();
        }
    }

    /// <summary>
    /// Moves particles along the path for a dynamic effect
    /// </summary>
    private void AnimateParticles()
    {
        // This is a placeholder for particle movement along the path
        // In a real implementation, you would move the particles along the curve
        
        // For simplicity, we'll just move each particle forward through the path
        for (int i = 0; i < particleSystems.Count; i++)
        {
            GameObject particle = particleSystems[i];
            if (particle != null)
            {
                // Move the particle along the path
                // This is a simplified implementation
                float speed = particleSpeed * Time.deltaTime;
                
                // For this example, we'll just move particles in the forward direction
                // In a real implementation, you would move along the actual path curve
                particle.transform.position += particle.transform.forward * speed;
            }
        }
    }

    /// <summary>
    /// Sets the color of the divine line
    /// </summary>
    public void SetLineColor(Color primary, Color secondary)
    {
        lineColor = primary;
        secondaryColor = secondary;
        
        lineRenderer.startColor = primary;
        lineRenderer.endColor = primary;
        
        if (lineMaterial != null)
        {
            lineMaterial.SetColor("_Color", primary);
            lineMaterial.SetColor("_SecondaryColor", secondary);
        }
    }

    /// <summary>
    /// Clears the line when navigation ends
    /// </summary>
    public void ClearLine()
    {
        lineRenderer.positionCount = 0;
        ClearParticles();
    }

    private void OnDestroy()
    {
        ClearParticles();
    }
} 
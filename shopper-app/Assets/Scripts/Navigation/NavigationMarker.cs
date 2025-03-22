using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class NavigationMarker : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Material markerMaterial;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseScale = 0.2f;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float hoverHeight = 0.5f;
    [SerializeField] private float hoverSpeed = 1f;
    [SerializeField] private float hoverScale = 0.1f;
    
    [Header("Marker Type")]
    [SerializeField] private bool isDestination = false;
    [SerializeField] private bool isWaypoint = false;
    
    private Vector3 initialScale;
    private Vector3 initialPosition;
    private Material instanceMaterial;
    private MeshRenderer meshRenderer;
    private float startTime;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (markerMaterial != null)
        {
            instanceMaterial = new Material(markerMaterial);
            meshRenderer.material = instanceMaterial;
        }
        
        initialScale = transform.localScale;
        initialPosition = transform.position;
        startTime = Time.time;
    }
    
    private void Update()
    {
        // Apply visual effects based on marker type
        if (isDestination)
        {
            ApplyDestinationEffects();
        }
        else if (isWaypoint)
        {
            ApplyWaypointEffects();
        }
    }
    
    private void ApplyDestinationEffects()
    {
        // Pulsing scale effect
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
        transform.localScale = initialScale * pulse;
        
        // Rotation effect
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Hovering effect
        float hover = Mathf.Sin(Time.time * hoverSpeed) * hoverScale;
        transform.position = initialPosition + Vector3.up * (hoverHeight + hover);
        
        // Update material properties
        if (instanceMaterial != null)
        {
            float glow = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            instanceMaterial.SetFloat("_GlowIntensity", glow);
        }
    }
    
    private void ApplyWaypointEffects()
    {
        // Simpler effects for waypoints
        float timeOffset = Time.time - startTime;
        
        // Fade in effect
        if (timeOffset < 1f)
        {
            float alpha = Mathf.Lerp(0f, 1f, timeOffset);
            SetMarkerAlpha(alpha);
        }
        
        // Subtle hover
        float hover = Mathf.Sin(Time.time * hoverSpeed * 0.5f) * (hoverScale * 0.5f);
        transform.position = initialPosition + Vector3.up * (hoverHeight * 0.5f + hover);
        
        // Gentle rotation
        transform.Rotate(Vector3.up, rotationSpeed * 0.5f * Time.deltaTime);
    }
    
    private void SetMarkerAlpha(float alpha)
    {
        if (instanceMaterial != null)
        {
            Color color = instanceMaterial.color;
            color.a = alpha;
            instanceMaterial.color = color;
        }
    }
    
    public void SetAsDestination()
    {
        isDestination = true;
        isWaypoint = false;
        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat("_GlowIntensity", 2f);
        }
    }
    
    public void SetAsWaypoint()
    {
        isDestination = false;
        isWaypoint = true;
        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat("_GlowIntensity", 1f);
        }
        transform.localScale = initialScale * 0.7f; // Make waypoints slightly smaller
    }
    
    private void OnDestroy()
    {
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
    
    // Optional: Add interaction methods
    private void OnMouseDown()
    {
        if (isDestination)
        {
            // Trigger destination reached event or interaction
            Debug.Log("Destination marker clicked");
        }
    }
    
    // Public methods for external control
    public void SetColor(Color color)
    {
        if (instanceMaterial != null)
        {
            instanceMaterial.color = color;
        }
    }
    
    public void SetScale(float scale)
    {
        initialScale = Vector3.one * scale;
        transform.localScale = initialScale;
    }
    
    public void SetHoverHeight(float height)
    {
        hoverHeight = height;
        initialPosition = transform.position;
    }
} 
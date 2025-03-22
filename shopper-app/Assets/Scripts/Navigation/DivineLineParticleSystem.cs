using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Adds particle effects to enhance the Divine Line renderer
/// </summary>
public class DivineLineParticleSystem : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private int maxParticles = 100;
    [SerializeField] private float particleSpacing = 1.0f;
    [SerializeField] private float particleLifetime = 2.0f;
    [SerializeField] private float particleSize = 0.2f;
    [SerializeField] private float particleSpeed = 1.0f;
    [SerializeField] private float particleHeightOffset = 0.1f;
    [SerializeField] private bool usePooling = true;
    
    [Header("Particle Appearance")]
    [SerializeField] private Material particleMaterial;
    [SerializeField] private Gradient particleColorGradient;
    [SerializeField] private AnimationCurve sizeCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 1);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 0);
    [SerializeField] private float colorCycleSpeed = 0.5f;
    
    [Header("Waypoint Effects")]
    [SerializeField] private GameObject waypointParticlePrefab;
    [SerializeField] private float waypointParticleSize = 0.5f;
    [SerializeField] private float waypointPulseSpeed = 1.0f;
    [SerializeField] private float waypointEmitRate = 0.2f;
    
    [Header("Proximity Effects")]
    [SerializeField] private float proximityPulseDistance = 5.0f;
    [SerializeField] private float proximityPulseScale = 1.5f;
    [SerializeField] private float proximityPulseSpeed = 2.0f;
    
    // References
    private DivineLineRenderer divineLineRenderer;
    private ARNavigationManager navigationManager;
    private Camera arCamera;
    
    // Runtime data
    private List<Vector3> pathPoints = new List<Vector3>();
    private List<GameObject> activeParticles = new List<GameObject>();
    private Queue<GameObject> particlePool = new Queue<GameObject>();
    private List<GameObject> waypointEffects = new List<GameObject>();
    private float nextParticleTime = 0;
    private float nextWaypointEmitTime = 0;
    
    private void Awake()
    {
        divineLineRenderer = GetComponent<DivineLineRenderer>();
        navigationManager = FindObjectOfType<ARNavigationManager>();
        arCamera = Camera.main;
        
        // Initialize particle pool
        if (usePooling && particlePrefab != null)
        {
            for (int i = 0; i < maxParticles; i++)
            {
                GameObject particle = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity, transform);
                particle.SetActive(false);
                particlePool.Enqueue(particle);
            }
        }
    }
    
    private void Update()
    {
        if (navigationManager == null || !navigationManager.IsNavigating()) 
        {
            // Clean up particles if navigation is not active
            if (activeParticles.Count > 0)
            {
                ClearParticles();
            }
            return;
        }
        
        // Get the current path points
        UpdatePathPoints();
        
        // Update waypoint effects
        UpdateWaypointEffects();
        
        // Emit particles along the path
        if (Time.time > nextParticleTime)
        {
            EmitParticleAlongPath();
            nextParticleTime = Time.time + 1.0f / particleSpeed;
        }
        
        // Update existing particles
        UpdateParticles();
        
        // Update proximity effects
        UpdateProximityEffects();
    }
    
    private void UpdatePathPoints()
    {
        // In a real implementation, this would get the path from DivineLineRenderer
        if (navigationManager != null && navigationManager.IsNavigating())
        {
            // Get current player position
            Vector3 playerPosition = GetPlayerPosition();
            
            // Get destination
            Vector3 destination = navigationManager.GetDestination();
            
            // For this example, we'll create a simple path from player to destination
            pathPoints.Clear();
            pathPoints.Add(playerPosition);
            
            // Add intermediate points including waypoints from the navigation manager
            Vector3 nextWaypoint = navigationManager.GetNextWaypoint();
            
            if (nextWaypoint != Vector3.zero && nextWaypoint != destination)
            {
                // Add the next waypoint
                pathPoints.Add(nextWaypoint);
                
                // Add additional points between waypoint and destination
                Vector3 direction = (destination - nextWaypoint).normalized;
                float distance = Vector3.Distance(nextWaypoint, destination);
                int segments = Mathf.CeilToInt(distance / particleSpacing);
                
                for (int i = 1; i < segments; i++)
                {
                    float t = i / (float)segments;
                    Vector3 point = Vector3.Lerp(nextWaypoint, destination, t);
                    pathPoints.Add(point);
                }
            }
            else
            {
                // Direct path to destination
                Vector3 direction = (destination - playerPosition).normalized;
                float distance = Vector3.Distance(playerPosition, destination);
                int segments = Mathf.CeilToInt(distance / particleSpacing);
                
                for (int i = 1; i < segments; i++)
                {
                    float t = i / (float)segments;
                    Vector3 point = Vector3.Lerp(playerPosition, destination, t);
                    pathPoints.Add(point);
                }
            }
            
            pathPoints.Add(destination);
        }
    }
    
    private void EmitParticleAlongPath()
    {
        if (pathPoints.Count < 2 || particlePrefab == null) return;
        
        // Choose a random segment of the path
        int segmentIndex = Random.Range(0, pathPoints.Count - 1);
        Vector3 start = pathPoints[segmentIndex];
        Vector3 end = pathPoints[segmentIndex + 1];
        
        // Choose a random point along the segment
        float t = Random.value;
        Vector3 position = Vector3.Lerp(start, end, t);
        position.y += particleHeightOffset;
        
        // Get a particle from the pool or create a new one
        GameObject particle;
        if (usePooling && particlePool.Count > 0)
        {
            particle = particlePool.Dequeue();
            particle.transform.position = position;
            particle.SetActive(true);
        }
        else
        {
            particle = Instantiate(particlePrefab, position, Quaternion.identity, transform);
        }
        
        // Configure the particle
        ParticleSystem particleSystem = particle.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startLifetime = particleLifetime;
            main.startSize = particleSize;
            
            // Set color based on position along the path
            float pathPosition = (segmentIndex + t) / (pathPoints.Count - 1);
            Color color = particleColorGradient.Evaluate(pathPosition);
            main.startColor = color;
            
            particleSystem.Play();
        }
        else
        {
            // If it's not a particle system, handle it as a regular GameObject
            // Set up a coroutine to animate and then return it to the pool
            StartCoroutine(AnimateParticle(particle, position, particleLifetime));
        }
        
        // Track active particles
        activeParticles.Add(particle);
        
        // Remove excess particles if we've exceeded the max
        while (activeParticles.Count > maxParticles)
        {
            GameObject oldestParticle = activeParticles[0];
            activeParticles.RemoveAt(0);
            
            if (usePooling)
            {
                oldestParticle.SetActive(false);
                particlePool.Enqueue(oldestParticle);
            }
            else
            {
                Destroy(oldestParticle);
            }
        }
    }
    
    private void UpdateParticles()
    {
        // Update existing particles (for non-ParticleSystem based particles)
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            GameObject particle = activeParticles[i];
            if (particle == null || !particle.activeInHierarchy)
            {
                activeParticles.RemoveAt(i);
                continue;
            }
            
            // For non-ParticleSystem particles, we handle animation in the AnimateParticle coroutine
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();
            if (ps != null && !ps.IsAlive(true))
            {
                // Return to pool if using pooling
                if (usePooling)
                {
                    particle.SetActive(false);
                    particlePool.Enqueue(particle);
                }
                else
                {
                    Destroy(particle);
                }
                activeParticles.RemoveAt(i);
            }
        }
    }
    
    private void UpdateWaypointEffects()
    {
        // Only update if we have waypoint prefabs
        if (waypointParticlePrefab == null) return;
        
        // Emit new waypoint effect
        if (Time.time > nextWaypointEmitTime && pathPoints.Count > 0)
        {
            // Create waypoint effect at next waypoint
            Vector3 waypointPosition = navigationManager.GetNextWaypoint();
            if (waypointPosition != Vector3.zero)
            {
                waypointPosition.y += particleHeightOffset;
                
                GameObject waypointEffect = Instantiate(waypointParticlePrefab, waypointPosition, Quaternion.identity, transform);
                ParticleSystem ps = waypointEffect.GetComponent<ParticleSystem>();
                
                if (ps != null)
                {
                    var main = ps.main;
                    main.startSize = waypointParticleSize;
                    ps.Play();
                }
                
                waypointEffects.Add(waypointEffect);
                nextWaypointEmitTime = Time.time + 1.0f / waypointEmitRate;
            }
        }
        
        // Clean up old waypoint effects
        for (int i = waypointEffects.Count - 1; i >= 0; i--)
        {
            GameObject effect = waypointEffects[i];
            if (effect == null)
            {
                waypointEffects.RemoveAt(i);
                continue;
            }
            
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null && !ps.IsAlive(true))
            {
                Destroy(effect);
                waypointEffects.RemoveAt(i);
            }
        }
    }
    
    private void UpdateProximityEffects()
    {
        if (pathPoints.Count < 2) return;
        
        // Get player position
        Vector3 playerPosition = GetPlayerPosition();
        
        // Check distance to destination
        Vector3 destination = pathPoints[pathPoints.Count - 1];
        float distanceToDestination = Vector3.Distance(playerPosition, destination);
        
        // Increase particle activity as player gets closer to destination
        if (distanceToDestination < proximityPulseDistance)
        {
            // Calculate pulse scale factor based on distance
            float distanceFactor = 1.0f - (distanceToDestination / proximityPulseDistance);
            float pulseScale = 1.0f + (distanceFactor * proximityPulseScale);
            
            // Increase pulse speed
            float pulseSpeedFactor = 1.0f + (distanceFactor * proximityPulseSpeed);
            
            // Apply to all active particles
            foreach (GameObject particle in activeParticles)
            {
                if (particle == null) continue;
                
                ParticleSystem ps = particle.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var main = ps.main;
                    main.startSize = particleSize * pulseScale;
                    main.simulationSpeed = pulseSpeedFactor;
                }
                else
                {
                    // For non-ParticleSystem objects
                    particle.transform.localScale = Vector3.one * particleSize * pulseScale;
                }
            }
        }
    }
    
    private Vector3 GetPlayerPosition()
    {
        if (arCamera != null)
        {
            Vector3 position = arCamera.transform.position;
            position.y = 0; // Keep at ground level
            return position;
        }
        return Vector3.zero;
    }
    
    private IEnumerator AnimateParticle(GameObject particle, Vector3 position, float lifetime)
    {
        float startTime = Time.time;
        float endTime = startTime + lifetime;
        
        // Save original scale
        Vector3 originalScale = particle.transform.localScale;
        
        // Apply random rotation
        particle.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        // Get renderer if exists
        Renderer renderer = particle.GetComponent<Renderer>();
        Material instanceMaterial = null;
        
        if (renderer != null && particleMaterial != null)
        {
            // Create instance of material
            instanceMaterial = new Material(particleMaterial);
            renderer.material = instanceMaterial;
        }
        
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / lifetime;
            
            // Scale based on curve
            float sizeMultiplier = sizeCurve.Evaluate(t);
            particle.transform.localScale = originalScale * sizeMultiplier;
            
            // Alpha based on curve
            float alpha = alphaCurve.Evaluate(t);
            
            // Color cycling
            float colorTime = (Time.time * colorCycleSpeed) % 1.0f;
            Color color = particleColorGradient.Evaluate(colorTime);
            
            if (instanceMaterial != null)
            {
                // Apply color with alpha
                Color finalColor = new Color(color.r, color.g, color.b, alpha);
                instanceMaterial.color = finalColor;
                
                // Optionally add glow or emission
                if (instanceMaterial.HasProperty("_EmissionColor"))
                {
                    instanceMaterial.SetColor("_EmissionColor", color * 2.0f);
                }
            }
            
            yield return null;
        }
        
        // Clean up
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
        
        if (usePooling)
        {
            particle.SetActive(false);
            particlePool.Enqueue(particle);
            
            if (activeParticles.Contains(particle))
            {
                activeParticles.Remove(particle);
            }
        }
        else
        {
            Destroy(particle);
        }
    }
    
    private void ClearParticles()
    {
        // Clear all active particles
        foreach (GameObject particle in activeParticles)
        {
            if (particle == null) continue;
            
            if (usePooling)
            {
                particle.SetActive(false);
                particlePool.Enqueue(particle);
            }
            else
            {
                Destroy(particle);
            }
        }
        activeParticles.Clear();
        
        // Clear waypoint effects
        foreach (GameObject effect in waypointEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        waypointEffects.Clear();
    }
    
    private void OnDestroy()
    {
        ClearParticles();
        
        // Clear particle pool
        if (usePooling)
        {
            while (particlePool.Count > 0)
            {
                GameObject particle = particlePool.Dequeue();
                if (particle != null)
                {
                    Destroy(particle);
                }
            }
        }
    }
} 
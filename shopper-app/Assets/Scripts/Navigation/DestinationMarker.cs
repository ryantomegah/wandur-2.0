using UnityEngine;
using System.Collections;

namespace Wandur.Navigation
{
    /// <summary>
    /// Creates a visually appealing destination marker with animated effects
    /// </summary>
    public class DestinationMarker : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private Color markerColor = new Color(0.5f, 0.8f, 1.0f, 0.8f);
        [SerializeField] private Material markerMaterial;
        [SerializeField] private float baseSize = 1.0f;
        [SerializeField] private float heightOffset = 0.05f;
        [SerializeField] private Gradient colorGradient;
        
        [Header("Primary Ring")]
        [SerializeField] private float ringWidth = 0.1f;
        [SerializeField] private float pulseSpeed = 1.0f;
        [SerializeField] private float pulseScale = 0.3f;
        [SerializeField] private float rotationSpeed = 30.0f;
        [SerializeField] private GameObject ringPrefab;
        
        [Header("Secondary Effects")]
        [SerializeField] private int beamCount = 3;
        [SerializeField] private float beamHeight = 2.0f;
        [SerializeField] private float beamWidth = 0.1f;
        [SerializeField] private float beamPulseSpeed = 1.5f;
        [SerializeField] private float beamFadeSpeed = 0.8f;
        [SerializeField] private bool useVerticalBeams = true;
        
        [Header("Particles")]
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] private float particleRate = 5.0f;
        [SerializeField] private float particleSize = 0.2f;
        [SerializeField] private float particleLifetime = 2.0f;
        [SerializeField] private float particleRiseSpeed = 0.5f;
        [SerializeField] private bool useParticles = true;
        
        // Private variables
        private Transform markerTransform;
        private Transform ringTransform;
        private GameObject[] beams;
        private ParticleSystem particles;
        private bool isInitialized = false;
        private float colorCycleTime = 0f;
        private Vector3 groundPosition;
        
        // Animation state
        private float currentPulseValue = 0f;
        private float currentBeamValue = 0f;
        
        public void Initialize(Vector3 position, Transform parent = null)
        {
            if (isInitialized)
            {
                // Just update position if already initialized
                groundPosition = position;
                Vector3 adjustedPosition = groundPosition + new Vector3(0, heightOffset, 0);
                transform.position = adjustedPosition;
                return;
            }
            
            // Store the ground position
            groundPosition = position;
            
            // Set position with height offset
            Vector3 adjustedPosition = groundPosition + new Vector3(0, heightOffset, 0);
            transform.position = adjustedPosition;
            
            // Set parent if provided
            if (parent != null)
            {
                transform.SetParent(parent, true);
            }
            
            // Create ring
            CreateRing();
            
            // Create beams
            if (useVerticalBeams)
            {
                CreateBeams();
            }
            
            // Create particles
            if (useParticles && particlePrefab != null)
            {
                CreateParticles();
            }
            
            isInitialized = true;
            
            // Start animations
            StartCoroutine(AnimateMarker());
        }
        
        private void CreateRing()
        {
            if (ringPrefab != null)
            {
                GameObject ring = Instantiate(ringPrefab, transform.position, Quaternion.Euler(90, 0, 0), transform);
                ringTransform = ring.transform;
                ringTransform.localScale = new Vector3(baseSize, baseSize, 1);
                
                // Apply material
                Renderer renderer = ring.GetComponent<Renderer>();
                if (renderer != null && markerMaterial != null)
                {
                    renderer.material = new Material(markerMaterial);
                    renderer.material.SetColor("_EmissionColor", markerColor);
                    renderer.material.color = markerColor;
                }
            }
            else
            {
                // Create a simple ring if no prefab
                GameObject ring = new GameObject("Ring");
                ring.transform.SetParent(transform, false);
                LineRenderer lineRenderer = ring.AddComponent<LineRenderer>();
                
                // Configure line renderer
                lineRenderer.useWorldSpace = false;
                lineRenderer.startWidth = ringWidth;
                lineRenderer.endWidth = ringWidth;
                lineRenderer.loop = true;
                
                if (markerMaterial != null)
                {
                    lineRenderer.material = new Material(markerMaterial);
                }
                else
                {
                    // Default material
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                }
                
                lineRenderer.startColor = markerColor;
                lineRenderer.endColor = markerColor;
                
                // Create ring points
                int segments = 36;
                lineRenderer.positionCount = segments;
                
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * 2 * Mathf.PI / segments;
                    float x = Mathf.Cos(angle) * baseSize;
                    float z = Mathf.Sin(angle) * baseSize;
                    lineRenderer.SetPosition(i, new Vector3(x, 0, z));
                }
                
                ringTransform = ring.transform;
                
                // Rotate to be flat on the ground
                ringTransform.localRotation = Quaternion.Euler(90, 0, 0);
            }
        }
        
        private void CreateBeams()
        {
            beams = new GameObject[beamCount];
            
            for (int i = 0; i < beamCount; i++)
            {
                GameObject beam = new GameObject("Beam_" + i);
                beam.transform.SetParent(transform, false);
                LineRenderer lineRenderer = beam.AddComponent<LineRenderer>();
                
                // Configure line renderer
                lineRenderer.useWorldSpace = false;
                lineRenderer.startWidth = beamWidth;
                lineRenderer.endWidth = 0f; // Tapered beam
                lineRenderer.positionCount = 2;
                
                if (markerMaterial != null)
                {
                    lineRenderer.material = new Material(markerMaterial);
                }
                else
                {
                    // Default material
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                }
                
                lineRenderer.startColor = markerColor;
                lineRenderer.endColor = new Color(markerColor.r, markerColor.g, markerColor.b, 0f);
                
                // Set beam start and end points
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, new Vector3(0, beamHeight, 0));
                
                // Position beams evenly around the center
                float angle = i * 2 * Mathf.PI / beamCount;
                float radius = baseSize * 0.5f;
                beam.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                
                beams[i] = beam;
            }
        }
        
        private void CreateParticles()
        {
            // Create particles
            GameObject particleObj = Instantiate(particlePrefab, transform.position, Quaternion.identity, transform);
            particles = particleObj.GetComponent<ParticleSystem>();
            
            if (particles != null)
            {
                // Configure particle system
                var main = particles.main;
                main.startLifetime = particleLifetime;
                main.startSize = particleSize;
                main.startColor = markerColor;
                
                // Set emission rate
                var emission = particles.emission;
                emission.rateOverTime = particleRate;
                
                // Configure velocity
                var velocity = particles.velocityOverLifetime;
                velocity.enabled = true;
                velocity.y = particleRiseSpeed;
                
                // Configure color over lifetime
                var colorOverLifetime = particles.colorOverLifetime;
                colorOverLifetime.enabled = true;
                
                if (colorGradient != null)
                {
                    colorOverLifetime.color = colorGradient;
                }
                else
                {
                    // Default fade out
                    Gradient grad = new Gradient();
                    grad.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(markerColor, 0.0f), new GradientColorKey(markerColor, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                    );
                    colorOverLifetime.color = grad;
                }
                
                // Play particle system
                particles.Play();
            }
        }
        
        private IEnumerator AnimateMarker()
        {
            while (true)
            {
                // Update pulse animation
                currentPulseValue = (1 + Mathf.Sin(Time.time * pulseSpeed)) * 0.5f;
                float scaleFactor = 1 + currentPulseValue * pulseScale;
                
                // Apply scale to ring
                if (ringTransform != null)
                {
                    ringTransform.localScale = new Vector3(baseSize * scaleFactor, baseSize * scaleFactor, 1);
                    ringTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
                }
                
                // Animate beams
                if (beams != null)
                {
                    currentBeamValue = (1 + Mathf.Sin(Time.time * beamPulseSpeed)) * 0.5f;
                    
                    for (int i = 0; i < beams.Length; i++)
                    {
                        if (beams[i] != null)
                        {
                            LineRenderer lineRenderer = beams[i].GetComponent<LineRenderer>();
                            if (lineRenderer != null)
                            {
                                // Pulse the height
                                float height = beamHeight * (0.5f + currentBeamValue * 0.5f);
                                lineRenderer.SetPosition(1, new Vector3(0, height, 0));
                                
                                // Cycle colors
                                if (colorGradient != null)
                                {
                                    colorCycleTime += Time.deltaTime * beamFadeSpeed;
                                    float colorT = (colorCycleTime + i * 0.3f) % 1f;
                                    Color color = colorGradient.Evaluate(colorT);
                                    lineRenderer.startColor = color;
                                    lineRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
                                }
                            }
                        }
                    }
                }
                
                yield return null;
            }
        }
        
        public void UpdatePosition(Vector3 position)
        {
            groundPosition = position;
            Vector3 adjustedPosition = groundPosition + new Vector3(0, heightOffset, 0);
            transform.position = adjustedPosition;
        }
        
        public void SetColor(Color color)
        {
            markerColor = color;
            
            // Update ring color
            if (ringTransform != null)
            {
                Renderer renderer = ringTransform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.SetColor("_EmissionColor", markerColor);
                    renderer.material.color = markerColor;
                }
                
                LineRenderer lineRenderer = ringTransform.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderer.startColor = markerColor;
                    lineRenderer.endColor = markerColor;
                }
            }
            
            // Update beam colors
            if (beams != null)
            {
                foreach (GameObject beam in beams)
                {
                    if (beam != null)
                    {
                        LineRenderer lineRenderer = beam.GetComponent<LineRenderer>();
                        if (lineRenderer != null)
                        {
                            lineRenderer.startColor = markerColor;
                            lineRenderer.endColor = new Color(markerColor.r, markerColor.g, markerColor.b, 0f);
                        }
                    }
                }
            }
            
            // Update particle color
            if (particles != null)
            {
                var main = particles.main;
                main.startColor = markerColor;
            }
        }
        
        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
} 
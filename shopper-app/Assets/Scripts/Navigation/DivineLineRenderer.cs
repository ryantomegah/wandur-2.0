using UnityEngine;
using System.Collections.Generic;

namespace Wandur.Navigation
{
    [RequireComponent(typeof(LineRenderer))]
    public class DivineLineRenderer : MonoBehaviour
    {
        [Header("Line Settings")]
        [SerializeField] private Material divineLineMaterial;
        [SerializeField] private float lineWidth = 0.15f;
        [SerializeField] private float lineHeight = 0.1f; // Height above ground
        [SerializeField] private float yOffset = 0.05f;   // Small offset to prevent Z-fighting with floor

        [Header("Effect Settings")]
        [SerializeField] private Color mainColor = new Color(0.5f, 0.8f, 1.0f, 0.8f);
        [SerializeField] private Color secondaryColor = new Color(0.3f, 0.6f, 1.0f, 0.6f);
        [SerializeField] private float glowIntensity = 1.5f;
        [SerializeField] private float flowSpeed = 1.0f;
        [SerializeField] private float pulseSpeed = 1.0f;
        [SerializeField] private float pulseScale = 0.2f;
        [SerializeField] private float edgeSoftness = 0.1f;
        
        [Header("Enhanced Effects")]
        [SerializeField] private Texture2D noiseTexture;
        [SerializeField] private float noiseScale = 2.0f;
        [SerializeField] private float noiseStrength = 0.2f;
        [SerializeField] private float shimmerSpeed = 2.0f;
        [SerializeField] private float shimmerStrength = 0.3f;
        [SerializeField] private float proximityGlow = 1.0f;
        [SerializeField] private bool responsiveGlow = true;
        
        [Header("Particle System")]
        [SerializeField] private bool useParticles = true;
        [SerializeField] private GameObject particleSystemPrefab;
        [SerializeField] private float particleSpawnInterval = 0.3f;
        
        // Private variables
        private LineRenderer lineRenderer;
        private List<Vector3> waypoints = new List<Vector3>();
        private DivineLineParticleSystem particleSystem;
        private float particleTimer = 0f;
        private float totalPathLength = 0f;
        private Transform destination;
        private float distanceToDestination = 0f;
        private MaterialPropertyBlock propBlock;
        
        private int mainColorID;
        private int secondaryColorID;
        private int glowIntensityID;
        private int flowSpeedID;
        private int pulseSpeedID;
        private int pulseScaleID;
        private int edgeSoftnessID;
        private int pathLengthID;
        private int detailNoiseTexID;
        private int noiseScaleID;
        private int noiseStrengthID;
        private int shimmerSpeedID;
        private int shimmerStrengthID;
        private int proximityGlowID;
        private int distanceFromEndID;
        private int responsiveGlowID;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            
            if (divineLineMaterial != null)
            {
                lineRenderer.material = divineLineMaterial;
            }
            
            // Create material property block for efficient updates
            propBlock = new MaterialPropertyBlock();
            
            // Cache shader property IDs
            mainColorID = Shader.PropertyToID("_MainColor");
            secondaryColorID = Shader.PropertyToID("_SecondaryColor");
            glowIntensityID = Shader.PropertyToID("_GlowIntensity");
            flowSpeedID = Shader.PropertyToID("_FlowSpeed");
            pulseSpeedID = Shader.PropertyToID("_PulseSpeed");
            pulseScaleID = Shader.PropertyToID("_PulseScale");
            edgeSoftnessID = Shader.PropertyToID("_EdgeSoftness");
            pathLengthID = Shader.PropertyToID("_PathLength");
            detailNoiseTexID = Shader.PropertyToID("_DetailNoiseTex");
            noiseScaleID = Shader.PropertyToID("_NoiseScale");
            noiseStrengthID = Shader.PropertyToID("_NoiseStrength");
            shimmerSpeedID = Shader.PropertyToID("_ShimmerSpeed");
            shimmerStrengthID = Shader.PropertyToID("_ShimmerStrength");
            proximityGlowID = Shader.PropertyToID("_ProximityGlow");
            distanceFromEndID = Shader.PropertyToID("_DistanceFromEnd");
            responsiveGlowID = Shader.PropertyToID("_ResponsiveGlow");
            
            // Initialize line settings
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 0;
            
            // Initialize the particle system if enabled
            if (useParticles && particleSystemPrefab != null)
            {
                var particleSystemObj = Instantiate(particleSystemPrefab, transform);
                particleSystem = particleSystemObj.GetComponent<DivineLineParticleSystem>();
                if (particleSystem == null)
                {
                    Debug.LogWarning("Particle system prefab does not contain a DivineLineParticleSystem component.");
                }
            }
            
            UpdateMaterialProperties();
        }

        private void Update()
        {
            // Update distance to destination for responsive glow
            if (responsiveGlow && destination != null)
            {
                Vector3 playerPos = Camera.main.transform.position;
                distanceToDestination = Vector3.Distance(playerPos, destination.position);
                lineRenderer.GetPropertyBlock(propBlock);
                propBlock.SetFloat(distanceFromEndID, distanceToDestination);
                lineRenderer.SetPropertyBlock(propBlock);
            }
            
            // Handle particles
            if (useParticles && particleSystem != null && waypoints.Count > 1)
            {
                particleTimer += Time.deltaTime;
                if (particleTimer >= particleSpawnInterval)
                {
                    particleSystem.EmitParticles();
                    particleTimer = 0f;
                }
            }
        }
        
        public void SetWaypoints(List<Vector3> newWaypoints, Transform destinationTransform = null)
        {
            waypoints.Clear();
            
            if (newWaypoints == null || newWaypoints.Count == 0)
            {
                lineRenderer.positionCount = 0;
                return;
            }
            
            // Store waypoints
            foreach (var point in newWaypoints)
            {
                // Apply height offset to keep line above ground
                Vector3 adjustedPoint = point + new Vector3(0, lineHeight + yOffset, 0);
                waypoints.Add(adjustedPoint);
            }
            
            // Set line renderer positions
            lineRenderer.positionCount = waypoints.Count;
            lineRenderer.SetPositions(waypoints.ToArray());
            
            // Calculate total path length for flow effect scaling
            CalculatePathLength();
            
            // Store destination transform for distance calculations
            destination = destinationTransform;
            if (destination != null)
            {
                distanceToDestination = Vector3.Distance(
                    Camera.main != null ? Camera.main.transform.position : transform.position, 
                    destination.position
                );
            }
            
            // Update material properties
            UpdateMaterialProperties();
            
            // Update particle system
            if (useParticles && particleSystem != null)
            {
                particleSystem.UpdatePathPoints(waypoints);
            }
        }
        
        private void CalculatePathLength()
        {
            totalPathLength = 0f;
            for (int i = 1; i < waypoints.Count; i++)
            {
                totalPathLength += Vector3.Distance(waypoints[i-1], waypoints[i]);
            }
            
            // Normalize to a reasonable range for shader
            totalPathLength = Mathf.Clamp(totalPathLength * 0.1f, 1f, 100f);
        }
        
        private void UpdateMaterialProperties()
        {
            lineRenderer.GetPropertyBlock(propBlock);
            
            propBlock.SetColor(mainColorID, mainColor);
            propBlock.SetColor(secondaryColorID, secondaryColor);
            propBlock.SetFloat(glowIntensityID, glowIntensity);
            propBlock.SetFloat(flowSpeedID, flowSpeed);
            propBlock.SetFloat(pulseSpeedID, pulseSpeed);
            propBlock.SetFloat(pulseScaleID, pulseScale);
            propBlock.SetFloat(edgeSoftnessID, edgeSoftness);
            propBlock.SetFloat(pathLengthID, totalPathLength);
            
            // Enhanced properties
            if (noiseTexture != null)
            {
                propBlock.SetTexture(detailNoiseTexID, noiseTexture);
            }
            propBlock.SetFloat(noiseScaleID, noiseScale);
            propBlock.SetFloat(noiseStrengthID, noiseStrength);
            propBlock.SetFloat(shimmerSpeedID, shimmerSpeed);
            propBlock.SetFloat(shimmerStrengthID, shimmerStrength);
            propBlock.SetFloat(proximityGlowID, proximityGlow);
            propBlock.SetFloat(distanceFromEndID, distanceToDestination);
            propBlock.SetFloat(responsiveGlowID, responsiveGlow ? 1.0f : 0.0f);
            
            lineRenderer.SetPropertyBlock(propBlock);
        }
        
        public void SetColor(Color primary, Color secondary)
        {
            mainColor = primary;
            secondaryColor = secondary;
            UpdateMaterialProperties();
        }
        
        public void SetGlowIntensity(float intensity)
        {
            glowIntensity = intensity;
            UpdateMaterialProperties();
        }
        
        public void Hide()
        {
            lineRenderer.enabled = false;
            if (useParticles && particleSystem != null)
            {
                particleSystem.gameObject.SetActive(false);
            }
        }
        
        public void Show()
        {
            lineRenderer.enabled = true;
            if (useParticles && particleSystem != null)
            {
                particleSystem.gameObject.SetActive(true);
            }
        }
        
        public void OnDestroy()
        {
            // Clean up resources
            if (useParticles && particleSystem != null)
            {
                Destroy(particleSystem.gameObject);
            }
        }
    }
} 
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DivineLineRenderer : MonoBehaviour
{
    [Header("Path Settings")]
    [SerializeField] private float lineWidth = 0.15f;
    [SerializeField] private float lineHeight = 0.05f;
    [SerializeField] private float smoothness = 0.1f;
    [SerializeField] private bool useWorldSpace = true;

    [Header("Visual Effects")]
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseScale = 0.2f;
    [SerializeField] private float flowSpeed = 1f;
    [SerializeField] private float glowIntensity = 1.5f;

    private Material lineMaterial;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private List<Vector3> pathPoints = new List<Vector3>();
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<Vector3> normals = new List<Vector3>();
    private float totalPathLength;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Initialize(Material material, float width, float height, Color color)
    {
        lineMaterial = new Material(material);
        lineMaterial.color = color;
        meshRenderer.material = lineMaterial;
        lineWidth = width;
        lineHeight = height;

        // Set shader properties
        lineMaterial.SetFloat("_GlowIntensity", glowIntensity);
        lineMaterial.SetFloat("_PulseSpeed", pulseSpeed);
        lineMaterial.SetFloat("_PulseScale", pulseScale);
        lineMaterial.SetFloat("_FlowSpeed", flowSpeed);
    }

    public void UpdatePath(Vector3[] points)
    {
        if (points == null || points.Length < 2) return;

        pathPoints.Clear();
        
        // Generate smooth path using Catmull-Rom spline
        if (points.Length > 3)
        {
            for (float t = 0; t <= 1; t += smoothness)
            {
                pathPoints.Add(CatmullRomSpline(points, t));
            }
        }
        else
        {
            pathPoints.AddRange(points);
        }

        GenerateMesh();
        UpdateMaterialProperties();
    }

    public void ClearPath()
    {
        pathPoints.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        normals.Clear();

        if (meshFilter.mesh != null)
        {
            meshFilter.mesh.Clear();
        }
    }

    private void GenerateMesh()
    {
        if (pathPoints.Count < 2) return;

        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        normals.Clear();
        totalPathLength = 0;

        // Calculate total path length for UV mapping
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            totalPathLength += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }

        float currentLength = 0;
        Vector3 lastRight = Vector3.zero;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 forward;
            if (i < pathPoints.Count - 1)
            {
                forward = (pathPoints[i + 1] - pathPoints[i]).normalized;
            }
            else
            {
                forward = (pathPoints[i] - pathPoints[i - 1]).normalized;
            }

            // Calculate right vector
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(forward, up).normalized;

            // Handle path twisting
            if (i > 0)
            {
                float dot = Vector3.Dot(right, lastRight);
                if (dot < 0)
                {
                    right = -right;
                }
            }
            lastRight = right;

            // Create vertices
            Vector3 position = pathPoints[i];
            vertices.Add(position + right * lineWidth * 0.5f + up * lineHeight); // Top right
            vertices.Add(position - right * lineWidth * 0.5f + up * lineHeight); // Top left
            vertices.Add(position + right * lineWidth * 0.5f); // Bottom right
            vertices.Add(position - right * lineWidth * 0.5f); // Bottom left

            // Calculate UV coordinates
            if (i > 0)
            {
                currentLength += Vector3.Distance(pathPoints[i], pathPoints[i - 1]);
            }
            float uvY = currentLength / totalPathLength;

            uvs.Add(new Vector2(1, uvY)); // Top right
            uvs.Add(new Vector2(0, uvY)); // Top left
            uvs.Add(new Vector2(1, uvY)); // Bottom right
            uvs.Add(new Vector2(0, uvY)); // Bottom left

            // Add normal vectors
            Vector3 normal = Vector3.Cross(right, forward).normalized;
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);

            // Create triangles
            if (i < pathPoints.Count - 1)
            {
                int baseIndex = i * 4;
                // Top face
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 4);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 4);
                triangles.Add(baseIndex + 5);

                // Right face
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 4);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 6);
                triangles.Add(baseIndex + 4);

                // Left face
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 5);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 5);
                triangles.Add(baseIndex + 7);

                // Bottom face
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 6);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 7);
                triangles.Add(baseIndex + 6);
            }
        }

        // Update mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private void UpdateMaterialProperties()
    {
        if (lineMaterial != null)
        {
            lineMaterial.SetFloat("_PathLength", totalPathLength);
            lineMaterial.SetFloat("_Time", Time.time);
        }
    }

    private Vector3 CatmullRomSpline(Vector3[] points, float t)
    {
        int numSections = points.Length - 3;
        int currentSection = Mathf.Min(Mathf.FloorToInt(t * numSections), numSections - 1);
        float u = t * numSections - currentSection;

        Vector3 p0 = points[currentSection];
        Vector3 p1 = points[currentSection + 1];
        Vector3 p2 = points[currentSection + 2];
        Vector3 p3 = points[currentSection + 3];

        return 0.5f * (
            (-p0 + 3f * p1 - 3f * p2 + p3) * (u * u * u) +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (u * u) +
            (-p0 + p2) * u +
            2f * p1
        );
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
        }
    }
} 
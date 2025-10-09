using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways] // Works in editor and runtime
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GridMesh : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize = 10;     // number of cells per side
    public float cellSize = 1f;   // size of each cell
    public Color gridColor = Color.white;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void OnValidate()
    {
        // Regenerate mesh whenever settings change in editor
        GenerateGrid();
    }

    void Awake()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

        var mesh = new Mesh();
        mesh.name = "GridMesh";

        var vertices = new List<Vector3>();
        var indices = new List<int>();

        // Vertical lines (along Z axis)
        for (int x = 0; x <= gridSize; x++)
        {
            float xPos = x * cellSize;
            vertices.Add(new Vector3(xPos, 0, 0));
            vertices.Add(new Vector3(xPos, 0, gridSize * cellSize));

            indices.Add(vertices.Count - 2);
            indices.Add(vertices.Count - 1);
        }

        // Horizontal lines (along X axis)
        for (int z = 0; z <= gridSize; z++)
        {
            float zPos = z * cellSize;
            vertices.Add(new Vector3(0, 0, zPos));
            vertices.Add(new Vector3(gridSize * cellSize, 0, zPos));

            indices.Add(vertices.Count - 2);
            indices.Add(vertices.Count - 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        // Material
        if (meshRenderer.sharedMaterial == null)
        {
            meshRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        meshRenderer.sharedMaterial.color = gridColor;
    }
}

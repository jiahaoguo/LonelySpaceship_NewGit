using UnityEngine;

[ExecuteAlways] // Works in edit mode and play mode
public class GridDrawer : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSizeX = 10;
    public int gridSizeZ = 10;
    public float cellSize = 1f;
    public Color gridColor = Color.white;

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;

        Vector3 origin = transform.position;

        // Draw lines along X
        for (int x = 0; x <= gridSizeX; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = start + new Vector3(0, 0, gridSizeZ * cellSize);
            Gizmos.DrawLine(start, end);
        }

        // Draw lines along Z
        for (int z = 0; z <= gridSizeZ; z++)
        {
            Vector3 start = origin + new Vector3(0, 0, z * cellSize);
            Vector3 end = start + new Vector3(gridSizeX * cellSize, 0, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    private void OnRenderObject()
    {
        // Runtime draw in Game view
        GL.PushMatrix();
        Material lineMaterial = GetLineMaterial();
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);
        GL.Color(gridColor);

        Vector3 origin = transform.position;

        // Lines along X
        for (int x = 0; x <= gridSizeX; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 end = start + new Vector3(0, 0, gridSizeZ * cellSize);
            GL.Vertex(start);
            GL.Vertex(end);
        }

        // Lines along Z
        for (int z = 0; z <= gridSizeZ; z++)
        {
            Vector3 start = origin + new Vector3(0, 0, z * cellSize);
            Vector3 end = start + new Vector3(gridSizeX * cellSize, 0, 0);
            GL.Vertex(start);
            GL.Vertex(end);
        }

        GL.End();
        GL.PopMatrix();
    }

    // Simple built-in line material
    static Material lineMat;
    static Material GetLineMaterial()
    {
        if (!lineMat)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMat = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            lineMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMat.SetInt("_ZWrite", 0);
        }
        return lineMat;
    }
}

using UnityEngine;

public class ScreenCross : MonoBehaviour
{
    [Header("Cross Settings")]
    public Color crossColor = Color.red;
    public float thickness = 2f;
    public float length = 20f; // Half-length of each line

    private Texture2D lineTex;

    void Awake()
    {
        lineTex = new Texture2D(1, 1);
        lineTex.SetPixel(0, 0, Color.white);
        lineTex.Apply();
    }

    void OnGUI()
    {
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        GUI.color = crossColor;

        // Horizontal line
        GUI.DrawTexture(new Rect(center.x - length, center.y - (thickness / 2f), length * 2, thickness), lineTex);

        // Vertical line
        GUI.DrawTexture(new Rect(center.x - (thickness / 2f), center.y - length, thickness, length * 2), lineTex);

        GUI.color = Color.white;
    }
}

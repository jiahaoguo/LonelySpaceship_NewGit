using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ParentSelectTool
{
    private static bool enableParentSelect = false;
    private static GUIStyle toggleButtonStyle;

    static ParentSelectTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    // Menu entry to toggle the tool
    [MenuItem("JiahaoTools/Selection/Parent Select Tool")]
    private static void ToggleTool()
    {
        enableParentSelect = !enableParentSelect;
        SceneView.RepaintAll();
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();

        if (toggleButtonStyle == null)
        {
            toggleButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal = { textColor = Color.white }
            };
        }

        GUILayout.BeginArea(new Rect(10, 10, 160, 40));

        Color prevColor = GUI.color;
        GUI.color = enableParentSelect ? Color.green : Color.gray;

        if (GUILayout.Button(enableParentSelect ? "Parent Select: ON" : "Parent Select: OFF", toggleButtonStyle))
        {
            enableParentSelect = !enableParentSelect;
            SceneView.RepaintAll();
        }

        GUI.color = prevColor;
        GUILayout.EndArea();

        Handles.EndGUI();

        if (!enableParentSelect)
            return;

        // Detect if the selected object should be replaced by its parent
        Event e = Event.current;
        if (e.type == EventType.Layout)
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                bool hasRenderer = selected.GetComponent<MeshRenderer>() != null;
                bool hasMeaningfulComponents = selected.GetComponents<Component>().Length > 2; // Transform + MeshRenderer + (maybe MeshFilter)
                bool hasParent = selected.transform.parent != null;

                if (hasRenderer && !hasMeaningfulComponents && hasParent)
                {
                    Selection.activeGameObject = selected.transform.parent.gameObject;
                }
            }
        }
    }
}

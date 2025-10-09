using UnityEngine;
using UnityEditor;
using System;

public class ObjectPlacerWindow : EditorWindow
{
    [Header("Placement Settings")]
    public GameObject objectToPlace;       // Prefab to spawn
    public Transform parentTransform;      // Parent under which objects go
    private bool placementMode = false;    // Toggle placement mode

    [MenuItem("JiahaoTools/Object Placer")]
    public static void ShowWindow()
    {
        GetWindow<ObjectPlacerWindow>("Object Placer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Placement Settings", EditorStyles.boldLabel);

        objectToPlace = (GameObject)EditorGUILayout.ObjectField("Object to Place", objectToPlace, typeof(GameObject), false);
        parentTransform = (Transform)EditorGUILayout.ObjectField("Parent Transform", parentTransform, typeof(Transform), true);

        GUILayout.Space(10);

        // Colored toggle button
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontStyle = FontStyle.Bold;

        Color prevColor = GUI.backgroundColor;
        GUI.backgroundColor = placementMode ? Color.green : Color.red;

        if (GUILayout.Button(placementMode ? "Placement Mode: ON" : "Placement Mode: OFF", buttonStyle, GUILayout.Height(30)))
        {
            placementMode = !placementMode;
        }

        GUI.backgroundColor = prevColor;

        if (placementMode)
        {
            EditorGUILayout.HelpBox("Click in Scene View to place objects.\n(ALT+Click = orbit camera, won’t place)", MessageType.Info);
        }
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!placementMode || objectToPlace == null) return;

        Event e = Event.current;

        // Left mouse click in Scene view
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(objectToPlace);

                if (parentTransform != null)
                    newObj.transform.SetParent(parentTransform);

                newObj.transform.position = hit.point;
                newObj.transform.rotation = Quaternion.identity;

                Undo.RegisterCreatedObjectUndo(newObj, "Placed Object");

                e.Use(); // consume event
            }
        }
    }
}

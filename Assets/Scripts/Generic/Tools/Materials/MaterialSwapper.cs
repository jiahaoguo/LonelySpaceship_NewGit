using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MaterialSwapper : EditorWindow
{
    private GameObject targetObject;
    private Material targetMaterial;

    private bool showExcludedObjects = false;
    private List<GameObject> excludedObjects = new List<GameObject>();

    [MenuItem("JiahaoTools/Material Swapper")]
    public static void ShowWindow()
    {
        GetWindow<MaterialSwapper>("Material Swapper");
    }

    private void OnGUI()
    {
        GUILayout.Label("Swap Materials Recursively", EditorStyles.boldLabel);

        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
        targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", targetMaterial, typeof(Material), false);

        EditorGUILayout.Space();

        // Collapsible Excluded Objects list
        showExcludedObjects = EditorGUILayout.Foldout(showExcludedObjects, "Excluded Objects", true);
        if (showExcludedObjects)
        {
            EditorGUI.indentLevel++;
            int removeIndex = -1;

            for (int i = 0; i < excludedObjects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                excludedObjects[i] = (GameObject)EditorGUILayout.ObjectField(excludedObjects[i], typeof(GameObject), true);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                    removeIndex = i;
                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex >= 0)
                excludedObjects.RemoveAt(removeIndex);

            if (GUILayout.Button("+ Add Object"))
                excludedObjects.Add(null);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Swap Materials"))
        {
            if (targetObject == null || targetMaterial == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both a Target Object and a Material.", "OK");
                return;
            }

            SwapMaterials(targetObject, targetMaterial);
            EditorUtility.DisplayDialog("Done",
                $"All MeshRenderers under {targetObject.name} now use {targetMaterial.name} (excluded objects skipped).",
                "OK");
        }
    }

    private void SwapMaterials(GameObject obj, Material mat)
    {
        // Skip if object is in exclusion list or a child of an excluded object
        if (IsExcluded(obj))
            return;

        // Apply to this object if has MeshRenderer
        MeshRenderer mr = obj.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            Undo.RecordObject(mr, "Swap Material");
            var newMats = new Material[mr.sharedMaterials.Length];
            for (int i = 0; i < newMats.Length; i++)
                newMats[i] = mat;

            mr.sharedMaterials = newMats;
            EditorUtility.SetDirty(mr);
        }

        // Recursively apply to children
        foreach (Transform child in obj.transform)
            SwapMaterials(child.gameObject, mat);
    }

    private bool IsExcluded(GameObject obj)
    {
        foreach (var excluded in excludedObjects)
        {
            if (excluded == null)
                continue;

            if (obj == excluded || obj.transform.IsChildOf(excluded.transform))
                return true;
        }
        return false;
    }
}

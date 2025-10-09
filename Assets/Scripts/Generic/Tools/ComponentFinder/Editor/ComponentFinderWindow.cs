using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ComponentFinderWindow : EditorWindow
{
    private string searchQuery = "";
    private Type selectedType;
    private Vector2 scrollPos;
    private List<GameObject> foundObjects = new List<GameObject>();
    private List<Type> allComponentTypes;

    [MenuItem("JiahaoTools/Component Finder")]
    public static void ShowWindow()
    {
        GetWindow<ComponentFinderWindow>("Component Finder");
    }

    void OnEnable()
    {
        // Cache all available Component types
        allComponentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null); }
            })
            .Where(t => typeof(Component).IsAssignableFrom(t) && !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToList();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Search for Component or Script", EditorStyles.boldLabel);

        // Search field
        string newQuery = EditorGUILayout.TextField("Search", searchQuery);
        if (newQuery != searchQuery)
        {
            searchQuery = newQuery;
            selectedType = null;
        }

        // Show filtered type list
        if (!string.IsNullOrEmpty(searchQuery))
        {
            var filtered = allComponentTypes
                .Where(t => t.Name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(20); // Limit results for performance

            foreach (var t in filtered)
            {
                if (GUILayout.Button(t.FullName, EditorStyles.miniButton))
                {
                    selectedType = t;
                    RefreshSearch();
                }
            }
        }

        if (selectedType == null)
        {
            EditorGUILayout.HelpBox("Type a name to search, then select a component from the list.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Found {foundObjects.Count} objects with {selectedType.Name}:", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var go in foundObjects)
        {
            if (GUILayout.Button(go.name, EditorStyles.objectField))
            {
                Selection.activeObject = go;
                EditorGUIUtility.PingObject(go);
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void RefreshSearch()
    {
        foundObjects.Clear();
        if (selectedType == null || !typeof(Component).IsAssignableFrom(selectedType))
            return;

        Component[] allComponents = FindObjectsOfType(selectedType) as Component[];
        foreach (var comp in allComponents)
        {
            if (comp != null)
                foundObjects.Add(comp.gameObject);
        }

        // Auto-highlight in hierarchy
        if (foundObjects.Count > 0)
        {
            Selection.objects = foundObjects.ToArray();
        }
    }
}

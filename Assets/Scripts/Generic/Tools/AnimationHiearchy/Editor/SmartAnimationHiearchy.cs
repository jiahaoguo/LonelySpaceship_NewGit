#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SmartAnimationHierarchy : EditorWindow
{
    private static int columnWidth = 300;

    private Animator animatorObject;
    private AnimationClip selectedClip;
    private List<AnimationClip> animationClips;
    private ArrayList pathsKeys;
    private Hashtable paths;

    private Dictionary<string, string> tempPathOverrides;
    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Window/Jiahao SmartAnimator/Smart Animation Hierarchy")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<SmartAnimationHierarchy>();
    }

    public SmartAnimationHierarchy()
    {
        animationClips = new List<AnimationClip>();
        tempPathOverrides = new Dictionary<string, string>();
    }

    void OnSelectionChange()
    {
        this.Repaint();
    }

    void OnGUI()
    {
        // Drag and drop Animator field
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Referenced Animator (Root):", GUILayout.Width(columnWidth));

        Animator newAnimatorObject = (Animator)EditorGUILayout.ObjectField(
            animatorObject,
            typeof(Animator),
            true,
            GUILayout.Width(columnWidth)
        );

        // If Animator changes, update the animation clips list
        if (newAnimatorObject != animatorObject)
        {
            animatorObject = newAnimatorObject;
            UpdateAnimationClips();
        }

        EditorGUILayout.EndHorizontal();

        if (animatorObject != null && animationClips.Count > 0)
        {
            // Dropdown to select an animation clip
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Animation Clip:", GUILayout.Width(columnWidth));

            int selectedIndex = animationClips.IndexOf(selectedClip);
            selectedIndex = EditorGUILayout.Popup(selectedIndex, animationClips.Select(clip => clip.name).ToArray(), GUILayout.Width(columnWidth));

            // Update selected clip when a new one is chosen
            if (selectedIndex >= 0 && selectedClip != animationClips[selectedIndex])
            {
                selectedClip = animationClips[selectedIndex];
                FillModel();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Fix Missing Paths"))
            {
                FixMissingPaths();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Reference path:", GUILayout.Width(columnWidth));
            GUILayout.Label("Animated properties:", GUILayout.Width(columnWidth * 0.5f));
            GUILayout.Label("(Count)", GUILayout.Width(60));
            GUILayout.Label("Object:", GUILayout.Width(columnWidth));
            EditorGUILayout.EndHorizontal();

            if (paths != null)
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                foreach (string path in pathsKeys)
                {
                    GUICreatePathItem(path);
                }
                GUILayout.EndScrollView();
            }

            GUILayout.Space(40);
        }
        else
        {
            GUILayout.Label("Please select an Animator with Animation Clips.");
        }
    }

    void GUICreatePathItem(string path)
    {
        string newPath = path;
        GameObject obj = FindObjectInRoot(path);
        GameObject newObj;
        ArrayList properties = (ArrayList)paths[path];

        string pathOverride = path;

        if (tempPathOverrides.ContainsKey(path))
            pathOverride = tempPathOverrides[path];

        EditorGUILayout.BeginHorizontal();

        pathOverride = EditorGUILayout.TextField(pathOverride, GUILayout.Width(columnWidth));
        if (pathOverride != path)
            tempPathOverrides[path] = pathOverride;

        if (GUILayout.Button("Change", GUILayout.Width(60)))
        {
            newPath = pathOverride;
            tempPathOverrides.Remove(path);
        }

        EditorGUILayout.LabelField(
            properties != null ? properties.Count.ToString() : "0",
            GUILayout.Width(60)
        );

        Color standardColor = GUI.color;

        if (obj != null)
        {
            GUI.color = Color.green;
        }
        else
        {
            GUI.color = Color.red;
        }

        newObj = (GameObject)EditorGUILayout.ObjectField(
            obj,
            typeof(GameObject),
            true,
            GUILayout.Width(columnWidth)
        );

        GUI.color = standardColor;

        EditorGUILayout.EndHorizontal();

        try
        {
            if (obj != newObj)
            {
                UpdatePath(path, ChildPath(newObj));
            }

            if (newPath != path)
            {
                UpdatePath(path, newPath);
            }
        }
        catch (UnityException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    void OnInspectorUpdate()
    {
        this.Repaint();
    }

    void FillModel()
    {
        if (selectedClip == null) return;

        paths = new Hashtable();
        pathsKeys = new ArrayList();

        FillModelWithCurves(AnimationUtility.GetCurveBindings(selectedClip));
        FillModelWithCurves(AnimationUtility.GetObjectReferenceCurveBindings(selectedClip));
    }

    private void FillModelWithCurves(EditorCurveBinding[] curves)
    {
        foreach (EditorCurveBinding curveData in curves)
        {
            string key = curveData.path;

            if (paths.ContainsKey(key))
            {
                ((ArrayList)paths[key]).Add(curveData);
            }
            else
            {
                ArrayList newProperties = new ArrayList();
                newProperties.Add(curveData);
                paths.Add(key, newProperties);
                pathsKeys.Add(key);
            }
        }
    }

    void FixMissingPaths()
    {
        foreach (string path in pathsKeys)
        {
            GameObject obj = FindObjectInRoot(path);
            if (obj == null)
            {
                string fixedPath = TryFixMissingPath(path);
                if (!string.IsNullOrEmpty(fixedPath))
                {
                    UpdatePath(path, fixedPath);
                }
            }
        }
    }

    string TryFixMissingPath(string path)
    {
        if (animatorObject == null) return null;

        string[] pathParts = path.Split('/');
        string lastPart = pathParts.Last();

        // Find all GameObjects with matching names in the hierarchy
        GameObject[] matchingObjects = FindObjectsByName(lastPart);

        // Handle duplicated names
        if (matchingObjects.Length == 1)
        {
            // If only one object matches, use it
            return ChildPath(matchingObjects[0]);
        }
        else if (matchingObjects.Length > 1)
        {
            // Handle duplicates by showing a popup for the user to select the correct one
            int selectedIndex = ShowDuplicateSelectionPopup(matchingObjects, path);
            if (selectedIndex >= 0)
            {
                return ChildPath(matchingObjects[selectedIndex]);
            }
        }

        Debug.LogWarning($"No unique GameObject found for '{lastPart}' in the hierarchy.");
        return null;
    }

    int ShowDuplicateSelectionPopup(GameObject[] matchingObjects, string originalPath)
    {
        // Create a list of paths to display in the popup
        string[] options = new string[matchingObjects.Length];
        for (int i = 0; i < matchingObjects.Length; i++)
        {
            options[i] = ChildPath(matchingObjects[i]); // Display full path for each duplicate
        }

        // Display the popup
        int selectedIndex = EditorUtility.DisplayDialogComplex(
            "Select GameObject",
            $"Multiple GameObjects found with the name '{matchingObjects[0].name}' for the path '{originalPath}'. Please select the correct object:",
            options[0], // Option 1 (select the first object)
            "Cancel",   // Cancel button
            null        // No third button needed
        );

        // If user cancels or closes the dialog, return -1
        if (selectedIndex == 1 || selectedIndex == -1)
        {
            return -1;
        }

        return selectedIndex;
    }

    GameObject[] FindObjectsByName(string name)
    {
        List<GameObject> matchingObjects = new List<GameObject>();
        Transform[] allTransforms = animatorObject.GetComponentsInChildren<Transform>(true);

        foreach (var t in allTransforms)
        {
            if (t.name == name)
                matchingObjects.Add(t.gameObject);
        }

        return matchingObjects.ToArray();
    }

    void UpdatePath(string oldPath, string newPath)
    {
        if (paths[newPath] != null)
        {
            throw new UnityException("Path " + newPath + " already exists in that animation!");
        }
        AssetDatabase.StartAssetEditing();
        for (int iCurrentClip = 0; iCurrentClip < animationClips.Count; iCurrentClip++)
        {
            AnimationClip animationClip = animationClips[iCurrentClip];
            Undo.RecordObject(animationClip, "Animation Hierarchy Change");

            for (int iCurrentPath = 0; iCurrentPath < pathsKeys.Count; iCurrentPath++)
            {
                string path = pathsKeys[iCurrentPath] as string;
                ArrayList curves = (ArrayList)paths[path];

                for (int i = 0; i < curves.Count; i++)
                {
                    EditorCurveBinding binding = (EditorCurveBinding)curves[i];
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                    ObjectReferenceKeyframe[] objectReferenceCurve = AnimationUtility.GetObjectReferenceCurve(animationClip, binding);

                    if (curve != null)
                        AnimationUtility.SetEditorCurve(animationClip, binding, null);
                    else
                        AnimationUtility.SetObjectReferenceCurve(animationClip, binding, null);

                    if (path == oldPath)
                        binding.path = newPath;

                    if (curve != null)
                        AnimationUtility.SetEditorCurve(animationClip, binding, curve);
                    else
                        AnimationUtility.SetObjectReferenceCurve(animationClip, binding, objectReferenceCurve);
                }
            }
        }
        AssetDatabase.StopAssetEditing();
        EditorUtility.ClearProgressBar();
        FillModel();
        this.Repaint();
    }

    GameObject FindObjectInRoot(string path)
    {
        if (animatorObject == null)
        {
            return null;
        }

        Transform child = animatorObject.transform.Find(path);

        if (child != null)
        {
            return child.gameObject;
        }
        else
        {
            return null;
        }
    }

    string ChildPath(GameObject obj, bool sep = false)
    {
        if (animatorObject == null)
        {
            throw new UnityException("Please assign Referenced Animator (Root) first!");
        }

        if (obj == animatorObject.gameObject)
        {
            return "";
        }
        else
        {
            if (obj.transform.parent == null)
            {
                throw new UnityException("Object must belong to " + animatorObject.ToString() + "!");
            }
            else
            {
                return ChildPath(obj.transform.parent.gameObject, true) + obj.name + (sep ? "/" : "");
            }
        }
    }

    private void UpdateAnimationClips()
    {
        if (animatorObject != null)
        {
            animationClips = new List<AnimationClip>();

            // Get all animation clips from the animator's runtime controller
            if (animatorObject.runtimeAnimatorController != null)
            {
                animationClips.AddRange(animatorObject.runtimeAnimatorController.animationClips);
            }
        }
        else
        {
            animationClips.Clear();
        }
    }
}

#endif

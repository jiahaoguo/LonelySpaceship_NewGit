using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LoadAnimationPro))]
public class LoadAnimationProEditor : Editor
{
    SerializedProperty loadInAnimations;
    SerializedProperty loadOutAnimations;
    SerializedProperty cascadeChildren;
    SerializedProperty onLoadInStart;
    SerializedProperty onLoadInFinish;
    SerializedProperty onLoadOutStart;
    SerializedProperty onLoadOutFinish;

    bool showEvents = false;

    void OnEnable()
    {
        cascadeChildren = serializedObject.FindProperty("cascadeChildren");
        loadInAnimations = serializedObject.FindProperty("loadInAnimations");
        loadOutAnimations = serializedObject.FindProperty("loadOutAnimations");
        onLoadInStart = serializedObject.FindProperty("onLoadInStart");
        onLoadInFinish = serializedObject.FindProperty("onLoadInFinish");
        onLoadOutStart = serializedObject.FindProperty("onLoadOutStart");
        onLoadOutFinish = serializedObject.FindProperty("onLoadOutFinish");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(cascadeChildren, new GUIContent("Cascade Children"));

        DrawAnimationGroup(loadInAnimations, "Load In (FROM offsets)");
        EditorGUILayout.Space();
        DrawAnimationGroup(loadOutAnimations, "Load Out (TO offsets)");

        EditorGUILayout.Space();
        showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
        if (showEvents)
        {
            EditorGUILayout.PropertyField(onLoadInStart);
            EditorGUILayout.PropertyField(onLoadInFinish);
            EditorGUILayout.PropertyField(onLoadOutStart);
            EditorGUILayout.PropertyField(onLoadOutFinish);
        }

        // === New Validation for RectTransform anchors ===
        LoadAnimationPro targetScript = (LoadAnimationPro)target;
        RectTransform rt = targetScript.transform as RectTransform;

        if (rt != null)
        {
            bool hasPositionAnim = HasPositionAnimation(loadInAnimations) || HasPositionAnimation(loadOutAnimations);

            if (hasPositionAnim)
            {
                bool isAnchorCentered =
                    Mathf.Approximately(rt.anchorMin.x, 0.5f) &&
                    Mathf.Approximately(rt.anchorMax.x, 0.5f) &&
                    Mathf.Approximately(rt.anchorMin.y, 0.5f) &&
                    Mathf.Approximately(rt.anchorMax.y, 0.5f);

                if (!isAnchorCentered)
                {
                    EditorGUILayout.HelpBox(
                        "⚠️ This object has a RectTransform with non-centered anchors, " +
                        "and one or more LoadAnimationPro animations use Position offset.\n\n" +
                        "Position-based animations may behave unexpectedly unless the anchors are centered " +
                        "(or you switch to anchoredPosition logic).",
                        MessageType.Error
                    );
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawAnimationGroup(SerializedProperty list, string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        for (int i = 0; i < list.arraySize; i++)
        {
            var element = list.GetArrayElementAtIndex(i);
            var type = element.FindPropertyRelative("type");

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(type);

            var animType = (LoadAnimationPro.AnimationType)type.enumValueIndex;

            if (animType != LoadAnimationPro.AnimationType.UnityAnimation)
            {
                EditorGUILayout.PropertyField(element.FindPropertyRelative("duration"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("curve"));
            }

            switch (animType)
            {
                case LoadAnimationPro.AnimationType.Scale:
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("scale"),
                        new GUIContent(label.Contains("FROM") ? "From Scale Offset" : "To Scale Offset"));
                    break;

                case LoadAnimationPro.AnimationType.Position:
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("position"),
                        new GUIContent(label.Contains("FROM") ? "From Position Offset" : "To Position Offset"));
                    break;

                case LoadAnimationPro.AnimationType.Rotation:
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("rotation"),
                        new GUIContent(label.Contains("FROM") ? "From Rotation Offset" : "To Rotation Offset"));
                    break;

                case LoadAnimationPro.AnimationType.Alpha:
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("alpha"),
                        new GUIContent(label.Contains("FROM") ? "From Alpha" : "To Alpha"));
                    break;

                case LoadAnimationPro.AnimationType.UnityAnimation:
                    DrawUnityAnimationClipSelector(element);
                    break;
            }

            if (GUILayout.Button("Remove")) list.DeleteArrayElementAtIndex(i);

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add Animation Option"))
            list.InsertArrayElementAtIndex(list.arraySize);
    }

    void DrawUnityAnimationClipSelector(SerializedProperty option)
    {
        var clipProp = option.FindPropertyRelative("clip");
        var useOverrideProp = option.FindPropertyRelative("useOverrideDuration");
        var overrideDurProp = option.FindPropertyRelative("overrideDuration");

        LoadAnimationPro targetScript = (LoadAnimationPro)target;
        Animator animator = targetScript.GetComponent<Animator>();

        AnimationClip currentClip = clipProp.objectReferenceValue as AnimationClip;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            string[] names = new string[clips.Length];
            int currentIndex = -1;

            for (int i = 0; i < clips.Length; i++)
            {
                names[i] = clips[i].name;
                if (clips[i] == currentClip)
                    currentIndex = i;
            }

            int newIndex = EditorGUILayout.Popup("Animation Clip", currentIndex, names);
            if (newIndex >= 0 && newIndex < clips.Length)
            {
                clipProp.objectReferenceValue = clips[newIndex];
                currentClip = clips[newIndex];
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No Animator with clips found on this object.", MessageType.Info);
        }

        if (currentClip != null)
        {
            float baseLen = currentClip.length;

            if (baseLen > 0)
                EditorGUILayout.LabelField("Clip Length", $"{baseLen:F2} sec (frameRate {currentClip.frameRate})");
            else
                EditorGUILayout.HelpBox("Unity reports this clip has length 0. Please set Override Duration manually.", MessageType.Warning);
        }

        EditorGUILayout.PropertyField(useOverrideProp, new GUIContent("Use Override Duration"));
        if (useOverrideProp.boolValue)
        {
            EditorGUILayout.PropertyField(overrideDurProp, new GUIContent("Override Duration (sec)"));
        }
    }

    // Helper: check if list has any position animations
    bool HasPositionAnimation(SerializedProperty list)
    {
        for (int i = 0; i < list.arraySize; i++)
        {
            var element = list.GetArrayElementAtIndex(i);
            var type = element.FindPropertyRelative("type");
            if ((LoadAnimationPro.AnimationType)type.enumValueIndex == LoadAnimationPro.AnimationType.Position)
                return true;
        }
        return false;
    }
}

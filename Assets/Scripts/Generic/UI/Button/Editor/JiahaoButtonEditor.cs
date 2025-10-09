using UnityEditor;
using UnityEditor.UI;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[CustomEditor(typeof(JiahaoButton))]
[CanEditMultipleObjects]
public class JiahaoButtonEditor : ButtonEditor
{
    private SerializedProperty onSelectEvent;
    private SerializedProperty onDeselectEvent;
    private SerializedProperty onHoverEnterEvent;
    private SerializedProperty onHoverExitEvent;

    private bool navigationFoldout = false;
    private int navModeIndex = 0;
    private readonly string[] navModes = new[] { "Horizontal", "Vertical" };

    private bool siblingOrderFoldout = false;
    private int siblingEventIndex = 0;
    private readonly string[] siblingEventModes = new[] { "Select Events", "Hover Events" };

    protected override void OnEnable()
    {
        base.OnEnable();

        onSelectEvent = serializedObject.FindProperty("OnSelectEvent");
        onDeselectEvent = serializedObject.FindProperty("OnDeselectEvent");
        onHoverEnterEvent = serializedObject.FindProperty("OnHoverEnterEvent");
        onHoverExitEvent = serializedObject.FindProperty("OnHoverExitEvent");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        // === Extra Events ===
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Extra Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(onSelectEvent);
        EditorGUILayout.PropertyField(onDeselectEvent);
        EditorGUILayout.PropertyField(onHoverEnterEvent);
        EditorGUILayout.PropertyField(onHoverExitEvent);

        // === Auto Navigation ===
        EditorGUILayout.Space();
        navigationFoldout = EditorGUILayout.Foldout(navigationFoldout, "Auto Navigation", true);
        if (navigationFoldout)
        {
            if (targets.Length == 1)
            {
                var button = (JiahaoButton)target;
                EditorGUILayout.BeginHorizontal();
                navModeIndex = EditorGUILayout.Popup(navModeIndex, navModes, GUILayout.Width(100));

                if (GUILayout.Button("Auto Set (Selected Only)", GUILayout.Height(22)))
                    AutoSetNavigation(button, navModes[navModeIndex], false);

                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Auto Set (All Siblings)", GUILayout.Height(22)))
                    AutoSetNavigation(button, navModes[navModeIndex], true);
            }
            else
            {
                EditorGUILayout.HelpBox("Auto Navigation not supported in multi-edit mode.", MessageType.Info);
            }
        }

        // === Sibling Layer Order ===
        EditorGUILayout.Space();
        siblingOrderFoldout = EditorGUILayout.Foldout(siblingOrderFoldout, "Sibling Layer Order", true);
        if (siblingOrderFoldout)
        {
            EditorGUILayout.BeginHorizontal();
            siblingEventIndex = EditorGUILayout.Popup(siblingEventIndex, siblingEventModes, GUILayout.Width(120));

            if (targets.Length == 1)
            {
                if (GUILayout.Button("Add Bring-To-Front Events", GUILayout.Height(22)))
                    AddBringToFrontEvents((JiahaoButton)target, siblingEventModes[siblingEventIndex]);
            }
            else
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Batch apply bring-to-front to all selected JiahaoButtons.", MessageType.None);

                if (GUILayout.Button("Batch Add To All", GUILayout.Height(22)))
                {
                    foreach (var obj in targets)
                    {
                        var button = obj as JiahaoButton;
                        if (button != null)
                            AddBringToFrontEvents(button, siblingEventModes[siblingEventIndex]);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }

    // -----------------------------
    // AUTO NAVIGATION LOGIC
    // -----------------------------

    private void AutoSetNavigation(JiahaoButton button, string mode, bool affectAllSiblings)
    {
        Transform parent = button.transform.parent;
        if (parent == null)
        {
            Debug.LogWarning($"[{button.name}] has no parent — cannot auto-set navigation.");
            return;
        }

        var siblings = parent.GetComponentsInChildren<JiahaoButton>(false);
        if (siblings.Length <= 1)
        {
            Debug.LogWarning($"[{button.name}] has no siblings to link with.");
            return;
        }

        if (affectAllSiblings)
        {
            for (int i = 0; i < siblings.Length; i++)
                SetButtonNavigation(siblings, i, mode);
            Debug.Log($"[JiahaoButton] Auto navigation set for all siblings ({mode}).");
        }
        else
        {
            int index = System.Array.IndexOf(siblings, button);
            if (index != -1)
            {
                SetButtonNavigation(siblings, index, mode);
                Debug.Log($"[{button.name}] Auto navigation set ({mode}).");
            }
        }

        foreach (var b in siblings)
            EditorUtility.SetDirty(b);
    }

    private void SetButtonNavigation(JiahaoButton[] siblings, int index, string mode)
    {
        int prevIndex = (index - 1 + siblings.Length) % siblings.Length;
        int nextIndex = (index + 1) % siblings.Length;

        JiahaoButton current = siblings[index];
        Navigation nav = current.navigation;
        nav.mode = Navigation.Mode.Explicit;

        if (mode == "Horizontal")
        {
            nav.selectOnLeft = siblings[prevIndex];
            nav.selectOnRight = siblings[nextIndex];
            nav.selectOnUp = null;
            nav.selectOnDown = null;
        }
        else
        {
            nav.selectOnUp = siblings[prevIndex];
            nav.selectOnDown = siblings[nextIndex];
            nav.selectOnLeft = null;
            nav.selectOnRight = null;
        }

        current.navigation = nav;
    }

    // -----------------------------
    // SIBLING ORDER LOGIC
    // -----------------------------

    private void AddBringToFrontEvents(JiahaoButton button, string eventType)
    {
        Undo.RecordObject(button, "Add Bring-To-Front Events");

        UnityEvent enterEvent = null;
        UnityEvent exitEvent = null;

        if (eventType == "Select Events")
        {
            enterEvent = button.OnSelectEvent;
            exitEvent = button.OnDeselectEvent;
        }
        else if (eventType == "Hover Events")
        {
            enterEvent = button.OnHoverEnterEvent;
            exitEvent = button.OnHoverExitEvent;
        }

        if (enterEvent == null || exitEvent == null)
        {
            Debug.LogWarning($"[{button.name}] Missing UnityEvents for {eventType}");
            return;
        }

        if (!HasPersistentListener(enterEvent, button, nameof(JiahaoButton.BringToFront)))
            UnityEventTools.AddPersistentListener(enterEvent, button.BringToFront);

        if (!HasPersistentListener(exitEvent, button, nameof(JiahaoButton.RestoreSiblingOrder)))
            UnityEventTools.AddPersistentListener(exitEvent, button.RestoreSiblingOrder);

        EditorUtility.SetDirty(button);
        Debug.Log($"[JiahaoButtonEditor] Bring-to-front events added to {eventType} for [{button.name}].");
    }

    private bool HasPersistentListener(UnityEvent evt, Object target, string methodName)
    {
        for (int i = 0; i < evt.GetPersistentEventCount(); i++)
        {
            if (evt.GetPersistentTarget(i) == target && evt.GetPersistentMethodName(i) == methodName)
                return true;
        }
        return false;
    }
}

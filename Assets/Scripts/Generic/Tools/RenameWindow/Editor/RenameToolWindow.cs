using UnityEngine;
using UnityEditor;

public class RenameToolWindow : EditorWindow
{
    private const int SlotCount = 10;
    private string[] names = new string[SlotCount];
    private const string PrefKey = "RenameTool_Names_";

    private bool multiNumbering = false; // 多物体编号模式
    private bool hotkeyMode = false;     // 快捷键模式

    [MenuItem("JiahaoTools/Rename Tool")]
    public static void ShowWindow()
    {
        GetWindow<RenameToolWindow>("Rename Tool");
    }

    private void OnEnable()
    {
        for (int i = 0; i < SlotCount; i++)
            names[i] = EditorPrefs.GetString(PrefKey + i, string.Empty);

        multiNumbering = EditorPrefs.GetBool("RenameTool_MultiNumbering", false);
        hotkeyMode = EditorPrefs.GetBool("RenameTool_HotkeyMode", false);

        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private void OnDisable()
    {
        SavePrefs();
        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
    }

    private void SavePrefs()
    {
        for (int i = 0; i < SlotCount; i++)
            EditorPrefs.SetString(PrefKey + i, names[i]);

        EditorPrefs.SetBool("RenameTool_MultiNumbering", multiNumbering);
        EditorPrefs.SetBool("RenameTool_HotkeyMode", hotkeyMode);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("批量重命名工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // --- 多物体编号模式 Toggle ---
        EditorGUILayout.BeginHorizontal();
        multiNumbering = EditorGUILayout.Toggle(multiNumbering, GUILayout.Width(20));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("多物体编号模式", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("为多个物体加 1,2,3… 后缀", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // --- 快捷键模式 Toggle ---
        EditorGUILayout.BeginHorizontal();
        hotkeyMode = EditorGUILayout.Toggle(hotkeyMode, GUILayout.Width(20));
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("快捷键模式", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Shift+数字键，当层级窗口激活时可用", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // --- Rename slots ---
        for (int i = 0; i < SlotCount; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(20));
            names[i] = EditorGUILayout.TextField(names[i]);

            if (GUILayout.Button("命名为此", GUILayout.Width(80)))
                RenameSelectedObjects(names[i]);
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("保存输入"))
            SavePrefs();
    }

    private void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        if (!hotkeyMode) return;

        Event e = Event.current;
        if (e == null || e.type != EventType.KeyDown || !e.shift) return;

        int index = -1;
        if (e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha9)
            index = (int)e.keyCode - (int)KeyCode.Alpha1;
        else if (e.keyCode == KeyCode.Alpha0)
            index = 9;

        if (index >= 0 && index < SlotCount)
        {
            RenameSelectedObjects(names[index]);
            e.Use(); // stop Unity from also handling the hotkey
        }
    }

    private void RenameSelectedObjects(string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("名称为空，无法重命名。");
            return;
        }

        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0) return;

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            var go = selectedObjects[i];
            if (go == null) continue;

            Undo.RecordObject(go, "Rename Object");

            if (multiNumbering && selectedObjects.Length > 1)
                go.name = newName + (i + 1);
            else
                go.name = newName;

            EditorUtility.SetDirty(go);
        }
    }
}

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ItemData item = (ItemData)target;

        // Draw default fields
        DrawDefaultInspector();

        GUILayout.Space(10);

        // Check if itemName is empty
        if (string.IsNullOrEmpty(item.itemName))
        {
            // Red warning box
            GUIStyle redStyle = new GUIStyle(EditorStyles.helpBox);
            redStyle.normal.textColor = Color.red;
            redStyle.fontStyle = FontStyle.Bold;
            redStyle.fontSize = 12;
            EditorGUILayout.LabelField("⚠ ERROR: itemName is empty!", redStyle);

            // Optional: auto-fill suggestion
            if (GUILayout.Button("Use asset name as itemName"))
            {
                item.itemName = item.name;
                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssets();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // Optional: Block saving with empty name (extra safety)
    private void OnDisable()
    {
        ItemData item = (ItemData)target;
        if (string.IsNullOrEmpty(item.itemName))
        {
            Debug.LogError($"[ItemData] '{item.name}' has no itemName! Please set it before saving.");
        }
    }
}

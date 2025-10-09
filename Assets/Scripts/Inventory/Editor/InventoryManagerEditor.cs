#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(InventoryManager))]
public class InventoryManagerEditor : Editor
{
    private ItemData selectedItem;
    private string search = "";
    private int amount = 1;
    private bool showSearchResults = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Debug / Testing", EditorStyles.boldLabel);

        var manager = (InventoryManager)target;
        var lib = Object.FindFirstObjectByType<ItemLibrary>();

        if (lib == null)
        {
            EditorGUILayout.HelpBox("No ItemLibrary found in scene.", MessageType.Warning);
            if (GUILayout.Button("Create ItemLibrary"))
                new GameObject("ItemLibrary").AddComponent<ItemLibrary>();
            return;
        }

        EditorGUILayout.BeginHorizontal();

        // --- Search field ---
        EditorGUI.BeginChangeCheck();
        search = EditorGUILayout.TextField(search, GUILayout.Width(160));
        if (EditorGUI.EndChangeCheck())
            showSearchResults = !string.IsNullOrEmpty(search);

        // --- Dropdown button ---
        if (GUILayout.Button("▼", GUILayout.Width(25)))
        {
            ShowItemMenu(lib);
            showSearchResults = false;
        }

        // --- Amount field ---
        amount = EditorGUILayout.IntField(amount, GUILayout.Width(60));

        // --- Add / Remove ---
        bool hasSelection = selectedItem != null;
        GUI.enabled = hasSelection;
        if (GUILayout.Button(amount >= 0 ? "Add" : "Remove", GUILayout.Width(80)))
        {
            if (amount >= 0)
                manager.AddItem(selectedItem, amount);
            else
                manager.RemoveItem(selectedItem, -amount);

            EditorUtility.SetDirty(manager);
        }

        // --- Clear All ---
        GUI.enabled = true;
        if (GUILayout.Button("Clear All", GUILayout.Width(80)))
        {
            Undo.RecordObject(manager, "Clear Inventory");
            manager.ClearAll(); // ✅ runtime + editor safe
            EditorUtility.SetDirty(manager);
        }

        EditorGUILayout.EndHorizontal();

        // --- Live search results ---
        if (showSearchResults && !string.IsNullOrEmpty(search))
        {
            var matches = lib.itemGroups
                .SelectMany(g => g.items)
                .Where(i => i != null && i.itemName.ToLower().Contains(search.ToLower()))
                .OrderBy(i => i.itemName)
                .Take(8)
                .ToList();

            if (matches.Count > 0)
            {
                EditorGUILayout.BeginVertical("box");
                foreach (var item in matches)
                {
                    if (GUILayout.Button(item.itemName, EditorStyles.miniButton))
                    {
                        selectedItem = item;
                        search = item.itemName;
                        showSearchResults = false;
                        GUI.FocusControl(null);
                        Repaint();
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        // --- Selected item label ---
        if (selectedItem != null)
        {
            EditorGUILayout.LabelField($"Selected: {selectedItem.itemName}", EditorStyles.miniLabel);
        }
    }

    private void ShowItemMenu(ItemLibrary lib)
    {
        GenericMenu menu = new GenericMenu();

        foreach (var group in lib.itemGroups.OrderBy(g => g.groupName))
        {
            string groupName = string.IsNullOrWhiteSpace(group.groupName) ? "Default" : group.groupName.Trim();
            foreach (var item in group.items.Where(i => i != null && !string.IsNullOrEmpty(i.itemName)).OrderBy(i => i.itemName))
            {
                string path = $"{groupName}/{item.itemName}";
                menu.AddItem(new GUIContent(path), selectedItem == item, () =>
                {
                    selectedItem = item;
                    search = item.itemName;
                    showSearchResults = false;
                    Repaint();
                });
            }
        }

        if (menu.GetItemCount() == 0)
            menu.AddDisabledItem(new GUIContent("No items available"));

        menu.ShowAsContext();
    }
}
#endif

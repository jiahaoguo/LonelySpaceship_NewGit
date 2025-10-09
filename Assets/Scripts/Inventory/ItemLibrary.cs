using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ItemLibrary : MonoBehaviour
{
    [System.Serializable]
    public class ItemGroup
    {
        public string groupName;
        public List<ItemData> items = new List<ItemData>();
    }

    [Header("Grouped view (auto generated)")]
    public List<ItemGroup> itemGroups = new List<ItemGroup>();

    private Dictionary<string, ItemData> itemLookup = new Dictionary<string, ItemData>();
    private Dictionary<string, List<ItemData>> groupLookup = new Dictionary<string, List<ItemData>>();

    private void OnEnable()
    {
        LoadAllItems();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        LoadAllItems();
    }
#endif

    public void LoadAllItems()
    {
        itemGroups.Clear();
        itemLookup.Clear();  // make sure it's clean before adding
        groupLookup.Clear();

        var allItems = Resources.LoadAll<ItemData>("ItemData");
        var groups = new Dictionary<string, List<ItemData>>();

#if UNITY_EDITOR
    foreach (var item in allItems)
    {
        var path = AssetDatabase.GetAssetPath(item);
        string group = ExtractTopGroup(path);
        if (!groups.ContainsKey(group))
            groups[group] = new List<ItemData>();
        groups[group].Add(item);

        // ✅ Register item for quick lookup
        if (!itemLookup.ContainsKey(item.name))
            itemLookup[item.name] = item;
    }
#else
        groups["Default"] = allItems.ToList();
        foreach (var item in allItems)
        {
            if (!itemLookup.ContainsKey(item.name))
                itemLookup[item.name] = item;
        }
#endif

        foreach (var g in groups)
        {
            itemGroups.Add(new ItemGroup { groupName = g.Key, items = g.Value });
            groupLookup[g.Key] = g.Value;
        }

        Debug.Log($"[ItemLibrary] Loaded {itemLookup.Count} items into lookup.");
    }


#if UNITY_EDITOR
    /// <summary>
    /// Extracts the first-level folder name under Resources/ItemData as the group.
    /// If no folder exists, returns "Default".
    /// </summary>
    private string ExtractTopGroup(string assetPath)
    {
        // Example paths:
        // Assets/Resources/ItemData/Sword.asset
        // Assets/Resources/ItemData/Weapons/Sword.asset
        // Assets/Resources/ItemData/Weapons/Subfolder/Sword.asset

        int rootIndex = assetPath.IndexOf("Resources/ItemData/");
        if (rootIndex < 0)
            return "Default";

        string subPath = assetPath.Substring(rootIndex + "Resources/ItemData/".Length);

        if (subPath.Contains("/"))
        {
            // Extract the first folder name before the next slash
            string firstFolder = subPath.Substring(0, subPath.IndexOf("/"));
            return firstFolder;
        }

        // Asset is directly under ItemData/
        return "Default";
    }
#endif

    public ItemData GetItemByName(string name)
    {
        itemLookup.TryGetValue(name, out var data);
        return data;
    }

    public List<string> GetAllGroupNames()
    {
        return new List<string>(groupLookup.Keys);
    }

    public List<ItemData> GetItemsInGroup(string group)
    {
        return groupLookup.TryGetValue(group, out var list) ? list : new List<ItemData>();
    }
}

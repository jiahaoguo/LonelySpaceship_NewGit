#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ItemLibraryAutoRefresh : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // Detect if any asset path involves Resources/ItemData
        bool changed = false;

        foreach (var path in importedAssets)
        {
            if (path.Contains("Resources/ItemData")) { changed = true; break; }
        }
        if (!changed)
        {
            foreach (var path in deletedAssets)
            {
                if (path.Contains("Resources/ItemData")) { changed = true; break; }
            }
        }
        if (!changed)
        {
            foreach (var path in movedAssets)
            {
                if (path.Contains("Resources/ItemData")) { changed = true; break; }
            }
        }

        if (!changed)
            return;

        // Find any ItemLibrary in open scenes and refresh
        var libs = Object.FindObjectsByType<ItemLibrary>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (libs == null || libs.Length == 0)
            return;

        foreach (var lib in libs)
        {
            if (lib == null) continue;
            lib.LoadAllItems();
            EditorUtility.SetDirty(lib);
        }

        Debug.Log($"[ItemLibraryAutoRefresh] Detected change under Resources/ItemData — refreshed {libs.Length} library instance(s).");
    }
}
#endif

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SaveContainer
{
    public string version = "1.0";

    [Serializable]
    public class SectionEntry
    {
        public string key;
        public string json;
    }

    public List<SectionEntry> sections = new();

    public void AddSection(string key, object data)
    {
        sections.Add(new SectionEntry
        {
            key = key,
            json = JsonUtility.ToJson(data)
        });
    }

    public string GetSectionJson(string key)
    {
        foreach (var s in sections)
        {
            if (s.key == key)
                return s.json;
        }
        return null;
    }
}

public class SaveSystemManager : MonoBehaviour
{
    [Header("Save Settings")]
    [Tooltip("Name of the current playthrough folder, e.g., Playthrough_01")]
    public string playthroughName = "Playthrough_01";

    [Tooltip("Base save file name (without timestamp).")]
    public string fileName = "save.json";

    [Tooltip("Auto load latest on start")]
    public bool autoLoadOnStart = true;

    // Paths ---------------------------------------------------------
    private string RootSaveFolder => Path.Combine(Application.persistentDataPath, "Saves");
    private string PlaythroughFolder => Path.Combine(RootSaveFolder, playthroughName);
    private string FilePath => Path.Combine(PlaythroughFolder, "latest.txt");

    private void Start()
    {
        if (autoLoadOnStart)
            LoadGame();
    }

    // ------------------------------------------------------------
    // SAVE GAME  (Manual Save)
    // ------------------------------------------------------------
    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string saveFolder = Path.Combine(PlaythroughFolder, $"Save_{timeStamp}");
        Directory.CreateDirectory(saveFolder);

        string jsonPath = Path.Combine(saveFolder, fileName);
        string screenshotPath = Path.Combine(saveFolder, "screenshot.png");

        var container = new SaveContainer();
        var agents = FindObjectsOfType<SaveAgentBase>(true);

        foreach (var agent in agents)
        {
            object data = agent.CaptureData();
            if (data != null)
            {
                container.AddSection(agent.SectionName, data);
                Debug.Log($"[SaveSystemManager] Saved section: {agent.SectionName}");
            }
        }

        string json = JsonUtility.ToJson(container, true);
        File.WriteAllText(jsonPath, json);
        Debug.Log($"💾 Manual Save created: {saveFolder}");

        ScreenCapture.CaptureScreenshot(screenshotPath);
        Debug.Log($"📸 Screenshot saved: {screenshotPath}");

        // Mark as latest manual save
        SetLatestSave(saveFolder);
    }

    // ------------------------------------------------------------
    // LOAD GAME
    // ------------------------------------------------------------
    [ContextMenu("Load Game")]
    public void LoadGame()
    {
        string folderToLoad = GetLatestSaveFolder();

        if (folderToLoad == null)
        {
            Debug.LogWarning("⚠ No save folder found. Skipping load.");
            return;
        }

        string jsonPath = Path.Combine(folderToLoad, fileName);
        if (!File.Exists(jsonPath))
        {
            Debug.LogWarning($"⚠ No save file found in {folderToLoad}.");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        var container = JsonUtility.FromJson<SaveContainer>(json);

        var agents = FindObjectsOfType<SaveAgentBase>(true);

        foreach (var agent in agents)
        {
            string sectionJson = container.GetSectionJson(agent.SectionName);
            if (string.IsNullOrEmpty(sectionJson))
            {
                Debug.LogWarning($"[SaveSystemManager] No section found for {agent.SectionName}");
                continue;
            }

            Type dataType = GetDataType(agent);
            object data = JsonUtility.FromJson(sectionJson, dataType);
            agent.RestoreData(data);

            Debug.Log($"[SaveSystemManager] Restored section: {agent.SectionName}");
        }

        Debug.Log($"✅ Game loaded successfully from {folderToLoad}");
    }

    // ------------------------------------------------------------
    // CLEAR SAVE
    // ------------------------------------------------------------
    [ContextMenu("Clear Save File")]
    public void ClearSave()
    {
        string latestFolder = GetLatestSaveFolder();
        if (latestFolder != null && Directory.Exists(latestFolder))
        {
            Directory.Delete(latestFolder, true);
            Debug.Log($"🧹 Deleted save folder: {latestFolder}");
        }
        else
        {
            Debug.Log("No save folder to delete.");
        }
    }

    // ------------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------------
    private Type GetDataType(SaveAgentBase agent)
    {
        foreach (var t in agent.GetType().GetNestedTypes())
        {
            if (t.Name.EndsWith("SaveData"))
                return t;
        }
        return typeof(object);
    }

    private void SetLatestSave(string folderPath)
    {
        Directory.CreateDirectory(PlaythroughFolder);
        string pointerFile = Path.Combine(PlaythroughFolder, "latest.txt");
        File.WriteAllText(pointerFile, folderPath);
    }

    private string GetLatestSaveFolder()
    {
        string pointerFile = Path.Combine(PlaythroughFolder, "latest.txt");
        if (File.Exists(pointerFile))
        {
            string path = File.ReadAllText(pointerFile);
            if (Directory.Exists(path))
                return path;
        }

        var dirs = Directory.GetDirectories(PlaythroughFolder, "Save_*");
        if (dirs.Length > 0)
            return dirs[^1]; // newest by name

        return null;
    }

    // ------------------------------------------------------------
    // (Future use) Auto-save folder utilities
    // ------------------------------------------------------------
    private string GetAutoSaveFolder()
    {
        string autoSaveFolder = Path.Combine(PlaythroughFolder, "AutoSave_0");
        return autoSaveFolder;
    }
}

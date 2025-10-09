using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;

[CustomEditor(typeof(SaveSystemManager))]
public class SaveSystemManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector first
        DrawDefaultInspector();

        // Spacing for clarity
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Editor Controls", EditorStyles.boldLabel);

        SaveSystemManager manager = (SaveSystemManager)target;

        // SAVE button
        if (GUILayout.Button("💾 Save Game", GUILayout.Height(30)))
        {
            manager.SaveGame();
            EditorUtility.DisplayDialog("Save System", "Game saved successfully.", "OK");
        }

        // LOAD button
        if (GUILayout.Button("📂 Load Game", GUILayout.Height(30)))
        {
            manager.LoadGame();
            EditorUtility.DisplayDialog("Save System", "Game loaded successfully.", "OK");
        }

        // OPEN FILE LOCATION button
        if (GUILayout.Button("📁 Open Save Folder", GUILayout.Height(30)))
        {
            string path = Application.persistentDataPath;

#if UNITY_EDITOR_WIN
            Process.Start("explorer.exe", path.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX
            Process.Start("open", path);
#else
            EditorUtility.RevealInFinder(path);
#endif
        }

        GUILayout.Space(10);
    }
}

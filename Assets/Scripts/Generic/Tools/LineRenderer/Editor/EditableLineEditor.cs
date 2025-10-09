using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(EditableLine))]
public class EditableLineEditor : Editor
{
    private int selectedIndex = -1;

    private Stack<Vector3[]> undoStack = new Stack<Vector3[]>();
    private Stack<Vector3[]> redoStack = new Stack<Vector3[]>();
    private const int MaxHistory = 20; // cap size

    // floating hint timers
    private string floatingHint = "";
    private double floatingHintEndTime = 0;
    private float floatingAlpha = 1f;

    void OnSceneGUI()
    {
        EditableLine line = (EditableLine)target;
        if (line.points == null) return;

        bool useWorld = line.UsingWorldSpace();
        Handles.color = Color.cyan;

        // Draw all points
        for (int i = 0; i < line.points.Length; i++)
        {
            Vector3 worldPos = useWorld ? line.points[i] : line.transform.TransformPoint(line.points[i]);
            float size = HandleUtility.GetHandleSize(worldPos) * 0.1f;

            if (Handles.Button(worldPos, Quaternion.identity, size, size * 1.5f, Handles.SphereHandleCap))
                selectedIndex = i;

            GUIStyle style = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.cyan } };
            Handles.Label(worldPos + Vector3.up * size * 0.5f, $"P{i}", style);

            if (selectedIndex == i)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    SaveState(line);
                    line.points[i] = useWorld ? newWorldPos : line.transform.InverseTransformPoint(newWorldPos);
                    line.Apply();
                    EditorUtility.SetDirty(line);
                }
            }
        }

        // Highlight insert target
        HighlightInsertTarget(line, useWorld);

        HandleDeleteKey(line);
        HandleInsertKey(line, useWorld);
        HandleUndoRedo(line);
        DrawInsertPreview(line, useWorld);
        DrawShortcutHints();
        DrawFloatingHint();
    }

    // ----------------- Undo / Redo -----------------
    private void SaveState(EditableLine line)
    {
        Vector3[] snapshot = new Vector3[line.points.Length];
        line.points.CopyTo(snapshot, 0);
        undoStack.Push(snapshot);

        if (undoStack.Count > MaxHistory)
        {
            var temp = new List<Vector3[]>(undoStack);
            temp.RemoveAt(0);
            undoStack = new Stack<Vector3[]>(temp);
        }

        redoStack.Clear();
    }

    private void HandleUndoRedo(EditableLine line)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.control && e.shift && e.keyCode == KeyCode.Z)
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push((Vector3[])line.points.Clone());
                line.points = undoStack.Pop();
                line.Apply();
                EditorUtility.SetDirty(line);

                ShowFloatingHint("Press Ctrl+Shift+Y to redo", 5);
            }
            e.Use();
        }

        if (e.type == EventType.KeyDown && e.control && e.shift && e.keyCode == KeyCode.Y)
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push((Vector3[])line.points.Clone());
                line.points = redoStack.Pop();
                line.Apply();
                EditorUtility.SetDirty(line);

                ShowFloatingHint("Press Ctrl+Shift+Z to undo", 5);
            }
            e.Use();
        }
    }

    // ----------------- Delete -----------------
    private void HandleDeleteKey(EditableLine line)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete && selectedIndex >= 0)
        {
            SaveState(line);

            var newPoints = new Vector3[line.points.Length - 1];
            for (int i = 0, j = 0; i < line.points.Length; i++)
            {
                if (i == selectedIndex) continue;
                newPoints[j++] = line.points[i];
            }

            line.points = newPoints;
            line.Apply();
            selectedIndex = -1;

            EditorUtility.SetDirty(line);

            ShowFloatingHint("Press Ctrl+Shift+Y to redo", 5);
            e.Use();
        }
    }

    // ----------------- Insert -----------------
    private void HandleInsertKey(EditableLine line, bool useWorld)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.shift && e.button == 0)
        {
            // Physics Raycast
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity))
                return;

            Vector3 worldPos = hit.point;

            SaveState(line);

            int insertIndex;
            if (selectedIndex == 0 && e.alt) // ALT special prepend
                insertIndex = 0;
            else
                insertIndex = selectedIndex >= 0 ? selectedIndex + 1 : line.points.Length;

            var newPoints = new Vector3[line.points.Length + 1];
            for (int i = 0, j = 0; i < newPoints.Length; i++)
            {
                if (i == insertIndex)
                {
                    newPoints[i] = useWorld ? worldPos : line.transform.InverseTransformPoint(worldPos);
                }
                else
                {
                    newPoints[i] = line.points[j++];
                }
            }

            line.points = newPoints;
            line.Apply();
            selectedIndex = insertIndex;

            EditorUtility.SetDirty(line);

            ShowFloatingHint("Press Ctrl+Shift+Z to undo", 5);
            e.Use();
        }
    }

    // ----------------- Highlight Insert Target -----------------
    private void HighlightInsertTarget(EditableLine line, bool useWorld)
    {
        if (line.points.Length < 1) return;

        if (selectedIndex < 0) // no selection → highlight last point
        {
            Vector3 p = useWorld ? line.points[line.points.Length - 1] : line.transform.TransformPoint(line.points[line.points.Length - 1]);
            Handles.color = Color.yellow;
            Handles.SphereHandleCap(0, p, Quaternion.identity, HandleUtility.GetHandleSize(p) * 0.2f, EventType.Repaint);
        }
        else if (selectedIndex == 0)
        {
            Vector3 p = useWorld ? line.points[0] : line.transform.TransformPoint(line.points[0]);

            if (Event.current.alt)
            {
                Handles.color = Color.yellow;
                Handles.SphereHandleCap(0, p, Quaternion.identity, HandleUtility.GetHandleSize(p) * 0.25f, EventType.Repaint);
            }

            // Transparent contextual tip when first point selected
            Vector2 mousePos = Event.current.mousePosition;
            Rect rect = new Rect(mousePos.x + 25, mousePos.y + 20, 320, 30);
            GUIStyle style = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(1f, 1f, 1f, 0.5f) },
                fontSize = 12,
                fontStyle = FontStyle.Italic
            };
            Handles.BeginGUI();
            GUI.Label(rect, "Hold Alt + Shift+Click to insert at start", style);
            Handles.EndGUI();
        }
        else if (selectedIndex < line.points.Length - 1) // normal case → highlight segment
        {
            Vector3 a = useWorld ? line.points[selectedIndex] : line.transform.TransformPoint(line.points[selectedIndex]);
            Vector3 b = useWorld ? line.points[selectedIndex + 1] : line.transform.TransformPoint(line.points[selectedIndex + 1]);

            Handles.color = Color.yellow;
            Handles.DrawAAPolyLine(5f, new Vector3[] { a, b });
        }
    }

    // ----------------- Insert Preview -----------------
    private void DrawInsertPreview(EditableLine line, bool useWorld)
    {
        Event e = Event.current;
        if (!e.shift) return;

        // Physics Raycast
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, Mathf.Infinity))
            return;

        Vector3 worldPos = hit.point;

        int insertIndex;
        if (selectedIndex == 0 && e.alt) // ALT prepend case
            insertIndex = 0;
        else
            insertIndex = selectedIndex >= 0 ? selectedIndex + 1 : line.points.Length;

        Handles.color = new Color(0f, 1f, 0f, 0.5f);

        if (line.points.Length > 0)
        {
            if (insertIndex == 0)
                Handles.DrawDottedLine(worldPos, useWorld ? line.points[0] : line.transform.TransformPoint(line.points[0]), 4f);
            else if (insertIndex == line.points.Length)
                Handles.DrawDottedLine(
                    useWorld ? line.points[line.points.Length - 1] : line.transform.TransformPoint(line.points[line.points.Length - 1]),
                    worldPos, 4f);
            else
            {
                Vector3 prev = useWorld ? line.points[insertIndex - 1] : line.transform.TransformPoint(line.points[insertIndex - 1]);
                Vector3 next = useWorld ? line.points[insertIndex] : line.transform.TransformPoint(line.points[insertIndex]);
                Handles.DrawDottedLine(prev, worldPos, 4f);
                Handles.DrawDottedLine(worldPos, next, 4f);
            }
        }

        float size = HandleUtility.GetHandleSize(worldPos) * 0.15f;
        Handles.SphereHandleCap(0, worldPos, Quaternion.identity, size, EventType.Repaint);

        SceneView.RepaintAll();
    }

    // ----------------- Floating Hint -----------------
    private void ShowFloatingHint(string text, int seconds = 0, float alpha = 1f)
    {
        floatingHint = text;
        floatingHintEndTime = seconds > 0 ? EditorApplication.timeSinceStartup + seconds : double.MaxValue;
        floatingAlpha = alpha;
    }

    private void DrawFloatingHint()
    {
        if (string.IsNullOrEmpty(floatingHint)) return;
        if (EditorApplication.timeSinceStartup > floatingHintEndTime) return;

        Vector2 mousePos = Event.current.mousePosition;
        Rect rect = new Rect(mousePos.x + 25, mousePos.y + 20, 320, 30);

        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = new Color(1f, 1f, 1f, floatingAlpha) },
            fontSize = 12,
            fontStyle = FontStyle.Italic
        };

        Handles.BeginGUI();
        GUI.Label(rect, floatingHint, style);
        Handles.EndGUI();

        SceneView.RepaintAll();
    }

    // ----------------- Shortcut Hints -----------------
    private void DrawShortcutHints()
    {
        Handles.BeginGUI();
        GUIStyle style = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.white }, fontSize = 12 };

        string[] lines = new string[]
        {
            "🖱 Click Sphere = Select Point",
            "🖱 Select + Shift+Click = Insert Point",
            "⌫ Delete = Remove Point",
            "Ctrl+Shift+Z = Undo Last Change",
            "Ctrl+Shift+Y = Redo Last Change",
            "Highlighted Segment/Point = Insert Target",
            "Alt+Shift+Click (first point) = Insert at Start"
        };

        int lineHeight = 18;
        int totalHeight = lines.Length * lineHeight + 10;
        Rect rect = new Rect(10, Screen.height - totalHeight - 30, 420, totalHeight);

        GUILayout.BeginArea(rect);
        foreach (var l in lines)
            GUILayout.Label(l, style);
        GUILayout.EndArea();

        Handles.EndGUI();
    }
}

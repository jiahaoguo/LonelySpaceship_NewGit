using UnityEngine;
using UnityEditor;

public static class CenterTools
{
    [MenuItem("GameObject/Center Tools/Move Parent To Children Center", false, 0)]
    static void MoveParentToChildrenCenter()
    {
        Transform parent = Selection.activeTransform;
        if (parent == null || parent.childCount == 0) return;

        Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "Move Parent To Children Center");

        // Calculate average position of children
        Vector3 avg = Vector3.zero;
        foreach (Transform child in parent)
        {
            avg += child.position;
        }
        avg /= parent.childCount;

        // Move parent to average, offset children so they stay in place
        Vector3 delta = avg - parent.position;
        parent.position = avg;

        foreach (Transform child in parent)
        {
            child.position -= delta;
        }
    }

}

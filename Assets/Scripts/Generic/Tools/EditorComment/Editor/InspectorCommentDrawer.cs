using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InspectorCommentAttribute))]
public class InspectorCommentDrawer : PropertyDrawer
{
    private bool foldout = true;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var comment = ((InspectorCommentAttribute)attribute).comment;
        int lines = Mathf.Max(3, comment.Split('\n').Length);
        return foldout ? EditorGUIUtility.singleLineHeight * (lines + 2)
                       : EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (InspectorCommentAttribute)attribute;
        foldout = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            foldout, "📘 Script Description", true);

        if (foldout)
        {
            var boxRect = new Rect(
                position.x,
                position.y + EditorGUIUtility.singleLineHeight + 2,
                position.width,
                position.height - EditorGUIUtility.singleLineHeight - 4);
            EditorGUI.HelpBox(boxRect, attr.comment, MessageType.Info);
        }
    }
}

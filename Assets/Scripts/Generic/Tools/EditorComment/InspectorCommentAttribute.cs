using UnityEngine;
using System;

/// <summary>
/// Displays a collapsible, non-editable comment box in the Unity Inspector.
/// Example:
/// [InspectorComment("This script loads all ItemData from Resources/ItemData.")]
/// </summary>
public class InspectorCommentAttribute : PropertyAttribute
{
    public string comment;
    public InspectorCommentAttribute(string comment)
    {
        this.comment = comment;
    }
}

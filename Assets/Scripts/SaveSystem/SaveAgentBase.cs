using UnityEngine;

/// <summary>
/// Base class for all save agents. Each agent knows how to capture and restore its own system’s data.
/// </summary>
public abstract class SaveAgentBase : MonoBehaviour
{
    /// <summary>
    /// Returns a unique name for this save section (e.g. "inventory", "player").
    /// </summary>
    public abstract string SectionName { get; }

    /// <summary>
    /// Captures the serializable data for this system.
    /// </summary>
    public abstract object CaptureData();

    /// <summary>
    /// Restores from a previously captured data object.
    /// </summary>
    public abstract void RestoreData(object data);
}

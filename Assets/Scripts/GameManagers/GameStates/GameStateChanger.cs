using UnityEngine;
using System;

public class GameStateChanger : MonoBehaviour
{
    [Tooltip("Which state should this object switch the game to?")]
    public GameState targetState = GameState.UI;

    [Tooltip("Should this change happen automatically on Start?")]
    public bool changeOnStart = false;

    void Start()
    {
        if (changeOnStart && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(targetState);
        }
    }

    /// <summary>
    /// Call this from a UI Button or interactable event.
    /// </summary>
    public void ChangeState()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(targetState);
        }
        else
        {
            Debug.LogWarning("No GameStateManager instance found in scene.");
        }
    }
}

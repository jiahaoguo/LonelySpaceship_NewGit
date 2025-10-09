using UnityEngine;
using System;

public enum GameState
{
    Gameplay,
    UI,
    Paused,
    Cutscene
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Gameplay;

    public static event Action<GameState> OnGameStateChanged;

    // --- Cursor override flag ---
    private bool cursorToggled = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyCursorState();
    }

    public void SetState(GameState newState)
    {
        if (newState == CurrentState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);

        // Handle time freeze/unfreeze
        if (newState == GameState.Gameplay)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;

        ApplyCursorState();
    }

    // --- Called by Input (ESC toggle) ---
    public void ToggleCursor()
    {
        // Only allow manual toggling during gameplay
        if (CurrentState != GameState.Gameplay)
            return;

        cursorToggled = !cursorToggled;
        ApplyCursorState();
    }

    private void ApplyCursorState()
    {
        // UI state always forces unlocked cursor
        if (CurrentState == GameState.UI)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        // Other states: respect toggle flag
        if (cursorToggled)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // --- Button-friendly helpers ---
    public void SetStateToUI() => SetState(GameState.UI);
    public void SetStateToGameplay() => SetState(GameState.Gameplay);

    public void SetStateByInt(int stateIndex)
    {
        if (Enum.IsDefined(typeof(GameState), stateIndex))
            SetState((GameState)stateIndex);
        else
            Debug.LogWarning("Invalid GameState index: " + stateIndex);
    }
}

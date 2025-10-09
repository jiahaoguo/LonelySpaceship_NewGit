using UnityEngine;
using UnityEngine.Events;

public class GameInitializeManager : MonoBehaviour
{
    [Header("Initialization Events")]
    public UnityEvent onInitialize;

    private void Awake()
    {
        // Run initialization events as soon as the game starts
        onInitialize?.Invoke();
    }
}

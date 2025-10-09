using UnityEngine;
using System;

public class InspirationManager : MonoBehaviour
{
    public static InspirationManager Instance { get; private set; }

    [Header("Inspiration Settings")]
    [Range(0, 100)]
    public int InspirationNum = 0;

    // Events
    public event Action OnGatchaOnce;
    public static event Action OnInitialized; // fired when Instance is ready

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Notify others that the manager is alive
        OnInitialized?.Invoke();
    }

    public void GatchaOnce()
    {
        if (InspirationNum < 100)
        {
            Debug.LogWarning("Not enough Inspiration! Need 100.");
            return;
        }

        InspirationNum = 0;
        Debug.Log("Gatcha Once executed!");

        OnGatchaOnce?.Invoke();
    }

    public void AddInspiration(int amount)
    {
        InspirationNum = Mathf.Clamp(InspirationNum + amount, 0, 100);
    }
}

using UnityEngine;

public class BrainDisplayController : MonoBehaviour
{
    [Header("References")]
    public Animator targetAnimator;

    [Header("Animation Settings")]
    public string gatchaTriggerName = "PlayGatcha";

    private void OnEnable()
    {
        // Subscribe if manager already exists
        TrySubscribe();

        // Also listen for late initialization
        InspirationManager.OnInitialized += TrySubscribe;
    }

    private void Start()
    {
        // Extra safety net if OnEnable happened before manager was ready
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (InspirationManager.Instance != null)
            InspirationManager.Instance.OnGatchaOnce -= HandleGatchaOnce;

        InspirationManager.OnInitialized -= TrySubscribe;
    }

    private void TrySubscribe()
    {
        if (InspirationManager.Instance != null)
        {
            // Avoid duplicate subscription
            InspirationManager.Instance.OnGatchaOnce -= HandleGatchaOnce;
            InspirationManager.Instance.OnGatchaOnce += HandleGatchaOnce;
        }
    }

    private void HandleGatchaOnce()
    {
        if (targetAnimator != null && !string.IsNullOrEmpty(gatchaTriggerName))
        {
            targetAnimator.SetTrigger(gatchaTriggerName);
        }
    }
}

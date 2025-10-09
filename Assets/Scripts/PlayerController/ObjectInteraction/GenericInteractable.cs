using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;  
using UnityEngine.Localization.Settings;

public class GenericInteractable : MonoBehaviour, IInteractable
{
    [Header("Prompt")]
    [SerializeField] private LocalizedString promptText; // now localized

    [Header("Events")]
    public UnityEvent onInteract;

    public void Interact()
    {
        onInteract?.Invoke();
    }

    public string GetPromptText()
    {
        string keyName = InteractableManager.Instance != null
            ? InteractableManager.Instance.GetInteractKeyName()
            : "?";

        // Resolve localized string
        string localizedPrompt = promptText.GetLocalizedString();

        return $"{localizedPrompt} ({keyName})";
    }
}

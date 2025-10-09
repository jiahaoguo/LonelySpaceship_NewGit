using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager Instance { get; private set; }

    [Header("References")]
    [Tooltip("Reference to your Player's PlayerInput component")]
    public PlayerInput playerInput;

    [Header("Input")]
    [SerializeField] private InputActionReference interactAction; // your "Interact" action

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public string GetInteractKeyName()
    {
        if (interactAction == null || interactAction.action == null)
            return "?";

        // --- Step 1: check last active device from PlayerInput ---
        InputDevice lastDevice = null;
        if (playerInput != null && playerInput.devices.Count > 0)
        {
            lastDevice = playerInput.devices[0];
        }

        if (lastDevice != null)
        {
            foreach (var binding in interactAction.action.bindings)
            {
                if (string.IsNullOrEmpty(binding.effectivePath))
                    continue;

                // Try to resolve this binding into a control on the device
                var control = InputControlPath.TryFindControl(lastDevice, binding.effectivePath);
                if (control != null)
                {
                    string keyName = InputControlPath.ToHumanReadableString(
                        binding.effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice
                    );

                    // Debug.Log($"[InteractableManager] Using {keyName} for {lastDevice.displayName}");
                    return keyName;
                }
            }
        }

        // --- Step 2: fallback to active control (works once the action is pressed) ---
        var activeControl = interactAction.action.activeControl;
        if (activeControl != null)
        {
            return InputControlPath.ToHumanReadableString(
                activeControl.path,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );
        }

        // --- Step 3: fallback to first binding ---
        if (interactAction.action.bindings.Count > 0)
        {
            return InputControlPath.ToHumanReadableString(
                interactAction.action.bindings[0].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );
        }

        return "?";
    }
}

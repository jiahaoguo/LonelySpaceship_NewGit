using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InventoryInputHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryUI;
    private LoadAnimationPro loadAnim;

    [Header("Events")]
    public UnityEvent onInventoryOpened;
    public UnityEvent onInventoryClosed;

    private bool isOpen = false;

    private void Awake()
    {
        if (inventoryUI != null)
            loadAnim = inventoryUI.GetComponent<LoadAnimationPro>();
    }

    public void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (isOpen)
        {
            if (loadAnim != null)
                loadAnim.LoadOut(); // graceful closing
            else
                inventoryUI.SetActive(false); // fallback if animation missing

            onInventoryClosed?.Invoke();
        }
        else
        {
            inventoryUI.SetActive(true); // triggers LoadIn automatically via OnEnable
            onInventoryOpened?.Invoke();
        }

        isOpen = !isOpen;
        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}

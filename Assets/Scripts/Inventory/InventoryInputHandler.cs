using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

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
                loadAnim.LoadOut();
            else
                inventoryUI.SetActive(false);

            onInventoryClosed?.Invoke();
        }
        else
        {
            inventoryUI.SetActive(true);
            onInventoryOpened?.Invoke();
        }

        isOpen = !isOpen;

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}

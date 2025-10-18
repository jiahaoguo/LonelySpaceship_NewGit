using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InventoryInputHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject inventoryUI;
    public GameObject hotbarUI;
    private LoadAnimationPro loadAnim;
    private LoadAnimationPro hotBarLoadAnim;
    [SerializeField] private InventoryUIController inventoryUIController;

    [Header("Events")]
    public UnityEvent onInventoryOpened;
    public UnityEvent onInventoryClosed;

    private bool isOpen;

    private void Awake()
    {
        if (inventoryUI) loadAnim = inventoryUI.GetComponent<LoadAnimationPro>();
        if (hotbarUI) hotBarLoadAnim = hotbarUI.GetComponent<LoadAnimationPro>();
    }

    public void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        isOpen = !isOpen;

        if (isOpen)
        {
            // --- Opening full inventory ---
            for (int i = 0; i < inventoryUIController.hotbarSlots.Length; i++)
            {
                if (inventoryUIController.hotbarSlots[i].gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    inventoryUIController.hotbarSelector.lastSelectedIndex = i;
                    break;
                }
            }

            inventoryUI.SetActive(true);
            hotBarLoadAnim?.LoadOut();
            onInventoryOpened?.Invoke();
            InventoryUISelection.Instance?.Hide();
            EventSystem.current?.SetSelectedGameObject(null);
        }
        else
        {
            // --- Closing inventory ---
            if (loadAnim) loadAnim.LoadOut();
            else inventoryUI.SetActive(false);

            hotbarUI.SetActive(true);
            onInventoryClosed?.Invoke();

            EventSystem.current?.SetSelectedGameObject(
                inventoryUIController.hotbarSlots[inventoryUIController.hotbarSelector.lastSelectedIndex].gameObject
            );
        }

        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}

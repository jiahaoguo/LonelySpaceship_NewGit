using UnityEngine;
using UnityEngine.UI;

public class InventoryCursorController : MonoBehaviour
{
    [Header("References")]
    public InventoryManager manager;
    public Image cursorIcon; // a UI Image following the mouse
    public Canvas canvas;

    private InventorySlot heldSlot;
    private bool isHoldingItem;

    private void OnEnable()
    {
        InventoryUISelection.Instance.onSlotSelected.AddListener(OnSlotSelected);
    }

    private void OnDisable()
    {
        InventoryUISelection.Instance.onSlotSelected.RemoveListener(OnSlotSelected);
    }

    private void Update()
    {
        if (isHoldingItem)
        {
            // Make the cursor icon follow the mouse
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out pos);
            cursorIcon.rectTransform.anchoredPosition = pos;
        }
    }

    private void OnSlotSelected(int index)
    {
        // Only handle full-inventory clicks (ignore hotbar)
        if (!IsFullInventorySlot(index))
            return;

        InventorySlot slot = manager.slots[index];

        // --- Pick up ---
        if (!isHoldingItem && !slot.IsEmpty)
        {
            heldSlot = new InventorySlot(slot.item, slot.quantity);
            slot.Clear();
            cursorIcon.sprite = slot.item.icon;
            cursorIcon.enabled = true;
            isHoldingItem = true;
            manager.onInventoryChanged?.Invoke();
        }
        // --- Place down ---
        else if (isHoldingItem)
        {
            // Place held item into clicked slot
            manager.slots[index] = heldSlot;
            cursorIcon.enabled = false;
            isHoldingItem = false;
            heldSlot = null;
            manager.onInventoryChanged?.Invoke();
        }
    }

    private bool IsFullInventorySlot(int index)
    {
        // Assuming hotbar = first X slots, rest = inventory
        int hotbarSize = 9;
        return index >= hotbarSize;
    }
}

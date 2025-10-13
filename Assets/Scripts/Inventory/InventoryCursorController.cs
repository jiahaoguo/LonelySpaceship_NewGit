using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCursorController : MonoBehaviour
{
    [Header("References")]
    public InventoryManager manager;
    public Image cursorIcon;           // the icon following the mouse
    public TextMeshProUGUI numberText; // quantity text under icon
    public Canvas canvas;              // canvas for positioning

    private InventorySlot heldSlot;
    private bool isHoldingItem;

    private void Awake()
    {
        HideCursorCarry();
    }

    private void Update()
    {
        if (!canvas) return;

        // Make cursor icon follow mouse
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out pos);
        if (cursorIcon != null)
            cursorIcon.rectTransform.anchoredPosition = pos;
    }

    /// <summary>
    /// Called by InventoryUIController when a slot is clicked.
    /// </summary>
    public void OnSlotClicked(int index)
    {
        if (manager == null || index < 0 || index >= manager.slots.Count)
            return;

        var slot = manager.slots[index];

        // --- Pickup ---
        if (!isHoldingItem)
        {
            if (!slot.IsEmpty)
            {
                heldSlot = new InventorySlot(slot.item, slot.quantity);
                ShowCursorCarry(heldSlot);
                slot.Clear();
                isHoldingItem = true;
                manager.onInventoryChanged?.Invoke();
            }
            return;
        }

        // --- Place down ---
        var targetSlot = manager.slots[index];

        // Case 1: same type, stackable → merge
        if (!targetSlot.IsEmpty && targetSlot.item == heldSlot.item && targetSlot.item.stackable)
        {
            int total = targetSlot.quantity + heldSlot.quantity;
            int maxStack = targetSlot.item.maxStack;

            if (total <= maxStack)
            {
                targetSlot.quantity = total;
                ClearCursorCarry();
            }
            else
            {
                targetSlot.quantity = maxStack;
                heldSlot.quantity = total - maxStack;
                UpdateCursorQuantity(heldSlot);
            }
        }
        // Case 2: different type → swap
        else if (!targetSlot.IsEmpty && targetSlot.item != heldSlot.item)
        {
            InventorySlot temp = new InventorySlot(targetSlot.item, targetSlot.quantity);
            manager.slots[index] = new InventorySlot(heldSlot.item, heldSlot.quantity);

            heldSlot = temp;
            UpdateCursorCarry(heldSlot);
        }
        // Case 3: empty slot → place
        else if (targetSlot.IsEmpty)
        {
            manager.slots[index] = new InventorySlot(heldSlot.item, heldSlot.quantity);
            ClearCursorCarry();
        }

        manager.onInventoryChanged?.Invoke();
    }

    // --- Cursor UI helpers ---
    private void ShowCursorCarry(InventorySlot data)
    {
        if (cursorIcon != null)
        {
            cursorIcon.enabled = true;
            cursorIcon.sprite = data.item.icon;
        }

        UpdateCursorQuantity(data);
    }

    private void UpdateCursorCarry(InventorySlot data)
    {
        if (cursorIcon != null)
            cursorIcon.sprite = data.item.icon;

        UpdateCursorQuantity(data);
        isHoldingItem = data != null && data.item != null && data.quantity > 0;
    }

    private void UpdateCursorQuantity(InventorySlot data)
    {
        if (numberText == null) return;

        if (data.item.stackable && data.quantity > 1)
        {
            numberText.enabled = true;
            numberText.text = data.quantity.ToString();
        }
        else
        {
            numberText.enabled = false;
            numberText.text = "";
        }
    }

    private void HideCursorCarry()
    {
        if (cursorIcon != null) cursorIcon.enabled = false;
        if (numberText != null)
        {
            numberText.enabled = false;
            numberText.text = "";
        }
    }

    private void ClearCursorCarry()
    {
        heldSlot = null;
        isHoldingItem = false;
        HideCursorCarry();
    }
}

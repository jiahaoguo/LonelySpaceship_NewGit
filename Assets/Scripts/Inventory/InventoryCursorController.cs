﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-100)]
public class InventoryCursorController : MonoBehaviour
{
    public static InventoryCursorController Instance;

    [Header("References")]
    public InventoryManager manager;
    public InventoryUIController uiController;
    public Image cursorIcon;
    public TextMeshProUGUI numberText;
    public Canvas canvas;

    private InventorySlot heldSlot;
    private bool isHoldingItem;

    private float lastClickTime = 0f;
    private int lastClickedIndex = -1;
    [SerializeField] private float doubleClickThreshold = 0.25f;

    private void Awake()
    {
        Instance = this;
        HideCursorCarry();
    }

    private void Update()
    {
        if (!canvas) return;

        // Follow mouse
        if (cursorIcon)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector2 pos);
            cursorIcon.rectTransform.anchoredPosition = pos;
        }
    }

    public bool IsHoldingItem => isHoldingItem;

    // ============================================================
    // MAIN CLICK HANDLING
    // ============================================================

    public void OnSlotClicked(int index, PointerEventData.InputButton button)
    {
        if (manager == null || index < 0 || index >= manager.slots.Count) return;

        bool rightClick = (button == PointerEventData.InputButton.Right);
        bool leftClick = (button == PointerEventData.InputButton.Left);
        var slot = manager.slots[index];
        if (slot == null) return;

        bool isDoubleClick = (index == lastClickedIndex && Time.time - lastClickTime <= doubleClickThreshold);
        lastClickTime = Time.time;
        lastClickedIndex = index;

        // --- Double click: gather all of same type ---
        if (isDoubleClick && leftClick)
        {
            if (!isHoldingItem && !slot.IsEmpty)
            {
                heldSlot = new InventorySlot(slot.item, slot.quantity);
                slot.Clear();
                ShowCursorCarry(heldSlot);
                isHoldingItem = true;
            }

            TryGatherAllToHand();
            manager.onInventoryChanged?.Invoke();
            uiController?.RefreshAll();
            return;
        }

        // --- Pick up item ---
        if (!isHoldingItem)
        {
            if (!slot.IsEmpty)
            {
                if (rightClick)
                {
                    int half = Mathf.CeilToInt(slot.quantity / 2f);
                    heldSlot = new InventorySlot(slot.item, half);
                    slot.quantity -= half;
                    if (slot.quantity <= 0) slot.Clear();
                }
                else
                {
                    heldSlot = new InventorySlot(slot.item, slot.quantity);
                    slot.Clear();
                }

                ShowCursorCarry(heldSlot);
                isHoldingItem = true;
                manager.onInventoryChanged?.Invoke();
                uiController?.RefreshSingleSlot(index);
            }
            return;
        }

        // --- Place or merge ---
        var target = manager.slots[index];
        if (target == null) return;

        if (rightClick)
        {
            HandleRightClick(target, index);
            return;
        }

        HandleLeftClick(target, index);
    }

    // ============================================================
    // LEFT & RIGHT CLICK LOGIC
    // ============================================================

    private void HandleRightClick(InventorySlot target, int index)
    {
        if (heldSlot == null) return;

        if (target.IsEmpty)
        {
            target.item = heldSlot.item;
            target.quantity = 1;
            heldSlot.quantity -= 1;
        }
        else if (target.item == heldSlot.item && target.item.stackable && target.quantity < target.item.maxStack)
        {
            target.quantity += 1;
            heldSlot.quantity -= 1;
        }
        else
        {
            InventorySlot temp = new InventorySlot(target.item, target.quantity);
            manager.slots[index] = new InventorySlot(heldSlot.item, heldSlot.quantity);
            heldSlot = temp;
        }

        if (heldSlot.quantity <= 0 || heldSlot.item == null)
            ClearCursorCarry();
        else
            UpdateCursorQuantity(heldSlot);

        manager.onInventoryChanged?.Invoke();
        uiController?.RefreshSingleSlot(index);
    }

    private void HandleLeftClick(InventorySlot target, int index)
    {
        if (heldSlot == null) return;

        if (!target.IsEmpty && target.item == heldSlot.item && target.item.stackable)
        {
            int total = target.quantity + heldSlot.quantity;
            int max = target.item.maxStack;

            if (total <= max)
            {
                target.quantity = total;
                ClearCursorCarry();
            }
            else
            {
                target.quantity = max;
                heldSlot.quantity = total - max;
                UpdateCursorQuantity(heldSlot);
            }
        }
        else if (!target.IsEmpty && target.item != heldSlot.item)
        {
            InventorySlot temp = new InventorySlot(target.item, target.quantity);
            manager.slots[index] = new InventorySlot(heldSlot.item, heldSlot.quantity);
            heldSlot = temp;
            UpdateCursorCarry(heldSlot);
        }
        else if (target.IsEmpty)
        {
            manager.slots[index] = new InventorySlot(heldSlot.item, heldSlot.quantity);
            ClearCursorCarry();
        }

        manager.onInventoryChanged?.Invoke();
        uiController?.RefreshSingleSlot(index);
    }

    // ============================================================
    // UTILITY / CURSOR DISPLAY
    // ============================================================

    private void TryGatherAllToHand()
    {
        if (!isHoldingItem || heldSlot == null || heldSlot.item == null || !heldSlot.item.stackable)
            return;

        int max = heldSlot.item.maxStack;
        int current = heldSlot.quantity;

        for (int i = 0; i < manager.slots.Count && current < max; i++)
        {
            var s = manager.slots[i];
            if (s.IsEmpty) continue;

            // ✅ Only gather from same item type
            if (s.item != heldSlot.item) continue;

            // ✅ Skip if that slot is already a max stack
            if (s.quantity >= s.item.maxStack) continue;

            // Move as much as needed to reach max
            int move = Mathf.Min(max - current, s.quantity);
            current += move;
            s.quantity -= move;

            if (s.quantity <= 0)
                s.Clear();

            uiController?.RefreshSingleSlot(i);
        }

        heldSlot.quantity = Mathf.Min(current, max);
        UpdateCursorQuantity(heldSlot);
        manager.onInventoryChanged?.Invoke();
    }


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
        if (data == null) { ClearCursorCarry(); return; }
        if (cursorIcon != null)
            cursorIcon.sprite = data.item.icon;
        UpdateCursorQuantity(data);
        isHoldingItem = data.item != null && data.quantity > 0;
    }

    private void UpdateCursorQuantity(InventorySlot data)
    {
        if (!numberText || data == null) return;

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
        if (cursorIcon)
            cursorIcon.enabled = false;

        if (numberText)
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

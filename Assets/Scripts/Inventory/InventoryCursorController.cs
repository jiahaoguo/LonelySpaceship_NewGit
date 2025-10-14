using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

    private bool isDraggingDistribution = false;
    private PointerEventData.InputButton dragButton;
    private HashSet<int> draggedSlots = new HashSet<int>();

    private void Awake()
    {
        Instance = this;
        HideCursorCarry();
    }

    private void Update()
    {
        if (!canvas) return;

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out pos);
        if (cursorIcon)
            cursorIcon.rectTransform.anchoredPosition = pos;

        if (isDraggingDistribution)
        {
            bool release = (dragButton == PointerEventData.InputButton.Left && Input.GetMouseButtonUp(0)) ||
                           (dragButton == PointerEventData.InputButton.Right && Input.GetMouseButtonUp(1));
            if (release)
                FinishDragDistribution();
        }
    }

    public bool IsHoldingItem => isHoldingItem;

    public void OnSlotClicked(int index, PointerEventData.InputButton button)
    {
        if (manager == null || index < 0 || index >= manager.slots.Count) return;

        bool rightClick = (button == PointerEventData.InputButton.Right);
        bool leftClick = (button == PointerEventData.InputButton.Left);
        var slot = manager.slots[index];

        bool isDoubleClick = (index == lastClickedIndex && Time.time - lastClickTime <= doubleClickThreshold);
        lastClickTime = Time.time;
        lastClickedIndex = index;

        if (isDoubleClick && leftClick && isHoldingItem)
        {
            manager.slots[index].Clear();
            uiController?.RefreshSingleSlot(index);

            TryGatherAllToHand();
            manager.onInventoryChanged?.Invoke();
            return;
        }

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

        var target = manager.slots[index];

        if (rightClick)
        {
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

            if (heldSlot.quantity <= 0) ClearCursorCarry(); else UpdateCursorQuantity(heldSlot);
            manager.onInventoryChanged?.Invoke();
            uiController?.RefreshSingleSlot(index);
            return;
        }

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

    public void OnSlotPointerDown(int index, PointerEventData.InputButton button)
    {
        if (!isHoldingItem) return;
        isDraggingDistribution = true;
        dragButton = button;
        draggedSlots.Clear();
        draggedSlots.Add(index);
    }

    public void OnDragOverSlot(int index)
    {
        if (!isDraggingDistribution) return;
        if (draggedSlots.Add(index))
            ApplyDragToSlot(index);
    }

    private void ApplyDragToSlot(int index)
    {
        if (!isDraggingDistribution || !isHoldingItem) return;

        var slot = manager.slots[index];
        if (heldSlot.quantity <= 0) return;

        if (slot.IsEmpty)
        {
            slot.item = heldSlot.item;
            slot.quantity = 1;
            heldSlot.quantity -= 1;
        }
        else if (slot.item == heldSlot.item && slot.item.stackable && slot.quantity < slot.item.maxStack)
        {
            slot.quantity += 1;
            heldSlot.quantity -= 1;
        }

        UpdateCursorQuantity(heldSlot);
        manager.onInventoryChanged?.Invoke();
        uiController?.RefreshSingleSlot(index);
    }

    private void FinishDragDistribution()
    {
        isDraggingDistribution = false;
        if (draggedSlots.Count == 0 || !isHoldingItem) return;

        if (dragButton == PointerEventData.InputButton.Left)
        {
            int totalSlots = draggedSlots.Count;
            int totalTargets = totalSlots + 1;
            int baseAmount = heldSlot.quantity / totalTargets;
            int remainder = heldSlot.quantity % totalTargets;

            foreach (int i in draggedSlots)
            {
                if (heldSlot.quantity <= 0) break;
                var s = manager.slots[i];
                int amount = baseAmount + (remainder-- > 0 ? 1 : 0);
                if (amount <= 0) continue;

                if (s.IsEmpty)
                    s.item = heldSlot.item;

                if (s.item == heldSlot.item && s.item.stackable)
                {
                    int canAdd = Mathf.Min(amount, s.item.maxStack - s.quantity);
                    s.quantity += canAdd;
                    heldSlot.quantity -= canAdd;
                }

                uiController?.RefreshSingleSlot(i);
            }
        }

        if (heldSlot.quantity <= 0) ClearCursorCarry(); else UpdateCursorQuantity(heldSlot);
        manager.onInventoryChanged?.Invoke();
    }

    private void TryGatherAllToHand()
    {
        if (!isHoldingItem || heldSlot.item == null || !heldSlot.item.stackable) return;

        int max = heldSlot.item.maxStack;
        int current = heldSlot.quantity;

        for (int i = 0; i < manager.slots.Count && current < max; i++)
        {
            var s = manager.slots[i];
            if (!s.IsEmpty && s.item == heldSlot.item)
            {
                int move = Mathf.Min(max - current, s.quantity);
                current += move;
                s.quantity -= move;
                if (s.quantity <= 0) s.Clear();
                uiController?.RefreshSingleSlot(i);
            }
        }

        heldSlot.quantity = current;
        UpdateCursorQuantity(heldSlot);
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
        if (cursorIcon != null)
            cursorIcon.sprite = data.item.icon;
        UpdateCursorQuantity(data);
        isHoldingItem = data != null && data.item != null && data.quantity > 0;
    }

    private void UpdateCursorQuantity(InventorySlot data)
    {
        if (!numberText) return;

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

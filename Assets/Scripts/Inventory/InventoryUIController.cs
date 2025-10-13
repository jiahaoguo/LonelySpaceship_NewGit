﻿using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    public InventoryManager manager;

    [Tooltip("Parent Transform that contains the hotbar SlotUI objects (first line).")]
    public Transform hotbarParent;

    [Tooltip("Parent Transform that contains all SlotUI objects for the full inventory (including the hotbar).")]
    public Transform inventoryParent;

    [Header("Interaction")]
    public InventoryCursorController cursorController; // assign in inspector if possible

    private SlotUI[] hotbarSlots;
    private SlotUI[] inventorySlots;

    private void Awake()
    {
        if (hotbarParent != null)
            hotbarSlots = hotbarParent.GetComponentsInChildren<SlotUI>(includeInactive: true);
        else
            Debug.LogWarning("InventoryUIController: Hotbar parent not assigned.");

        if (inventoryParent != null)
            inventorySlots = inventoryParent.GetComponentsInChildren<SlotUI>(includeInactive: true);
        else
            Debug.LogWarning("InventoryUIController: Inventory parent not assigned.");

        if (cursorController == null)
            cursorController = FindObjectOfType<InventoryCursorController>();
    }

    private void OnEnable()
    {
        if (manager != null)
            manager.onInventoryChanged.AddListener(RefreshAll);
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.onInventoryChanged.RemoveListener(RefreshAll);
    }

    private void Start()
    {
        if (manager == null)
        {
            Debug.LogWarning("InventoryUIController: Missing InventoryManager reference!");
            return;
        }

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (manager == null) return;

        // --- Refresh Hotbar ---
        if (hotbarSlots != null)
        {
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                InventorySlot slotData = i < manager.slots.Count ? manager.slots[i] : null;
                var slotUI = hotbarSlots[i];
                slotUI.SetSlot(slotData);

                // Bind selection visual/focus
                BindJiahaoButtonEvents(slotUI, i);

                // Bind click (pickup/place)
                BindClick(slotUI, i);
            }
        }

        // --- Refresh Full Inventory (includes Hotbar as first row) ---
        if (inventorySlots != null)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                InventorySlot slotData = i < manager.slots.Count ? manager.slots[i] : null;
                var slotUI = inventorySlots[i];
                slotUI.SetSlot(slotData);

                // Bind selection visual/focus
                BindJiahaoButtonEvents(slotUI, i);

                // Bind click (pickup/place)
                BindClick(slotUI, i);
            }
        }
    }

    private void BindClick(SlotUI slotUI, int index)
    {
        slotUI.Initialize(index);
        slotUI.OnSlotClicked.RemoveAllListeners();
        slotUI.OnSlotClicked.AddListener((clickedIndex) =>
        {
            if (cursorController != null)
                cursorController.OnSlotClicked(clickedIndex);
        });
    }

    private void BindJiahaoButtonEvents(SlotUI slotUI, int index)
    {
        var jButton = slotUI.GetComponent<JiahaoButton>();
        if (jButton != null)
        {
            jButton.OnSelectEvent.RemoveAllListeners();
            jButton.OnSelectEvent.AddListener(() =>
            {
                InventoryUISelection.Instance?.OnSlotSelected(index);
            });

            jButton.OnDeselectEvent.RemoveAllListeners();
            jButton.OnDeselectEvent.AddListener(() =>
            {
                InventoryUISelection.Instance?.OnSlotDeselected(index);
            });
        }
    }
}

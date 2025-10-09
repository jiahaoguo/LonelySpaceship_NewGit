using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles saving and loading of inventory data without modifying the InventoryManager itself.
/// </summary>
public class InventorySaveAgent : SaveAgentBase
{
    [Header("References")]
    public InventoryManager inventoryManager;
    public ItemLibrary itemLibrary;

    [Serializable]
    public class InventorySaveData
    {
        [Serializable]
        public class SlotData
        {
            public string itemName;
            public int quantity;
        }

        public List<SlotData> slots = new();
    }

    public override string SectionName => "inventory";

    public override object CaptureData()
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("⚠ InventorySaveAgent: Missing InventoryManager reference.");
            return null;
        }

        InventorySaveData data = new InventorySaveData();
        foreach (var slot in inventoryManager.slots)
        {
            var slotData = new InventorySaveData.SlotData
            {
                itemName = slot.item != null ? slot.item.name : string.Empty,
                quantity = slot.quantity
            };
            data.slots.Add(slotData);
        }

        return data;
    }

    public override void RestoreData(object data)
    {
        Debug.Log("=== [InventorySaveAgent] Starting inventory restore ===");

        if (inventoryManager == null)
        {
            Debug.LogWarning("[InventorySaveAgent] ⚠ Missing InventoryManager reference. Aborting restore.");
            return;
        }

        if (data is not InventorySaveData invData)
        {
            Debug.LogError("[InventorySaveAgent] ❌ Invalid data type provided to RestoreData.");
            return;
        }

        // Clear current inventory
        inventoryManager.slots.Clear();
        Debug.Log($"[InventorySaveAgent] Cleared existing slots. Restoring {invData.slots.Count} slots...");

        int restoredCount = 0;

        foreach (var slotData in invData.slots)
        {
            var slot = new InventorySlot();

            Debug.Log("Slot item name is " + slotData.itemName);
            if (!string.IsNullOrEmpty(slotData.itemName))
            {
                slot.item = itemLibrary != null ? itemLibrary.GetItemByName(slotData.itemName) : null;

                if (slot.item == null)
                    Debug.LogWarning($"[InventorySaveAgent] ⚠ Item '{slotData.itemName}' not found in ItemLibrary.");
                else
                    Debug.Log($"[InventorySaveAgent] ✓ Found item '{slot.item.name}' in library.");
            }
            else
            {
                Debug.Log("[InventorySaveAgent] Slot has no item name, skipping item assignment.");
            }

            slot.quantity = slotData.quantity;
            inventoryManager.slots.Add(slot);
            restoredCount++;

            Debug.Log($"[InventorySaveAgent] Added slot #{restoredCount}: Item = {(slot.item != null ? slot.item.name : "NULL")}, Quantity = {slot.quantity}");
        }

        Debug.Log($"=== [InventorySaveAgent] Inventory restore complete. Total slots restored: {restoredCount} ===");
    }

}

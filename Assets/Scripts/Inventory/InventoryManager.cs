using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public bool IsEmpty => item == null || quantity <= 0;

    public InventorySlot() { }

    public InventorySlot(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public void Clear()
    {
        item = null;
        quantity = 0;
    }
}
[ExecuteAlways] // allows running in Edit Mode
public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    public int inventorySize = 36;
    public List<InventorySlot> slots = new List<InventorySlot>();

    [Header("Inventory Events")]
    public UnityEvent onInventoryChanged;

    void Awake() => EnsureSlots();

#if UNITY_EDITOR
    private void OnValidate() => EnsureSlots();
#endif

    private void EnsureSlots()
    {
        if (slots == null)
            slots = new List<InventorySlot>();

        while (slots.Count < inventorySize)
            slots.Add(new InventorySlot());

        if (slots.Count > inventorySize)
            slots.RemoveRange(inventorySize, slots.Count - inventorySize);
    }

    /// <summary>
    /// Adds an item. Returns true if successful.
    /// </summary>
    public bool AddItem(ItemData item, int amount = 1)
    {
        EnsureSlots();
        if (item == null || amount <= 0) return false;

        bool changed = false;

        // Stack existing
        if (item.stackable)
        {
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.item == item && slot.quantity < item.maxStack)
                {
                    int canAdd = Mathf.Min(amount, item.maxStack - slot.quantity);
                    slot.quantity += canAdd;
                    amount -= canAdd;
                    changed = true;
                    if (amount <= 0) break;
                }
            }
        }

        // Fill empty
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                int toAdd = Mathf.Min(amount, item.maxStack);
                slot.item = item;
                slot.quantity = toAdd;
                amount -= toAdd;
                changed = true;
                if (amount <= 0) break;
            }
        }

        if (changed)
        {
            EditorMarkDirty();
            onInventoryChanged?.Invoke();
        }

        if (amount > 0)
            Debug.LogWarning("⚠ Inventory full or could not add all items.");

        return changed;
    }

    /// <summary>
    /// Removes an item. Returns true if successful.
    /// </summary>
    public bool RemoveItem(ItemData item, int amount = 1)
    {
        bool changed = false;

        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.item == item)
            {
                if (slot.quantity > amount)
                {
                    slot.quantity -= amount;
                    changed = true;
                    break;
                }
                else
                {
                    amount -= slot.quantity;
                    slot.Clear();
                    changed = true;
                    if (amount <= 0) break;
                }
            }
        }

        if (changed)
        {
            EditorMarkDirty();
            onInventoryChanged?.Invoke();
        }

        return changed;
    }

    /// <summary>
    /// Clears all inventory slots.
    /// </summary>
    public void ClearAll()
    {
        foreach (var slot in slots)
            slot.Clear();

        EditorMarkDirty();
        onInventoryChanged?.Invoke();
    }

    public int GetItemCount(ItemData item)
    {
        int total = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.item == item)
                total += slot.quantity;
        }
        return total;
    }

#if UNITY_EDITOR
    private void EditorMarkDirty()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
#endif
}
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    public InventoryManager manager;
    public Transform hotbarParent;
    public Transform inventoryParent;
    public InventoryCursorController cursorController;

    [HideInInspector] public SlotUI[] hotbarSlots;
    [HideInInspector] public SlotUI[] inventorySlots;

    private void Awake()
    {
        if (hotbarParent)
            hotbarSlots = hotbarParent.GetComponentsInChildren<SlotUI>(includeInactive: true);

        if (inventoryParent)
            inventorySlots = inventoryParent.GetComponentsInChildren<SlotUI>(includeInactive: true);

        if (cursorController == null)
            cursorController = FindObjectOfType<InventoryCursorController>();
    }

    private void OnEnable()
    {
        if (manager != null)
            manager.onInventoryChanged.AddListener(RefreshAll);
        RefreshAll();
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.onInventoryChanged.RemoveListener(RefreshAll);
    }

    public void RefreshAll()
    {
        if (manager == null) return;

        if (hotbarSlots != null)
            for (int i = 0; i < hotbarSlots.Length; i++)
                BindSlot(hotbarSlots[i], i);

        if (inventorySlots != null)
            for (int i = 0; i < inventorySlots.Length; i++)
                BindSlot(inventorySlots[i], i);
    }

    private void BindSlot(SlotUI slotUI, int index)
    {
        if (slotUI == null) return;

        InventorySlot slotData = index < manager.slots.Count ? manager.slots[index] : null;
        slotUI.SetSlot(slotData);
        slotUI.SetIndex(index);
        slotUI.Refresh();

        slotUI.OnSlotPointerClicked.RemoveAllListeners();
        slotUI.OnSlotPointerClicked.AddListener((clickedIndex, button) =>
        {
            cursorController?.OnSlotClicked(clickedIndex, button);
        });
    }

    public void RefreshSingleSlot(int index)
    {
        if (manager == null) return;
        if (index < 0 || index >= manager.slots.Count) return;

        if (hotbarSlots != null && index < hotbarSlots.Length && hotbarSlots[index])
        {
            hotbarSlots[index].SetSlot(manager.slots[index]);
            hotbarSlots[index].Refresh();
        }

        if (inventorySlots != null && index < inventorySlots.Length && inventorySlots[index])
        {
            inventorySlots[index].SetSlot(manager.slots[index]);
            inventorySlots[index].Refresh();
        }
    }
}

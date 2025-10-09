using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    [Header("References")]
    public InventoryManager manager;
    [Tooltip("Parent Transform that contains all SlotUI objects.")]
    public Transform hotbarParent;

    private SlotUI[] hotbarSlots;

    private void Awake()
    {
        if (hotbarParent != null)
            hotbarSlots = hotbarParent.GetComponentsInChildren<SlotUI>(includeInactive: true);
        else
            Debug.LogWarning("InventoryUIController: Hotbar parent not assigned.");
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
        if (manager == null || hotbarSlots == null)
            return;

        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            InventorySlot slotData = i < manager.slots.Count ? manager.slots[i] : null;
            hotbarSlots[i].SetSlot(slotData);

            // --- Auto-bind JiahaoButton selection events ---
            var jButton = hotbarSlots[i].GetComponent<JiahaoButton>();
            if (jButton != null)
            {
                int index = i;

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
}

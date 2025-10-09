using UnityEngine;
using UnityEngine.Events;

public class InventoryUISelection : MonoBehaviour
{
    public static InventoryUISelection Instance { get; private set; }

    [Header("Selection State")]
    public int currentSelectedIndex = -1;

    [System.Serializable]
    public class SlotSelectEvent : UnityEvent<int> { }
    public SlotSelectEvent onSlotSelected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OnSlotSelected(int index)
    {
        currentSelectedIndex = index;
        onSlotSelected?.Invoke(index);
        Debug.Log($"[InventoryUISelection] Selected slot index: {index}");
    }

    public void OnSlotDeselected(int index)
    {
        if (currentSelectedIndex == index)
            currentSelectedIndex = -1;
    }
}

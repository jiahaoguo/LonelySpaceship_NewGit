using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class SlotUI : MonoBehaviour
{
    [Header("References")]
    public Image icon;
    public TextMeshProUGUI quantityText;
    public Button button; // If not set, will auto-get in Awake

    [System.Serializable] public class IntEvent : UnityEvent<int> { }
    public IntEvent OnSlotClicked = new IntEvent();

    private int slotIndex = -1;
    private InventorySlot slotData;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    /// <summary>Called by UI controller to assign the global slot index and (re)bind click.</summary>
    public void Initialize(int index)
    {
        slotIndex = index;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (slotIndex >= 0)
                    OnSlotClicked.Invoke(slotIndex);
            });
        }
    }

    public void SetSlot(InventorySlot newSlot)
    {
        slotData = newSlot;
        Refresh();
    }

    public void Refresh()
    {
        if (slotData == null || slotData.IsEmpty)
        {
            if (icon != null) icon.enabled = false;
            if (quantityText != null) quantityText.text = "";
            return;
        }

        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = slotData.item.icon;
        }

        if (quantityText != null)
        {
            // Show number only if stackable and >1
            quantityText.text = (slotData.item.stackable && slotData.quantity > 1)
                ? slotData.quantity.ToString()
                : "";
        }
    }
}

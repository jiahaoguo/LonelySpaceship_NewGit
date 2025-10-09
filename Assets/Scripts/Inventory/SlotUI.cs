using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    [Header("References")]
    public Image icon;
    public TextMeshProUGUI quantityText;

    private InventorySlot slotData;

    public void SetSlot(InventorySlot newSlot)
    {
        slotData = newSlot;
        Refresh();
    }

    public void Refresh()
    {
        if (slotData == null || slotData.IsEmpty)
        {
            icon.enabled = false;
            quantityText.text = "";
            return;
        }

        icon.enabled = true;
        icon.sprite = slotData.item.icon;

        // Quantity display: only show number if >1
        quantityText.text = slotData.item.stackable && slotData.quantity > 1
            ? slotData.quantity.ToString()
            : "";
    }
}

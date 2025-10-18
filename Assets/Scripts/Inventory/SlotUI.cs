using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI quantityText;

    [HideInInspector] public int slotIndex;
    [HideInInspector] public bool isHotbarSlot;   // let controller mark this
    public bool isSelected { get; private set; }  // 🔹 new flag

    [System.Serializable]
    public class SlotClickEvent : UnityEngine.Events.UnityEvent<int, PointerEventData.InputButton> { }
    public SlotClickEvent OnSlotPointerClicked = new SlotClickEvent();

    private InventorySlot slot;

    public void SetSlot(InventorySlot slotData) => slot = slotData;
    public void SetIndex(int index) => slotIndex = index;

    public void Refresh()
    {
        if (slot == null || slot.item == null || slot.quantity <= 0)
        {
            icon.enabled = false;
            quantityText.enabled = false;
        }
        else
        {
            icon.enabled = true;
            icon.sprite = slot.item.icon;
            if (slot.item.stackable && slot.quantity > 1)
            {
                quantityText.enabled = true;
                quantityText.text = slot.quantity.ToString();
            }
            else
            {
                quantityText.enabled = false;
                quantityText.text = "";
            }
        }
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
        Debug.Log("set selected");
        if (isSelected)
            InventoryUISelection.Instance?.OnSlotSelected(slotIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotPointerClicked.Invoke(slotIndex, eventData.button);
    }
}

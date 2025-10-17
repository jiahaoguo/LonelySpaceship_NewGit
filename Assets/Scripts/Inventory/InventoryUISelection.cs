using UnityEngine;
using UnityEngine.UI;

public class InventoryUISelection : MonoBehaviour
{
    public static InventoryUISelection Instance;

    public Image highlightImage;
    private RectTransform rect;

    private void Awake()
    {
        Instance = this;
        rect = highlightImage != null ? highlightImage.GetComponent<RectTransform>() : null;
        if (highlightImage != null)
        {
            highlightImage.enabled = false;
        }
    }

    public void OnSlotSelected(int index)
    {
        if (index < 0) return;

        SlotUI[] slots = FindObjectsOfType<SlotUI>();
        foreach (var s in slots)
        {
            if (s.slotIndex == index)
            {
                highlightImage.enabled = true;
                rect.position = s.GetComponent<RectTransform>().position;
                break;
            }
        }
    }

    public void Hide()
    {
        highlightImage.enabled = false;
    }
}

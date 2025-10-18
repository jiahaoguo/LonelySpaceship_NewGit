using UnityEngine;
using UnityEngine.UI;

public class InventoryUISelection : MonoBehaviour
{
    public static InventoryUISelection Instance;

    public Image highlightImage;
    private RectTransform rect;
    private int currentIndex = 0;

    private void Awake()
    {
        Instance = this;
        rect = highlightImage ? highlightImage.rectTransform : null;
        if (highlightImage) highlightImage.enabled = false;
    }

    public void OnSlotSelected(int index)
    {
        if (!highlightImage) return;

        currentIndex = index;

        // find the SlotUI with that index inside active hotbar
        InventoryUIController controller = FindObjectOfType<InventoryUIController>();
        if (controller && controller.hotbarSlots != null &&
            index >= 0 && index < controller.hotbarSlots.Length)
        {
            SlotUI slot = controller.hotbarSlots[index];
            if (slot && slot.gameObject.activeInHierarchy)
            {
                highlightImage.enabled = true;
                rect.position = slot.GetComponent<RectTransform>().position;
                return;
            }
        }

        highlightImage.enabled = false;
    }

    public void Hide()
    {
        if (highlightImage) highlightImage.enabled = false;
    }

    public int GetCurrentIndex() => currentIndex;
}

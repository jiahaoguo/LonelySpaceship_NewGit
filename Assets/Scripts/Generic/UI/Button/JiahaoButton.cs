using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[AddComponentMenu("UI/Jiahao Button")]
public class JiahaoButton : Button, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Extra Events")]
    public UnityEvent OnSelectEvent = new UnityEvent();
    public UnityEvent OnDeselectEvent = new UnityEvent();

    [Header("Hover Events")]
    public UnityEvent OnHoverEnterEvent = new UnityEvent();
    public UnityEvent OnHoverExitEvent = new UnityEvent();

    private int _originalSiblingIndex = -1;

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        OnSelectEvent.Invoke();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        OnDeselectEvent.Invoke();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        OnHoverEnterEvent.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        OnHoverExitEvent.Invoke();
    }

    // These helper methods make the editor setup work correctly
    public void BringToFront()
    {
        _originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
    }

    public void RestoreSiblingOrder()
    {
        if (_originalSiblingIndex >= 0 && _originalSiblingIndex < transform.parent.childCount)
            transform.SetSiblingIndex(_originalSiblingIndex);
    }
}

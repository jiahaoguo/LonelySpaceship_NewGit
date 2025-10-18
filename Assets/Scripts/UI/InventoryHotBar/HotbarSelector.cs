using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class HotbarSelector : MonoBehaviour
{
    [Header("Input Settings")]
    public InputActionAsset inputActions;
    public string actionMapName = "UI_Navigate_Hotbar";
    public string selectSlotActionName = "SelectSlot";

    [Header("Hotbar Settings")]
    public Transform hotbarParent;

    private Selectable[] slotButtons;
    private InputAction selectSlotAction;

    public int lastSelectedIndex;

    private void Awake()
    {
        if (inputActions != null)
            inputActions = Instantiate(inputActions);

        var map = inputActions?.FindActionMap(actionMapName, true);
        map?.Enable();

        selectSlotAction = map?.FindAction(selectSlotActionName, true);
    }

    private void Start()
    {
        if (hotbarParent != null)
            slotButtons = hotbarParent.GetComponentsInChildren<Selectable>(false);
        else
            Debug.LogWarning("HotbarSelector: No hotbar parent assigned.");
    }

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (selectSlotAction != null)
        {
            selectSlotAction.performed += OnSelectSlot;
            selectSlotAction.Enable();
        }
    }


    private void OnDisable()
    {

        if (selectSlotAction != null)
        {
            selectSlotAction.performed -= OnSelectSlot;
            selectSlotAction.Disable();
        }
    }

    private void Update()
    {
        Debug.Log("current selected object: " + EventSystem.current.currentSelectedGameObject);
        int index = GetPressedNumberKey();
        if (index >= 0 && slotButtons != null && index < slotButtons.Length)
        SelectSlot(index);
    }

    private int GetPressedNumberKey()
    {

        var kb = Keyboard.current;
        if (kb == null) return -1;

        // top row digits 1–9
        for (int i = 0; i < 9; i++)
        {
            var key = kb[(Key)((int)Key.Digit1 + i)];
            if (key != null && key.wasPressedThisFrame)
                return i;
        }

        // numpad digits 1–9
        for (int i = 0; i < 9; i++)
        {
            var key = kb[(Key)((int)Key.Numpad1 + i)];
            if (key != null && key.wasPressedThisFrame)
                return i;
        }

        return -1;
    }

    private void SelectSlot(int index)
    {
        var target = slotButtons[index];
        EventSystem.current.SetSelectedGameObject(target.gameObject);

        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(target.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
        ExecuteEvents.Execute(target.gameObject, pointer, ExecuteEvents.selectHandler);
    }

    private void OnSelectSlot(InputAction.CallbackContext ctx)
    {
        // still keep your original InputAction support
        int index = GetBindingIndex(ctx);
        if (index >= 0 && slotButtons != null && index < slotButtons.Length)
            SelectSlot(index);
    }

    private int GetBindingIndex(InputAction.CallbackContext ctx)
    {
        var control = ctx.control;
        if (control == null) return -1;

        for (int i = 0; i < ctx.action.bindings.Count; i++)
        {
            if (InputControlPath.Matches(ctx.action.bindings[i].effectivePath, control))
                return i;
        }
        return -1;
    }
}

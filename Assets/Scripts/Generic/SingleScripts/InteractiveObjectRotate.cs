using UnityEngine;

public class InteractiveObjectRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;         // sensitivity of rotation
    public bool allowHorizontal = true;      // allow left-right rotation
    public bool allowVertical = true;        // allow up-down rotation
    public bool reverseHorizontal = false;   // invert horizontal input
    public bool reverseVertical = false;     // invert vertical input

    [Header("Input Settings")]
    public bool useMouse = true;             // if false, will use touch input

    [Header("Inertia Settings")]
    public bool useInertia = true;           // keep spinning after release
    public float inertiaDamping = 5f;        // higher = slows faster

    private Vector2 lastInputPos;
    private Vector2 currentDelta;
    private Vector2 inertiaDelta;
    private bool isDragging = false;

    void Update()
    {
        if (useMouse)
        {
            HandleMouseInput();
        }
        else
        {
            HandleTouchInput();
        }

        // Apply inertia if enabled
        if (useInertia && !isDragging && inertiaDelta.sqrMagnitude > 0.01f)
        {
            RotateObject(inertiaDelta, true);
            inertiaDelta = Vector2.Lerp(inertiaDelta, Vector2.zero, inertiaDamping * Time.unscaledDeltaTime);
        }
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastInputPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            inertiaDelta = currentDelta; // save momentum
        }

        if (isDragging)
        {
            currentDelta = (Vector2)Input.mousePosition - lastInputPos;
            RotateObject(currentDelta, false);
            lastInputPos = Input.mousePosition;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
                lastInputPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
                inertiaDelta = currentDelta; // save momentum
            }

            if (isDragging && touch.phase == TouchPhase.Moved)
            {
                currentDelta = touch.position - lastInputPos;
                RotateObject(currentDelta, false);
                lastInputPos = touch.position;
            }
        }
    }

    private void RotateObject(Vector2 delta, bool isInertia)
    {
        float dt = Time.unscaledDeltaTime;
        float multiplier = isInertia ? rotationSpeed : rotationSpeed * dt;

        float rotX = allowVertical ? (reverseVertical ? delta.y : -delta.y) * multiplier : 0f;
        float rotY = allowHorizontal ? (reverseHorizontal ? -delta.x : delta.x) * multiplier : 0f;

        transform.Rotate(rotX, rotY, 0, Space.World);
    }
}

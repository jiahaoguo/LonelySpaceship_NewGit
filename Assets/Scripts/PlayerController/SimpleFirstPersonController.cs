using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float lookSpeed = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Jump Forgiveness")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    private CharacterController controller;
    private Transform cam;
    private float pitch = 0f;

    private float verticalVelocity = 0f;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;

    private bool inputEnabled = true;

    private Vector2 moveInput;
    private Vector2 lookInput;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;

        GameStateManager.OnGameStateChanged += HandleGameStateChanged;
    }

    void OnDestroy()
    {
        GameStateManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        inputEnabled = (newState == GameState.Gameplay);
    }

    void Update()
    {
        if (!inputEnabled)
            return;

        // --- Movement ---
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= moveSpeed;

        // --- Ground / Coyote Timer ---
        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
            verticalVelocity = -1f;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            verticalVelocity += gravity * Time.deltaTime;
        }

        // --- Jump Check ---
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0;
        }

        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        // --- Mouse Look ---
        float mouseX = lookInput.x * lookSpeed;
        float mouseY = lookInput.y * lookSpeed;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        cam.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    // -----------------------------
    // 🔹 Input System Callbacks
    // -----------------------------
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }
}

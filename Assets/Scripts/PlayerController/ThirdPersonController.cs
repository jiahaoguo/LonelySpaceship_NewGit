using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("References")]
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isSprinting;
    private bool isGrounded;
    private float groundCheckRadius = 0.3f;
    private Transform groundCheck;
    private LayerMask groundMask;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Optional: create a small empty for ground check
        groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.SetParent(transform);
        groundCheck.localPosition = Vector3.zero;
        groundMask = LayerMask.GetMask("Default");
    }

    private void Update()
    {
        HandleGravity();
        ApplyMovement();
    }

    // ——— Input-Triggered Functions ———

    /// <summary>Called by Input System when Move input changes.</summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>Called when Jump is pressed.</summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            TryJump();
    }

    /// <summary>Called when Sprint is pressed/released.</summary>
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed) isSprinting = true;
        if (context.canceled) isSprinting = false;
    }

    // ——— Movement Core ———

    private void ApplyMovement()
    {
        Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Rotate toward camera direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move in camera-relative direction
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * CurrentSpeed() * Time.deltaTime);
        }
    }

    private float rotationVelocity;
    private float CurrentSpeed()
    {
        return isSprinting ? sprintSpeed : moveSpeed;
    }

    // ——— Jumping and Gravity ———

    private void HandleGravity()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // small downward force to stick to ground

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void TryJump()
    {
        if (isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }

    // ——— Utility ———

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
#endif
}

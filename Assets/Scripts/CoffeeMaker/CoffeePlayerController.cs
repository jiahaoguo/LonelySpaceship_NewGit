using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
public class CoffeePlayerController : MonoBehaviour
{
    [Header("Detection Settings")]
    public float checkDistance = 0.2f;          // distance below feet to check
    public LayerMask groundMask;                // layers to consider "ground"

    [Header("Events")]
    public UnityEvent onStepOnHarmfulDrop;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        CheckStepSurface();
    }

    private void CheckStepSurface()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f; // start a little above feet
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, controller.height * 0.5f + checkDistance, groundMask))
        {
            CoffeeDrop drop = hit.collider.GetComponentInParent<CoffeeDrop>();
            if (drop != null && IsHarmful(drop))
            {
                onStepOnHarmfulDrop?.Invoke();
            }
        }
    }

    private bool IsHarmful(CoffeeDrop drop)
    {
        // CoffeeDrop state check
        var harmfulState = CoffeeDropState.Harmful;
        return drop != null && drop.GetCurrentState() == harmfulState;
    }
}

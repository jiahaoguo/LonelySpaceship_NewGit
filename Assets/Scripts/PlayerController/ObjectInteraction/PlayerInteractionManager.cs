using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionManager : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRadius = 3f;        // how far player can reach
    public LayerMask interactableLayer;      // only objects on this layer count
    public Camera playerCamera;              // main camera for raycast
    public float rayDistance = 5f;           // max ray distance for pointing

    private IInteractable currentTarget;

    void Update()
    {
        FindInteractable();
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && currentTarget != null)
        {
            currentTarget.Interact();
        }
    }

    private void FindInteractable()
    {
        currentTarget = null;

        if (playerCamera == null) return;

        // Raycast from center of the screen
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayer))
        {
            // Look on the collider, or up the hierarchy
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
                interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                // Also require distance within interactRadius
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist <= interactRadius)
                {
                    currentTarget = interactable;
                }
            }
        }
    }

    public string GetCurrentPrompt()
    {
        return currentTarget != null ? currentTarget.GetPromptText() : string.Empty;
    }

    public IInteractable GetCurrentTarget()
    {
        return currentTarget;
    }
}

using UnityEngine;
using TMPro;               // if using TextMeshPro
using UnityEngine.UI;     // fallback to legacy Text if needed

public class InteractableUIController : MonoBehaviour
{
    [Header("References")]
    public PlayerInteractionManager player;
    public Transform interactableUIParent;
    public GameObject interactableUIPrefab;

    private GameObject activeUI;
    private IInteractable lastTarget;

    void Update()
    {
        var target = player != null ? player.GetCurrentTarget() : null;

        // If target changed, rebuild UI accordingly
        if (!ReferenceEquals(target, lastTarget))
        {
            // Destroy old UI when looking away or switching targets
            if (activeUI != null)
            {
                Destroy(activeUI);
                activeUI = null;
            }

            lastTarget = target;

            // If we have a new target, create the UI
            if (lastTarget != null)
            {
                activeUI = Instantiate(interactableUIPrefab, interactableUIParent);

                // Fill text
                var tmp = activeUI.GetComponentInChildren<TextMeshProUGUI>(true);
                if (tmp != null)
                {
                    tmp.text = lastTarget.GetPromptText();
                }
                else
                {
                    var legacy = activeUI.GetComponentInChildren<Text>(true);
                    if (legacy != null) legacy.text = lastTarget.GetPromptText();
                }
            }
        }

        // Optional: keep the text fresh if prompt text can change frame-to-frame
        if (activeUI != null && lastTarget != null)
        {
            var tmp = activeUI.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) tmp.text = lastTarget.GetPromptText();
            else
            {
                var legacy = activeUI.GetComponentInChildren<Text>(true);
                if (legacy != null) legacy.text = lastTarget.GetPromptText();
            }
        }
    }
}

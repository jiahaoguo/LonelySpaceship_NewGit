public interface IInteractable
{
    void Interact();           // Called when player presses F
    string GetPromptText();    // What to show in UI (e.g., "Open Door")
}

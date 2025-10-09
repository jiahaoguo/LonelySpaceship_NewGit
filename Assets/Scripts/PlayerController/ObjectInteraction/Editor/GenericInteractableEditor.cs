using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

[CustomEditor(typeof(GenericInteractable))]
public class GenericInteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector first
        DrawDefaultInspector();

        // Add spacing
        EditorGUILayout.Space();

        // Tooltip + button
        EditorGUILayout.HelpBox(
            "Bind UI: Ensures this interactable will open a UI.\n\n" +
            "- Adds a GameStateChanger (if missing).\n" +
            "- Sets its target state to UI.\n" +
            "- Binds the onInteract event to trigger ChangeState().",
            MessageType.Info
        );

        if (GUILayout.Button("Bind UI"))
        {
            BindUI();
        }
    }

    private void BindUI()
    {
        GenericInteractable interactable = (GenericInteractable)target;
        GameObject go = interactable.gameObject;

        // --- Ensure GameStateChanger exists ---
        GameStateChanger changer = go.GetComponent<GameStateChanger>();
        if (changer == null)
        {
            changer = go.AddComponent<GameStateChanger>();
            Undo.RegisterCreatedObjectUndo(changer, "Add GameStateChanger");
        }

        // Set default state to UI
        changer.targetState = GameState.UI;

        // --- Ensure onInteract has ChangeState bound ---
        UnityEvent onInteract = interactable.onInteract;

        bool alreadyBound = false;
        for (int i = 0; i < onInteract.GetPersistentEventCount(); i++)
        {
            Object targetObj = onInteract.GetPersistentTarget(i);
            string methodName = onInteract.GetPersistentMethodName(i);

            if (targetObj == changer && methodName == nameof(GameStateChanger.ChangeState))
            {
                alreadyBound = true;
                break;
            }
        }

        if (!alreadyBound)
        {
            UnityAction action = changer.ChangeState;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(onInteract, action);
            Debug.Log("Bound onInteract to GameStateChanger.ChangeState().");
        }
        else
        {
            Debug.Log("onInteract already bound to GameStateChanger.ChangeState().");
        }

        // Mark dirty so changes save
        EditorUtility.SetDirty(interactable);
        EditorUtility.SetDirty(changer);
    }
}

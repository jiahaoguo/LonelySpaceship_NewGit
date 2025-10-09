using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


[CustomEditor(typeof(GameStateChanger))]
[CanEditMultipleObjects]
public class GameStateChangerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameStateChanger changer = (GameStateChanger)target;

        if (changer.targetState == GameState.Gameplay)
        {
            GUILayout.Space(10);

            GUIContent buttonContent = new GUIContent(
                "Pair With Button",
                "Finds a Button component on this GameObject. " +
                "If the Button does not already trigger ChangeState(), " +
                "this will automatically add a new OnClick event that calls it."
            );

            if (GUILayout.Button(buttonContent))
            {
                Button btn = changer.GetComponent<Button>();
                if (btn == null)
                {
                    Debug.LogWarning("No Button component found on this GameObject.");
                    return;
                }

                bool alreadyHasChangeState = false;

                for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
                {
                    if (btn.onClick.GetPersistentTarget(i) == changer &&
                        btn.onClick.GetPersistentMethodName(i) == "ChangeState")
                    {
                        alreadyHasChangeState = true;
                        break;
                    }
                }

                if (!alreadyHasChangeState)
                {
                    UnityEventTools.AddPersistentListener(btn.onClick, changer.ChangeState);
                    Debug.Log("✅ Added persistent ChangeState() call to Button's OnClick.");
                }
                else
                {
                    Debug.Log("ℹ️ Button already has ChangeState() assigned.");
                }

                EditorUtility.SetDirty(btn);
            }
        }
    }
}

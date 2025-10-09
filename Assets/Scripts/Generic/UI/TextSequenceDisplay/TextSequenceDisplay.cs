using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Localization; // for LocalizedString

[System.Serializable]
public class TextEntry
{
    public LocalizedString localizedText; // now uses localization
    public float duration = 2f;           // how long to stay before next
}

[System.Serializable]
public class TweenSettings
{
    public bool enabled = false;
    public float duration = 0.5f;

    [Header("Animation Curves")]
    public AnimationCurve alphaCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve positionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Position Offset")]
    public Vector3 startOffset = Vector3.zero;
}

[System.Serializable]
public class TextGroup
{
    public string groupName;
    public List<TextEntry> entries = new List<TextEntry>();

    [Header("Uniform Tween Settings")]
    public bool useUniformTween = false;
    public TweenSettings loadIn = new TweenSettings();
    public TweenSettings loadOut = new TweenSettings();

    [Header("Events")]
    public UnityEvent onGroupStart;
    public UnityEvent onGroupFinish;
}

public class TextSequenceDisplay : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI targetText;

    [Header("Groups")]
    public List<TextGroup> groups = new List<TextGroup>();

    private Coroutine displayRoutine;
    private CanvasGroup canvasGroup;
    private Vector3 basePosition;

    private void Awake()
    {
        if (targetText != null)
        {
            canvasGroup = targetText.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = targetText.gameObject.AddComponent<CanvasGroup>();

            basePosition = targetText.rectTransform.anchoredPosition;
        }
    }

    public void PlayGroup(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= groups.Count)
        {
            Debug.LogWarning($"Invalid group index: {groupIndex}");
            return;
        }

        if (displayRoutine != null)
            StopCoroutine(displayRoutine);

        displayRoutine = StartCoroutine(DisplayGroupRoutine(groups[groupIndex]));
    }

    public void PlayGroup(string groupName)
    {
        int index = groups.FindIndex(g => g.groupName == groupName);
        if (index >= 0)
            PlayGroup(index);
        else
            Debug.LogWarning($"No group found with name: {groupName}");
    }

    private IEnumerator DisplayGroupRoutine(TextGroup group)
    {
        if (targetText == null)
        {
            Debug.LogWarning("TextSequenceDisplay has no TextMeshProUGUI assigned!");
            yield break;
        }

        group.onGroupStart?.Invoke();

        for (int i = 0; i < group.entries.Count; i++)
        {
            var entry = group.entries[i];

            // Resolve localized string (sync)
            targetText.text = entry.localizedText.GetLocalizedString();

            // Load In animation
            if (group.useUniformTween && group.loadIn.enabled)
                yield return AnimateText(group.loadIn, true);

            // Stay for duration
            yield return new WaitForSeconds(entry.duration);

            // Load Out animation
            if (i < group.entries.Count - 1)
            {
                if (group.useUniformTween && group.loadOut.enabled)
                    yield return AnimateText(group.loadOut, false);
            }
            else
            {
                if (group.useUniformTween && group.loadOut.enabled)
                    yield return AnimateText(group.loadOut, false);

                // Clear text after last entry
                targetText.text = string.Empty;
            }
        }

        group.onGroupFinish?.Invoke();
        displayRoutine = null;
    }

    private IEnumerator AnimateText(TweenSettings settings, bool isIn)
    {
        RectTransform rt = targetText.rectTransform;

        float t = 0f;
        Vector3 startPos = basePosition + (isIn ? settings.startOffset : Vector3.zero);
        Vector3 endPos = basePosition + (isIn ? Vector3.zero : settings.startOffset);

        Vector3 startScale = isIn ? Vector3.zero : Vector3.one;
        Vector3 endScale = isIn ? Vector3.one : Vector3.zero;

        float startAlpha = isIn ? 0f : 1f;
        float endAlpha = isIn ? 1f : 0f;

        while (t < settings.duration)
        {
            float normalized = t / settings.duration;

            // Alpha
            float a = Mathf.Lerp(startAlpha, endAlpha, settings.alphaCurve.Evaluate(normalized));
            canvasGroup.alpha = a;

            // Scale
            float s = Mathf.Lerp(startScale.x, endScale.x, settings.scaleCurve.Evaluate(normalized));
            rt.localScale = new Vector3(s, s, s);

            // Position
            float p = settings.positionCurve.Evaluate(normalized);
            rt.anchoredPosition = Vector3.Lerp(startPos, endPos, p);

            t += Time.deltaTime;
            yield return null;
        }

        // Ensure final state
        canvasGroup.alpha = endAlpha;
        rt.localScale = endScale;
        rt.anchoredPosition = endPos;
    }
}

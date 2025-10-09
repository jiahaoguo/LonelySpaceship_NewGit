using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[ExecuteAlways]
public class LoadAnimationPro : MonoBehaviour
{
    public enum AnimationType { Position, Rotation, Scale, Alpha, UnityAnimation }

    [System.Serializable]
    public class AnimationOption
    {
        public AnimationType type;

        public float duration = 0.5f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Position offset
        public Vector3 position;

        // Rotation offset
        public Vector3 rotation;

        // Scale offset
        public Vector3 scale;

        // Alpha absolute
        [Range(0, 1)] public float alpha = 0f;

        // Unity Animation
        public AnimationClip clip;
        public bool useOverrideDuration = false;
        public float overrideDuration = 1f;
    }

    [Header("Settings")]
    public bool cascadeChildren = true;

    [Header("Load In Options (FROM offsets)")]
    public List<AnimationOption> loadInAnimations = new List<AnimationOption>();

    [Header("Load Out Options (TO offsets)")]
    public List<AnimationOption> loadOutAnimations = new List<AnimationOption>();

    // Events (hidden, shown by custom inspector)
    [SerializeField, HideInInspector] private UnityEvent onLoadInStart;
    [SerializeField, HideInInspector] private UnityEvent onLoadInFinish;
    [SerializeField, HideInInspector] private UnityEvent onLoadOutStart;
    [SerializeField, HideInInspector] private UnityEvent onLoadOutFinish;

    private Coroutine animRoutine;
    private Animator animator;
    private float originalAnimatorSpeed = 1f;

    // ===== Cached Baseline Values =====
    private Vector3 basePosition;
    private Vector3 baseRotation;
    private Vector3 baseScale;
    private float baseAlpha;
    private bool baseValuesCaptured = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator != null) originalAnimatorSpeed = animator.speed;
    }

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            if (animRoutine != null) StopCoroutine(animRoutine);

            CaptureBaseValues();

            onLoadInStart?.Invoke();
            animRoutine = StartCoroutine(PlayGroup(loadInAnimations, true, () =>
            {
                onLoadInFinish?.Invoke();
            }));
        }
    }

    public void LoadOut()
    {
        CaptureBaseValues();
        onLoadOutStart?.Invoke();

        if (cascadeChildren)
        {
            var all = GetComponentsInChildren<LoadAnimationPro>(true);
            foreach (var anim in all)
            {
                bool isRoot = anim == this;
                anim.PlayLoadOut(isRoot);
            }
        }
        else
        {
            PlayLoadOut(true);
        }
    }

    private void PlayLoadOut(bool isRoot)
    {
        if (animRoutine != null) StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(PlayGroup(loadOutAnimations, false, () =>
        {
            onLoadOutFinish?.Invoke();
            if (isRoot)
                gameObject.SetActive(false);
        }));
    }

    private void CaptureBaseValues()
    {
        if (baseValuesCaptured) return;
        baseValuesCaptured = true;

        basePosition = transform.localPosition;
        baseRotation = transform.localEulerAngles;
        baseScale = transform.localScale;
        baseAlpha = GetAlpha();
    }

    private IEnumerator PlayGroup(List<AnimationOption> options, bool isLoadIn, System.Action onComplete)
    {
        if (options.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        float maxDuration = 0f;
        foreach (var opt in options)
        {
            maxDuration = Mathf.Max(maxDuration, GetOptionDuration(opt));
        }

        float time = 0f;

        // Unity Animations (play immediately)
        foreach (var opt in options)
        {
            if (opt.type == AnimationType.UnityAnimation && animator != null && opt.clip != null)
            {
                float effectiveDuration = GetOptionDuration(opt);
                float clipLen = GetClipLength(opt.clip);
                animator.speed = clipLen > 0 ? (clipLen / effectiveDuration) : 1f;
                animator.Play(opt.clip.name, 0, 0f);
            }
        }

        while (time < maxDuration)
        {
            // ✅ use unscaledDeltaTime so pause does not freeze animation
            time += Time.unscaledDeltaTime;

            foreach (var opt in options)
            {
                if (opt.type == AnimationType.UnityAnimation) continue;

                float eval = opt.curve.Evaluate(Mathf.Clamp01(time / GetOptionDuration(opt)));

                switch (opt.type)
                {
                    case AnimationType.Scale:
                        transform.localScale = isLoadIn
                            ? Vector3.LerpUnclamped(baseScale + opt.scale, baseScale, eval)
                            : Vector3.LerpUnclamped(baseScale, baseScale + opt.scale, eval);
                        break;

                    case AnimationType.Position:
                        transform.localPosition = isLoadIn
                            ? Vector3.LerpUnclamped(basePosition + opt.position, basePosition, eval)
                            : Vector3.LerpUnclamped(basePosition, basePosition + opt.position, eval);
                        break;

                    case AnimationType.Rotation:
                        transform.localRotation = isLoadIn
                            ? Quaternion.LerpUnclamped(Quaternion.Euler(baseRotation + opt.rotation), Quaternion.Euler(baseRotation), eval)
                            : Quaternion.LerpUnclamped(Quaternion.Euler(baseRotation), Quaternion.Euler(baseRotation + opt.rotation), eval);
                        break;

                    case AnimationType.Alpha:
                        SetAlpha(isLoadIn
                            ? Mathf.LerpUnclamped(opt.alpha, baseAlpha, eval)
                            : Mathf.LerpUnclamped(baseAlpha, opt.alpha, eval));
                        break;
                }
            }

            yield return null;
        }


        if (animator != null) animator.speed = originalAnimatorSpeed;

        onComplete?.Invoke();
    }

    private float GetOptionDuration(AnimationOption opt)
    {
        if (opt.type == AnimationType.UnityAnimation && opt.clip != null)
        {
            float baseLen = GetClipLength(opt.clip);
            float dur = opt.useOverrideDuration ? Mathf.Max(0.01f, opt.overrideDuration) : baseLen;
            if (dur <= 0f) dur = 1f;
            return dur;
        }
        return Mathf.Max(opt.duration, 0.01f);
    }

    private float GetClipLength(AnimationClip clip)
    {
        if (clip == null) return 0f;
        float len = clip.length;
        if (len <= 0f) len = 1f; // fallback
        return len;
    }

    // ===== Alpha Handling =====
    private float GetAlpha()
    {
        if (TryGetComponent(out CanvasGroup cg)) return cg.alpha;
        if (TryGetComponent(out Graphic g)) return g.color.a;
        if (TryGetComponent(out SpriteRenderer sr)) return sr.color.a;
        if (TryGetComponent(out Renderer r) && r.material.HasProperty("_Color")) return r.material.color.a;
        return 1f;
    }

    private void SetAlpha(float a)
    {
        if (TryGetComponent(out CanvasGroup cg)) cg.alpha = a;
        else if (TryGetComponent(out Graphic g)) g.color = new Color(g.color.r, g.color.g, g.color.b, a);
        else if (TryGetComponent(out SpriteRenderer sr)) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a);
        else if (TryGetComponent(out Renderer r) && r.material.HasProperty("_Color"))
            r.material.color = new Color(r.material.color.r, r.material.color.g, r.material.color.b, a);
    }

    // Public accessors for events
    public UnityEvent OnLoadInStart => onLoadInStart ??= new UnityEvent();
    public UnityEvent OnLoadInFinish => onLoadInFinish ??= new UnityEvent();
    public UnityEvent OnLoadOutStart => onLoadOutStart ??= new UnityEvent();
    public UnityEvent OnLoadOutFinish => onLoadOutFinish ??= new UnityEvent();
}

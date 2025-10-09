//--------------------------------------------------------------------------//
// Copyright 2023-2025 Chocolate Dinosaur Ltd. All rights reserved.
// For full documentation visit https://www.chocolatedinosaur.com
//--------------------------------------------------------------------------//

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ChocDino.UIFX;

namespace ChocDino.UIFX.Demos
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(DropShadowFilter))]
    public class Demo01DropShadowHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler
    {
        [Header("Time Settings")]
        [Tooltip("If true, uses unscaled time (ignores Time.timeScale).")]
        [SerializeField] private bool useUnscaledTime = false;

        [Header("Transform Settings")]
        [Tooltip("If true, scale using localScale instead of rect sizeDelta.")]
        [SerializeField] private bool useScaleInsteadOfRect = false;

        [Header("Hover Animation Settings")]
        [Tooltip("Scale multiplier applied on hover/selection. (1.0 = no change)")]
        [SerializeField] private float hoverScaleMultiplier = 1.1f;

        [Tooltip("Color brightness multiplier when NOT hovered.")]
        [SerializeField] private float idleColorBrightness = 0.92f;

        [Tooltip("Alpha when NOT hovered (0 = fully transparent, 1 = fully opaque).")]
        [Range(0f, 1f)]
        [SerializeField] private float idleAlpha = 0.5f;

        [Tooltip("Shadow strength when hovered/selected (0–1).")]
        [Range(0f, 1f)]
        [SerializeField] private float hoverShadowStrength = 1f;

        [Tooltip("Shadow strength when not hovered/selected (0–1).")]
        [Range(0f, 1f)]
        [SerializeField] private float idleShadowStrength = 0f;

        [Tooltip("Damping speed when hovering/selected.")]
        [SerializeField] private float dampSpeedOver = 8f;

        [Tooltip("Damping speed when returning to normal.")]
        [SerializeField] private float dampSpeedFall = 6f;

        [Header("Linked Shadows")]
        [Tooltip("Other DropShadowFilter components to tween together with this one.")]
        [SerializeField] private DropShadowFilter[] additionalShadows;

        private DropShadowFilter _shadow;
        private Graphic _graphic;
        private RectTransform _xform;
        private bool _isActive = false; // covers both hover + selection
        private Color _startColor = Color.white;
        private Vector2 _startSize;
        private Vector3 _startScale;

        void Awake()
        {
            _shadow = GetComponent<DropShadowFilter>();
            _graphic = GetComponent<Graphic>();
            _xform = GetComponent<RectTransform>();
            _startColor = _graphic.color;
            _startSize = _xform.sizeDelta;
            _startScale = _xform.localScale;

            // Apply initial idle alpha
            Color initialColor = _startColor;
            initialColor.a = idleAlpha;
            _graphic.color = initialColor;

            UpdateAnimation(true);
        }

        void Update()
        {
            UpdateAnimation(false);
        }

        void UpdateAnimation(bool force)
        {
            float targetShadow = _isActive ? hoverShadowStrength : idleShadowStrength;
            float dampSpeed = _isActive ? dampSpeedOver : dampSpeedFall;

            // Color logic
            Color targetColor = _isActive
                ? new Color(_startColor.r, _startColor.g, _startColor.b, 1f)
                : new Color(
                    _startColor.r * idleColorBrightness,
                    _startColor.g * idleColorBrightness,
                    _startColor.b * idleColorBrightness,
                    idleAlpha
                );

            // Size/scale logic
            Vector2 targetSize = _isActive ? _startSize * hoverScaleMultiplier : _startSize;
            Vector3 targetScale = _isActive ? _startScale * hoverScaleMultiplier : _startScale;

            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (force)
                ApplyInstant(targetShadow, targetColor, targetSize, targetScale);
            else if (Mathf.Abs(_shadow.Strength - targetShadow) > 0.001f)
                ApplyDamped(targetShadow, targetColor, targetSize, targetScale, dampSpeed, delta);
        }

        private void ApplyInstant(float targetShadow, Color targetColor, Vector2 targetSize, Vector3 targetScale)
        {
            _shadow.Strength = targetShadow;
            _graphic.color = targetColor;

            if (useScaleInsteadOfRect)
                _xform.localScale = targetScale;
            else
                _xform.sizeDelta = targetSize;

            if (additionalShadows != null)
            {
                foreach (var s in additionalShadows)
                {
                    if (s == null) continue;
                    s.Strength = targetShadow;
                }
            }
        }

        private void ApplyDamped(float targetShadow, Color targetColor, Vector2 targetSize, Vector3 targetScale, float dampSpeed, float delta)
        {
            _shadow.Strength = MathUtils.DampTowards(_shadow.Strength, targetShadow, dampSpeed, delta);
            _graphic.color = MathUtils.DampTowards(_graphic.color, targetColor, dampSpeed, delta);

            if (useScaleInsteadOfRect)
                _xform.localScale = MathUtils.DampTowards(_xform.localScale, targetScale, dampSpeed, delta);
            else
                _xform.sizeDelta = MathUtils.DampTowards(_xform.sizeDelta, targetSize, dampSpeed, delta);

            if (additionalShadows != null)
            {
                foreach (var s in additionalShadows)
                {
                    if (s == null) continue;
                    s.Strength = MathUtils.DampTowards(s.Strength, targetShadow, dampSpeed, delta);
                }
            }
        }

        // ----- UI Event Handling -----

        public void OnPointerEnter(PointerEventData eventData) => _isActive = true;
        public void OnPointerExit(PointerEventData eventData) => _isActive = false;

        public void OnSelect(BaseEventData eventData) => _isActive = true;
        public void OnDeselect(BaseEventData eventData) => _isActive = false;
    }
}

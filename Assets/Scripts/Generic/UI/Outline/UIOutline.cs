using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UIOutline : MonoBehaviour
{
    [Header("Outline Settings")]
    [Min(0f)]
    public float outlineThickness = 0f;

    [Range(0.01f, 0.8f)]
    public float imageCorner = 0.1f; // Only used if parent is Sliced

    [Header("Colors")]
    public Color outlineColor = Color.white;
    public Color innerColor = Color.white;

    [Header("Lock Settings")]
    public bool lockInnerImage = true;

    [Header("Manual Tuning")]
    public Vector2 manualInnerOffset = Vector2.zero;
    public float manualInnerPPUMultiplier = 0f;

    [HideInInspector]
    public Image innerImage;

    private Image parentImage;
    private RectTransform parentRect;

    [SerializeField, HideInInspector]
    private Vector2 originalSize;

    private void Awake()
    {
        CacheRefs();
        SaveOriginalSize();
    }

    private void OnEnable()
    {
        InitializeUIOutline();
        ApplyOutline();
    }

    private void OnValidate()
    {
        CacheRefs();
        if (originalSize == Vector2.zero && parentRect != null)
            SaveOriginalSize();

        ApplyOutline();
        ApplyLockState();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying && parentRect != null)
        {
            Vector2 currentSize = parentRect.sizeDelta;

            // Recalculate original size if manually resized
            Vector2 expectedSize = originalSize + Vector2.one * outlineThickness * 2f;
            if (!ApproximatelyEqual(currentSize, expectedSize))
            {
                originalSize = new Vector2(
                    Mathf.Max(0, currentSize.x - outlineThickness * 2f),
                    Mathf.Max(0, currentSize.y - outlineThickness * 2f)
                );

                ApplyOutline();
                ApplyLockState();
            }
        }
    }

    private bool ApproximatelyEqual(Vector2 a, Vector2 b, float tolerance = 0.01f)
    {
        return Mathf.Abs(a.x - b.x) < tolerance && Mathf.Abs(a.y - b.y) < tolerance;
    }
#endif

    private void CacheRefs()
    {
        if (parentImage == null) parentImage = GetComponent<Image>();
        if (parentRect == null) parentRect = GetComponent<RectTransform>();
    }

    private void SaveOriginalSize()
    {
        if (parentRect != null)
        {
            originalSize = new Vector2(
                Mathf.Max(0, parentRect.rect.width - outlineThickness * 2f),
                Mathf.Max(0, parentRect.rect.height - outlineThickness * 2f)
            );
        }
    }

    private void InitializeUIOutline()
    {
        CacheRefs();
        SaveOriginalSize();

        Transform existing = transform.Find("InnerImage");
        if (existing != null)
        {
            innerImage = existing.GetComponent<Image>();
        }
        else
        {
            GameObject go = new GameObject("InnerImage", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            innerImage = go.GetComponent<Image>();
        }

        CopyImageProperties(parentImage, innerImage);

        RectTransform innerRect = innerImage.rectTransform;
        innerRect.anchorMin = new Vector2(0.5f, 0.5f);
        innerRect.anchorMax = new Vector2(0.5f, 0.5f);
        innerRect.pivot = new Vector2(0.5f, 0.5f);
        innerRect.anchoredPosition = Vector2.zero;

        innerImage.transform.SetAsFirstSibling();

        ApplyLockState();
    }

    private void ApplyOutline()
    {
        if (parentRect == null || innerImage == null) return;

        // Outer size = inner size + outline
        parentRect.sizeDelta = originalSize + Vector2.one * outlineThickness * 2f;

        // Inner rect stays original size
        RectTransform innerRect = innerImage.rectTransform;
        innerRect.sizeDelta = originalSize;
        innerRect.anchoredPosition = manualInnerOffset;

        // Handle sliced mode
        if (parentImage.type == Image.Type.Sliced)
        {
            parentImage.pixelsPerUnitMultiplier = imageCorner;
            float B = EstimateB(imageCorner);
            innerImage.pixelsPerUnitMultiplier =
                manualInnerPPUMultiplier > 0 ? manualInnerPPUMultiplier : B;
        }

        parentImage.color = outlineColor;
        innerImage.color = innerColor;
        innerImage.transform.SetAsFirstSibling();
    }

    private void CopyImageProperties(Image source, Image target)
    {
        if (source == null || target == null) return;
        target.sprite = source.sprite;
        target.type = source.type;
        target.preserveAspect = source.preserveAspect;
        target.material = source.material;
    }

    private float EstimateB(float A)
    {
        float[] As = { 0.05f, 0.07f, 0.10f, 0.20f };
        float[] Bs = { 0.07f, 0.10f, 0.17f, 0.40f };

        if (A <= As[0])
            return Bs[0] + (Bs[1] - Bs[0]) * (A - As[0]) / (As[1] - As[0]);

        if (A >= As[As.Length - 1])
        {
            int n = As.Length;
            return Bs[n - 2] + (Bs[n - 1] - Bs[n - 2]) * (A - As[n - 2]) / (As[n - 1] - As[n - 2]);
        }

        for (int i = 0; i < As.Length - 1; i++)
        {
            if (A >= As[i] && A <= As[i + 1])
            {
                return Bs[i] + (Bs[i + 1] - Bs[i]) * (A - As[i]) / (As[i + 1] - As[i]);
            }
        }

        return A;
    }

    private void ApplyLockState()
    {
#if UNITY_EDITOR
        if (innerImage == null) return;

        if (lockInnerImage)
            innerImage.gameObject.hideFlags = HideFlags.NotEditable;
        else
            innerImage.gameObject.hideFlags = HideFlags.None;
#endif
    }

    private void OnDestroy()
    {
        if (parentRect != null && originalSize != Vector2.zero)
            parentRect.sizeDelta = originalSize;

        if (parentImage != null)
            parentImage.color = Color.white;

#if UNITY_EDITOR
        if (innerImage != null)
            innerImage.gameObject.hideFlags = HideFlags.None;
#endif

        if (innerImage != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(innerImage.gameObject);
            else
                Destroy(innerImage.gameObject);
#else
            Destroy(innerImage.gameObject);
#endif
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIOutline))]
public class UIOutlineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UIOutline outline = (UIOutline)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("outlineThickness"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("outlineColor"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("innerColor"));

        if (outline.GetComponent<Image>().type == Image.Type.Sliced)
        {
            EditorGUILayout.Slider(serializedObject.FindProperty("imageCorner"), 0.01f, 0.8f);
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("lockInnerImage"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Manual Tuning", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("manualInnerOffset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("manualInnerPPUMultiplier"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

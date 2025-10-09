using UnityEngine;
using UnityEngine.UI;

public class SliderMaterialBinder : MonoBehaviour
{
    [Header("UI and Material")]
    [Tooltip("The UI slider that provides the value.")]
    public Slider targetSlider;

    [Tooltip("The material whose property will be updated.")]
    public Material targetMaterial;

    [Header("Shader Property")]
    [Tooltip("The name of the material property to control (e.g. _FillAmount).")]
    public string propertyName = "_FillAmount";

    [Header("Value Mapping")]
    [Tooltip("Choose how the slider value is converted before applying to the material.")]
    public ValueMappingMode mappingMode = ValueMappingMode.Linear;

    [Tooltip("Exponent used for Exponential/Logarithmic mapping. Example: 2 = square, 3 = cubic.")]
    public float exponent = 2f;

    [Header("Options")]
    [Tooltip("If true, reverses the slider value (1 = 0, 0 = 1).")]
    public bool reverseValue = false;

    public enum ValueMappingMode
    {
        Linear,         // Pass value directly
        Exponential,    // Start slow, grow fast near max
        LogarithmicLike // Start fast, slow near max
    }

    void Start()
    {
        if (targetSlider == null || targetMaterial == null)
        {
            Debug.LogWarning("SliderMaterialBinder missing references!");
            return;
        }

        // Subscribe to slider changes
        targetSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // Initialize property from current slider value
        OnSliderValueChanged(targetSlider.value);
    }

    private void OnSliderValueChanged(float value)
    {
        float min = targetSlider.minValue;
        float max = targetSlider.maxValue;

        // Normalize slider value to [0,1]
        float t = Mathf.InverseLerp(min, max, value);

        // Reverse if enabled
        if (reverseValue)
            t = 1f - t;

        float finalValue = value;

        switch (mappingMode)
        {
            case ValueMappingMode.Linear:
                finalValue = Mathf.Lerp(min, max, t);
                break;

            case ValueMappingMode.Exponential:
                t = Mathf.Pow(t, exponent);
                finalValue = Mathf.Lerp(min, max, t);
                break;

            case ValueMappingMode.LogarithmicLike:
                if (exponent <= 0f) exponent = 2f; // safeguard
                t = Mathf.Pow(t, 1f / exponent);
                finalValue = Mathf.Lerp(min, max, t);
                break;
        }

        // Use MaterialUtils to update the material property
        MaterialUtils.SetMaterialFloat(targetMaterial, propertyName, finalValue);
    }

    private void OnDestroy()
    {
        if (targetSlider != null)
            targetSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}

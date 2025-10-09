using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SliderText : MonoBehaviour
{
    [Header("References")]
    public Slider targetSlider;

    [Header("Display Options")]
    public bool displayPercentage = false;
    public string format = "0.##"; // number format

    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();

        if (targetSlider == null)
        {
            Debug.LogWarning("SliderText: No slider assigned.");
            return;
        }

        // Subscribe to slider updates
        targetSlider.onValueChanged.AddListener(OnSliderValueChanged);

        // Initialize with current slider value
        OnSliderValueChanged(targetSlider.value);
    }

    private void OnSliderValueChanged(float value)
    {
        if (displayPercentage)
        {
            float percentage = (value / targetSlider.maxValue) * 100f;
            textComponent.text = percentage.ToString(format) + "%";
        }
        else
        {
            textComponent.text = value.ToString(format);
        }
    }

    void OnDestroy()
    {
        if (targetSlider != null)
            targetSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}

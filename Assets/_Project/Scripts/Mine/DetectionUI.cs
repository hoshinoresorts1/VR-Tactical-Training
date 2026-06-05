using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetectionUI : MonoBehaviour
{
    public Slider detectionSlider;
    public TMP_Text percentText;

    public void UpdateDetectionUI(float value)
    {
        if (detectionSlider != null)
            detectionSlider.value = value;

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}

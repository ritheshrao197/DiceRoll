using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bonus debug panel: buttons to force the next roll to a specific value.
/// In a shipping build this GameObject should be disabled or stripped.
/// Communicates by setting a public property on DiceRoller.
/// </summary>
public class DebugRollPanel : MonoBehaviour
{
    [SerializeField] private DiceRoller diceRoller;
    [SerializeField] private Toggle     forceToggle;
    [SerializeField] private Slider     valueSlider;
    [SerializeField] private TMPro.TextMeshProUGUI valueLabel;

    private void OnEnable()
    {
        forceToggle.onValueChanged.AddListener(OnToggleChanged);
        valueSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        forceToggle.onValueChanged.RemoveListener(OnToggleChanged);
        valueSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnToggleChanged(bool on)
    {
        SetForceRoll(on, (int)valueSlider.value);
    }

    private void OnSliderChanged(float value)
    {
        if (valueLabel) valueLabel.text = ((int)value).ToString();
        if (forceToggle.isOn)
            SetForceRoll(true, (int)value);
    }

    private void SetForceRoll(bool active, int value)
    {
        // Access via reflection-safe public setters exposed on DiceRoller
        diceRoller.SetDebugForce(active, value);
    }
}

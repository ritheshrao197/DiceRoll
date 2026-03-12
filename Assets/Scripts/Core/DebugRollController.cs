using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// BONUS / DEBUG ONLY: Adds buttons to force the next roll to 3 or 6.
/// Wire up in the Inspector. Hide this GameObject in release builds.
/// </summary>
public class DebugRollController : MonoBehaviour
{
    [SerializeField] private DiceRoller diceRoller;
    [SerializeField] private Button     forceThreeButton;
    [SerializeField] private Button     forceSixButton;
    [SerializeField] private Button     clearForceButton;
    [SerializeField] private TextMeshProUGUI statusLabel;

    private void Start()
    {
        forceThreeButton?.onClick.AddListener(() => SetForce(3));
        forceSixButton?.onClick.AddListener(()   => SetForce(6));
        clearForceButton?.onClick.AddListener(ClearForce);
        UpdateStatus(0, false);
    }

    private void SetForce(int value)
    {
        // Access via SerializedField on DiceRoller through a public method to keep DiceRoller clean
        diceRoller.SetDebugForce(true, value);
        UpdateStatus(value, true);
    }

    private void ClearForce()
    {
        diceRoller.SetDebugForce(false, 1);
        UpdateStatus(0, false);
    }

    private void UpdateStatus(int value, bool active)
    {
        if (statusLabel)
            statusLabel.text = active ? $"[DEBUG] Forcing roll → {value}" : "[DEBUG] Random roll";
    }
}

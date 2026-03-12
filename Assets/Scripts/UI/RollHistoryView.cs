using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Optional bonus feature: shows the last N dice results in a compact strip.
/// Subscribes to GameCalculator.OnHistoryUpdated.
/// </summary>
public class RollHistoryView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI historyLabel;
    [SerializeField] private string          prefix = "History: ";

    private void OnEnable()  => GameCalculator.Instance.OnHistoryUpdated += Refresh;
    private void OnDisable() => GameCalculator.Instance.OnHistoryUpdated -= Refresh;

    private void Refresh(IEnumerable<int> history)
    {
        if (!historyLabel) return;
        historyLabel.text = prefix + string.Join("  →  ", history);
    }
}

using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// BONUS: Displays the last 5 roll results in a side panel.
/// Each entry shows: "Roll N: [dice] → Points × Multiplier = Total"
/// </summary>
public class RollHistoryPanel : MonoBehaviour
{
    [SerializeField] private Transform         entryContainer;  // Vertical layout group parent
    [SerializeField] private TextMeshProUGUI   entryPrefab;     // Single-line TMP label prefab
    [SerializeField] private int               maxEntries = 5;

    private readonly Queue<TextMeshProUGUI> entries = new();
    private int rollNumber = 0;

    /// Called by GameOrchestrator after each completed roll.
    public void AddEntry(int dice, int points, int multiplier, int total)
    {
        rollNumber++;

        // Reuse oldest entry if at cap
        TextMeshProUGUI label;
        if (entries.Count >= maxEntries)
        {
            label = entries.Dequeue();
            label.transform.SetAsLastSibling();
        }
        else
        {
            label = Instantiate(entryPrefab, entryContainer);
        }

        label.text = $"#{rollNumber}  🎲{dice}  →  {points} × {multiplier} = {total}";
        entries.Enqueue(label);
    }
}

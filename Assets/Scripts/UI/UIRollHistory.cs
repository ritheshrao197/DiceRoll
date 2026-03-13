using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>Last 5 rolls. Subscribes to EventBus.</summary>
public class UIRollHistory : MonoBehaviour
{
    [SerializeField] private TMP_Text historyLabel;
    [SerializeField] private int      maxEntries = 5;

    private readonly Queue<int> _history = new Queue<int>();

    private void OnEnable()  => GameEventBus.OnRollCompleted += Add;
    private void OnDisable() => GameEventBus.OnRollCompleted -= Add;

    private void Add(int v)
    {
        if (_history.Count >= maxEntries) _history.Dequeue();
        _history.Enqueue(v);

        var sb = new System.Text.StringBuilder("ROLL HISTORY\n");
        var list = new List<int>(_history);
        for (int i = list.Count - 1; i >= 0; i--)
            sb.AppendLine($"  - {list[i]}");

        if (historyLabel) historyLabel.text = sb.ToString();
    }
}

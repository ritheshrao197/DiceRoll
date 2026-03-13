using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>Last 5 rolls. Subscribes to EventBus.</summary>
public class UIRollHistory : MonoBehaviour
{
    [SerializeField] private TMP_Text historyLabel;
    [SerializeField] private int      maxEntries = 5;

    private readonly List<int> _history = new List<int>(5);
    private readonly System.Text.StringBuilder _builder = new System.Text.StringBuilder(64);

    private void OnEnable()  => EventManager.OnGameEvent += HandleGameEvent;
    private void OnDisable() => EventManager.OnGameEvent -= HandleGameEvent;

    private void HandleGameEvent(GameEvent evt)
    {
        if (evt is RollCompletedEvent rollCompletedEvent)
            Add(rollCompletedEvent.FaceValue);
    }

    private void Add(int v)
    {
        if (_history.Count >= maxEntries) _history.RemoveAt(0);
        _history.Add(v);

        _builder.Clear();
        _builder.AppendLine("ROLL HISTORY");
        for (int i = _history.Count - 1; i >= 0; i--)
            _builder.Append("  - ").Append(_history[i]).AppendLine();

        if (historyLabel) historyLabel.text = _builder.ToString();
    }
}


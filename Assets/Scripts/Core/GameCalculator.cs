using UnityEngine;

/// <summary>
/// Owns Points / Multiplier / Total state.
/// Reads from EventManager, publishes back through EventManager. No direct UI references.
/// </summary>
public class GameCalculator : MonoBehaviour
{
    private const int DefaultMultiplier = 10;

    public int Points     { get; private set; }
    public int Multiplier { get; private set; }
    public int Total      { get; private set; }

    private void OnEnable()  => EventManager.OnGameEvent += HandleGameEvent;
    private void OnDisable() => EventManager.OnGameEvent -= HandleGameEvent;

    private void HandleGameEvent(GameEvent evt)
    {
        if (evt is RollCompletedEvent rollCompletedEvent)
            OnRollCompleted(rollCompletedEvent.FaceValue);
    }

    private void OnRollCompleted(int face)
    {
        // Reset and apply base result immediately
        Points     = face;
        Multiplier = DefaultMultiplier;
        Recalculate();
    }

    public void SetMultiplier(int value)
    {
        if (Multiplier == value)
            return;

        Multiplier = value;
        Recalculate();
    }

    public void AddToPoints(int bonus)
    {
        if (bonus == 0)
            return;

        Points += bonus;
        Recalculate();
    }

    private void Recalculate()
    {
        Total = Points * Multiplier;
        EventManager.TriggerEvent(new EquationChangedEvent(Points, Multiplier, Total));
    }
}


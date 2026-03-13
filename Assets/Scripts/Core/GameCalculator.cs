using UnityEngine;

/// <summary>
/// Owns Points / Multiplier / Total state.
/// Reads from EventBus, publishes back to EventBus. No direct UI references.
/// </summary>
public class GameCalculator : MonoBehaviour
{
    private const int DefaultMultiplier = 10;

    public int Points     { get; private set; }
    public int Multiplier { get; private set; }
    public int Total      { get; private set; }

    private void OnEnable()  => GameEventBus.SubscribeRollCompleted(OnRollCompleted, this);
    private void OnDisable() => GameEventBus.UnsubscribeRollCompleted(OnRollCompleted);

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
        GameEventBus.EquationChanged(Points, Multiplier, Total);
    }
}


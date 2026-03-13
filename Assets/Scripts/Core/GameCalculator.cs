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

    private void OnEnable()  => GameEventBus.OnRollCompleted += OnRollCompleted;
    private void OnDisable() => GameEventBus.OnRollCompleted -= OnRollCompleted;

    private void OnRollCompleted(int face)
    {
        // Reset and apply base result immediately
        Points     = face;
        Multiplier = DefaultMultiplier;
        Recalculate();
    }

    public void SetMultiplier(int value) { Multiplier = value; Recalculate(); }
    public void AddToPoints(int bonus)   { Points += bonus;    Recalculate(); }

    private void Recalculate()
    {
        Total = Points * Multiplier;
        GameEventBus.EquationChanged(Points, Multiplier, Total);
    }
}

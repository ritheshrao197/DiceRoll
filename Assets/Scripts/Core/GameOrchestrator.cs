using UnityEngine;

/// <summary>
/// Central coordinator that connects the dice result to the calculator
/// and then triggers Spirit Card evaluation.
///
/// Assign DiceRoller in the Inspector; the rest are accessed via singletons.
/// </summary>
public class GameOrchestrator : MonoBehaviour
{
    [SerializeField] private DiceRoller diceRoller;
    [SerializeField] private SpiritCardManager spiritCardManager;
    private void Awake()
    {
        if (diceRoller == null)
            diceRoller = FindFirstObjectByType<DiceRoller>();
        if (spiritCardManager == null)
            spiritCardManager = FindFirstObjectByType<SpiritCardManager>();
    }

    private void OnEnable()  => diceRoller.OnRollComplete += HandleRollComplete;
    private void OnDisable() => diceRoller.OnRollComplete -= HandleRollComplete;

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void HandleRollComplete(int diceResult)
    {
        // 1. Set Points from dice, reset multiplier to default, fire UI event
        GameCalculator.Instance.ApplyDiceResult(diceResult);

        // 2. Evaluate all Spirit Cards (they may further modify state and refire UI event)
        spiritCardManager.EvaluateCards(diceResult);

        // 3. (Optional) Add to roll history
        RollHistoryPanel history = FindFirstObjectByType<RollHistoryPanel>();
        history?.AddEntry(diceResult, GameCalculator.Instance.Points,
                          GameCalculator.Instance.Multiplier,
                          GameCalculator.Instance.Total);
    }
}

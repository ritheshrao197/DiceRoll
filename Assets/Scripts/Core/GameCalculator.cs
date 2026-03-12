using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the game state: Points, Multiplier, Total.
/// Notifies listeners when values change so UI and effects can react.
/// </summary>
public class GameCalculator : MonoBehaviour
{
    // ── Events ──────────────────────────────────────────────────────────────
    public  event Action<int, int, int> OnEquationChanged;   // points, multiplier, total
    public  event Action<int>           OnRollResultReady;   // fires after all card effects

    // ── State ────────────────────────────────────────────────────────────────
    public int Points     { get; private set; }
    public int Multiplier { get; private set; } = 10;
    public int Total      => Points * Multiplier;

    [Header("Roll History")]
    [SerializeField] private int historyCapacity = 5;
    private readonly Queue<int> _rollHistory = new();
    public IEnumerable<int> RollHistory => _rollHistory;

    public static GameCalculator Instance { get; internal set; }

    void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GameCalculators detected. There should only be one.");
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    public  event Action<IEnumerable<int>> OnHistoryUpdated;

    // ── References ───────────────────────────────────────────────────────────
    [SerializeField] private SpiritCardManager spiritCardManager;

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by DiceRoller once the dice animation has settled.
    /// Applies default values, then lets spirit cards modify them.
    /// </summary>
    public void ApplyDiceResult(int diceValue)
    {
        // Reset to defaults each roll
        Points     = diceValue;
        Multiplier = 10;

        // Let Spirit Cards modify Points / Multiplier before final broadcast
        spiritCardManager.EvaluateCards(diceValue);

        // Broadcast final equation state
        OnEquationChanged?.Invoke(Points, Multiplier, Total);
        OnRollResultReady?.Invoke(diceValue);

        // Record history
        RecordHistory(diceValue);
    }

    /// <summary>Lets a Spirit Card override the multiplier.</summary>
    public void SetMultiplier(int value)
    {
        Multiplier = value;
    }

    /// <summary>Lets a Spirit Card add bonus points.</summary>
    public void AddPoints(int bonus)
    {
        Points += bonus;
    }

    private void RecordHistory(int value)
    {
        _rollHistory.Enqueue(value);
        while (_rollHistory.Count > historyCapacity)
            _rollHistory.Dequeue();
        OnHistoryUpdated?.Invoke(_rollHistory);
    }
}

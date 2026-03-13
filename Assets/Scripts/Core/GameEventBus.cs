using System;
using UnityEngine;

/// <summary>
/// Static event bus — the single communication channel for the whole game.
/// No direct component references needed. Anyone can publish or subscribe.
///
/// Usage:
///   Publish:   GameEventBus.RollCompleted(4);
///   Subscribe: GameEventBus.OnRollCompleted += MyHandler;
///   Unsub:     GameEventBus.OnRollCompleted -= MyHandler;
/// </summary>
public static class GameEventBus
{
    // ── Dice ──────────────────────────────────────────────────────────────────
    /// Fired when the dice starts rolling.
    public static event Action OnRollStarted;

    /// Fired when the dice settles — payload is face value 1–6.
    public static event Action<int> OnRollCompleted;

    // ── Equation ─────────────────────────────────────────────────────────────
    /// Fired whenever Points / Multiplier / Total change.
    public static event Action<int, int, int> OnEquationChanged;  // pts, mult, total

    // ── Spirit Cards ─────────────────────────────────────────────────────────
    /// Fired when a Spirit Card activates — payload is card index (0-based).
    public static event Action<int> OnSpiritCardActivated;

    /// Fired when a Spirit Card does NOT activate — payload is card index.
    public static event Action<int> OnSpiritCardIdle;

    // ── Publishers (called by game systems) ───────────────────────────────────
    public static void RollStarted()                        => OnRollStarted?.Invoke();
    public static void RollCompleted(int face)              => OnRollCompleted?.Invoke(face);
    public static void EquationChanged(int p, int m, int t) => OnEquationChanged?.Invoke(p, m, t);
    public static void SpiritCardActivated(int idx)         => OnSpiritCardActivated?.Invoke(idx);
    public static void SpiritCardIdle(int idx)              => OnSpiritCardIdle?.Invoke(idx);

    /// Call this when loading a new scene to avoid stale subscribers.
    public static void ClearAll()
    {
        OnRollStarted        = null;
        OnRollCompleted      = null;
        OnEquationChanged    = null;
        OnSpiritCardActivated= null;
        OnSpiritCardIdle     = null;
    }
}

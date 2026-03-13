using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static event bus: the single communication channel for the whole game.
/// No direct component references needed. Anyone can publish or subscribe.
///
/// Usage:
///   Publish:   GameEventBus.RollCompleted(4);
///   Subscribe: GameEventBus.SubscribeRollCompleted(MyHandler, this);
///   Unsub:     GameEventBus.UnsubscribeRollCompleted(MyHandler);
/// </summary>
public static class GameEventBus
{
    private static readonly Dictionary<Delegate, string> SubscriberOwners = new Dictionary<Delegate, string>();

    private static Action _onRollStarted;
    private static Action<int> _onRollCompleted;
    private static Action<int, int, int> _onEquationChanged;
    private static Action<int> _onSpiritCardActivated;
    private static Action<int> _onSpiritCardIdle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLoad() => ClearAll();

    public static void SubscribeRollStarted(Action handler, UnityEngine.Object owner = null) =>
        AddListener(ref _onRollStarted, handler, nameof(SubscribeRollStarted), owner);

    public static void UnsubscribeRollStarted(Action handler) =>
        RemoveListener(ref _onRollStarted, handler);

    public static void SubscribeRollCompleted(Action<int> handler, UnityEngine.Object owner = null) =>
        AddListener(ref _onRollCompleted, handler, nameof(SubscribeRollCompleted), owner);

    public static void UnsubscribeRollCompleted(Action<int> handler) =>
        RemoveListener(ref _onRollCompleted, handler);

    public static void SubscribeEquationChanged(Action<int, int, int> handler, UnityEngine.Object owner = null) =>
        AddListener(ref _onEquationChanged, handler, nameof(SubscribeEquationChanged), owner);

    public static void UnsubscribeEquationChanged(Action<int, int, int> handler) =>
        RemoveListener(ref _onEquationChanged, handler);

    public static void SubscribeSpiritCardActivated(Action<int> handler, UnityEngine.Object owner = null) =>
        AddListener(ref _onSpiritCardActivated, handler, nameof(SubscribeSpiritCardActivated), owner);

    public static void UnsubscribeSpiritCardActivated(Action<int> handler) =>
        RemoveListener(ref _onSpiritCardActivated, handler);

    public static void SubscribeSpiritCardIdle(Action<int> handler, UnityEngine.Object owner = null) =>
        AddListener(ref _onSpiritCardIdle, handler, nameof(SubscribeSpiritCardIdle), owner);

    public static void UnsubscribeSpiritCardIdle(Action<int> handler) =>
        RemoveListener(ref _onSpiritCardIdle, handler);

    public static void RollStarted()                        => _onRollStarted?.Invoke();
    public static void RollCompleted(int face)              => _onRollCompleted?.Invoke(face);
    public static void EquationChanged(int p, int m, int t) => _onEquationChanged?.Invoke(p, m, t);
    public static void SpiritCardActivated(int idx)         => _onSpiritCardActivated?.Invoke(idx);
    public static void SpiritCardIdle(int idx)              => _onSpiritCardIdle?.Invoke(idx);

    /// <summary>Call this when loading a new scene to avoid stale subscribers.</summary>
    public static void ClearAll()
    {
        _onRollStarted = null;
        _onRollCompleted = null;
        _onEquationChanged = null;
        _onSpiritCardActivated = null;
        _onSpiritCardIdle = null;
        SubscriberOwners.Clear();
    }

    private static void AddListener<T>(ref T listeners, T handler, string eventName, UnityEngine.Object owner) where T : Delegate
    {
        if (handler == null)
            return;

        if (listeners != null && Array.Exists(listeners.GetInvocationList(), d => d == (Delegate)(object)handler))
        {
            Debug.LogWarning($"GameEventBus duplicate subscription blocked for {DescribeOwner(owner, handler)} on {eventName}.");
            return;
        }

        listeners = (T)Delegate.Combine(listeners, handler);
        SubscriberOwners[(Delegate)(object)handler] = DescribeOwner(owner, handler);
    }

    private static void RemoveListener<T>(ref T listeners, T handler) where T : Delegate
    {
        if (handler == null)
            return;

        listeners = (T)Delegate.Remove(listeners, handler);
        SubscriberOwners.Remove((Delegate)(object)handler);
    }

    private static string DescribeOwner(UnityEngine.Object owner, Delegate handler)
    {
        if (owner != null)
            return owner.name;

        return handler.Method.DeclaringType != null
            ? handler.Method.DeclaringType.Name
            : "UnknownSubscriber";
    }
}


using System;
using UnityEngine;

public static class EventManager
{
    public static event Action<GameEvent> OnGameEvent;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnLoad() => ClearAll();

    public static void TriggerEvent(GameEvent evt)
    {
        if (evt == null)
        {
            Debug.LogWarning("EventManager received a null event.");
            return;
        }

        OnGameEvent?.Invoke(evt);
    }

    public static void ClearAll()
    {
        OnGameEvent = null;
    }
}

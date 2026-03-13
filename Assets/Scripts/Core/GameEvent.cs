using UnityEngine;

public abstract class GameEvent
{
}

public sealed class RollStartedEvent : GameEvent
{
    public static readonly RollStartedEvent Instance = new RollStartedEvent();

    private RollStartedEvent()
    {
    }
}

public sealed class RollCompletedEvent : GameEvent
{
    public int FaceValue { get; }

    public RollCompletedEvent(int faceValue)
    {
        FaceValue = faceValue;
    }
}

public sealed class EquationChangedEvent : GameEvent
{
    public int Points { get; }
    public int Multiplier { get; }
    public int Total { get; }

    public EquationChangedEvent(int points, int multiplier, int total)
    {
        Points = points;
        Multiplier = multiplier;
        Total = total;
    }
}

public sealed class SpiritCardActivatedEvent : GameEvent
{
    public int CardIndex { get; }

    public SpiritCardActivatedEvent(int cardIndex)
    {
        CardIndex = cardIndex;
    }
}

public sealed class SpiritCardIdleEvent : GameEvent
{
    public int CardIndex { get; }

    public SpiritCardIdleEvent(int cardIndex)
    {
        CardIndex = cardIndex;
    }
}

using UnityEngine;

/// <summary>
/// Evaluates all Spirit Cards after each roll.
/// Uses GameCalculator directly (same GameObject via GetComponent is fine,
/// or inject via Inspector). Publishes activation/idle events to EventBus.
/// </summary>
public class SpiritCardManager : MonoBehaviour
{
    [SerializeField] private SpiritCardData[] cards;
    [SerializeField] private GameCalculator   calculator;

    public void Evaluate(int diceValue)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;

            if (cards[i].IsTriggered(diceValue))
            {
                cards[i].ApplyEffect(calculator);
                GameEventBus.SpiritCardActivated(i);
            }
            else
            {
                GameEventBus.SpiritCardIdle(i);
            }
        }
    }
}

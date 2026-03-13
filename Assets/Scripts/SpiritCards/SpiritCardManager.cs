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

    private void Awake()
    {
        if (calculator == null)
            Debug.LogError("SpiritCardManager requires a GameCalculator reference.", this);
        if (cards == null || cards.Length == 0)
            Debug.LogWarning("SpiritCardManager has no spirit cards assigned.", this);
    }

    public void Evaluate(int diceValue)
    {
        if (calculator == null || cards == null)
            return;

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

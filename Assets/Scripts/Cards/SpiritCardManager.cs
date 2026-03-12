using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds references to all SpiritCardData assets and their matching SpiritCardView components.
/// GameCalculator calls EvaluateCards() during a result; this class routes activation signals
/// to the correct view.
/// </summary>
public class SpiritCardManager : MonoBehaviour
{
    [System.Serializable]
    public struct CardEntry
    {
        public SpiritCardData data;
        public SpiritCardView view;
    }

    [SerializeField] private List<CardEntry> cards = new();

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Iterates all registered cards, applies effects for triggered ones,
    /// and signals each view whether it activated.
    /// </summary>
    public void EvaluateCards(int diceValue)
    {
        foreach (var entry in cards)
        {
            bool triggered = entry.data.IsTriggered(diceValue);

            if (triggered)
                entry.data.ApplyEffect();

            // Notify view regardless so it can show idle/active state
            entry.view?.SetActivationState(triggered);
        }
    }

    /// <summary>Resets all card views to idle (call at start of each roll).</summary>
    public void ResetAllCards()
    {
        foreach (var entry in cards)
            entry.view?.SetActivationState(false);
    }
}

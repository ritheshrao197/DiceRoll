using UnityEngine;

/// <summary>
/// Data definition for a Spirit Card.
/// Create via: Assets → Create → DiceSpirit → SpiritCard
/// </summary>
[CreateAssetMenu(menuName = "DiceSpirit/SpiritCard", fileName = "SpiritCard_New")]
public class SpiritCardData : ScriptableObject
{
    [Header("Identity")]
    public string cardName        = "Spirit Card";
    [TextArea(2, 4)]
    public string description     = "Card description.";

    [Header("Trigger")]
    [Tooltip("The dice face value (1–6) that activates this card. Set to 0 to never trigger.")]
    public int triggerOnDiceValue = 6;

    [Header("Effect")]
    public CardEffectType effectType = CardEffectType.SetMultiplier;
    [Tooltip("Value used by the effect (e.g. new multiplier, bonus points).")]
    public int effectValue = 2;

    [Header("Visuals")]
    public Color cardAccentColor  = new Color(1f, 0.85f, 0.1f, 1f); // gold default
    public Sprite cardArtwork;                                        // optional

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Returns true when this card should fire for the given dice result.</summary>
    public bool IsTriggered(int diceValue) => triggerOnDiceValue != 0 && diceValue == triggerOnDiceValue;

    /// <summary>Applies the card's effect to the calculator.</summary>
    public void ApplyEffect()
    {
        switch (effectType)
        {
            case CardEffectType.SetMultiplier:
                GameCalculator.Instance.SetMultiplier(effectValue);
                break;

            case CardEffectType.AddPoints:
                GameCalculator.Instance.AddPoints(effectValue);
                break;
        }
    }
}

public enum CardEffectType
{
    SetMultiplier,
    AddPoints
}

using UnityEngine;

/// <summary>
/// ScriptableObject defining one Spirit Card.
/// Right-click in Project > Create > DiceGame > SpiritCard
/// </summary>
[CreateAssetMenu(fileName = "SpiritCard_New", menuName = "DiceGame/SpiritCard")]
public class SpiritCardData : ScriptableObject
{
    [Header("Identity")]
    public string cardName   = "Spirit Card";
    [TextArea(2, 4)]
    public string description = "";

    [Header("Trigger")]
    [Tooltip("Dice value that activates this card (1-6). 0 = always triggers.")]
    public int triggerOnDiceValue = 6;

    [Header("Effect")]
    public SpiritEffectType effectType  = SpiritEffectType.SetMultiplier;
    public int              effectValue = 2;

    public bool IsTriggered(int diceValue) =>
        triggerOnDiceValue == 0 || diceValue == triggerOnDiceValue;

    public void ApplyEffect(GameCalculator calc)
    {
        if (calc == null)
        {
            Debug.LogError($"SpiritCardData '{cardName}' cannot apply without a GameCalculator.");
            return;
        }

        switch (effectType)
        {
            case SpiritEffectType.SetMultiplier: calc.SetMultiplier(effectValue); break;
            case SpiritEffectType.AddToPoints:   calc.AddToPoints(effectValue);   break;
        }
    }
}

public enum SpiritEffectType { SetMultiplier, AddToPoints }



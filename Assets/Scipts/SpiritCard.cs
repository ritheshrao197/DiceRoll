using UnityEngine;
namespace DiceRollingGame
{
public abstract class SpiritCard : ScriptableObject
{
    public string cardName;

    public abstract bool ShouldTrigger(int diceResult);

    public abstract void ApplyEffect(ref int points, ref int multiplier);
}
}
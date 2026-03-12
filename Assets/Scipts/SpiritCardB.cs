using UnityEngine;
namespace DiceRollingGame
{
    [CreateAssetMenu(menuName="SpiritCards/CardB")]
public class SpiritCardB : SpiritCard
{
    public override bool ShouldTrigger(int diceResult)
    {
        return diceResult == 3;
    }

    public override void ApplyEffect(ref int points, ref int multiplier)
    {
        points += 10;
    }
}
}
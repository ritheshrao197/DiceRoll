using UnityEngine;
namespace DiceRollingGame
{
    [CreateAssetMenu(menuName = "SpiritCards/CardA")]
    public class SpiritCardA : SpiritCard
    {
        public override bool ShouldTrigger(int diceResult)
        {
            return diceResult == 6;
        }

        public override void ApplyEffect(ref int points, ref int multiplier)
        {
            multiplier = 2;
        }
    }
}
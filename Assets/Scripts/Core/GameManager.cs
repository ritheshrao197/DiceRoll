using System.Collections;
using UnityEngine;

/// <summary>
/// Coordinates the roll-to-spirit-card evaluation timing.
/// Subscribes to EventBus. Only direct reference is to DiceRoller (to call Roll()).
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private DiceRoller        diceRoller;
    [SerializeField] private SpiritCardManager spiritCardManager;
    [SerializeField] private float             spiritCardDelay = 0.5f;

    private void OnEnable()  => GameEventBus.OnRollCompleted += OnRollCompleted;
    private void OnDisable() => GameEventBus.OnRollCompleted -= OnRollCompleted;

    private void OnRollCompleted(int face) =>
        StartCoroutine(DelayedCardEval(face));

    private IEnumerator DelayedCardEval(int face)
    {
        yield return new WaitForSeconds(spiritCardDelay);
        spiritCardManager.Evaluate(face);
    }

    /// Called by the Roll Button.
    public void RequestRoll()
    {
        if (diceRoller != null && !diceRoller.IsRolling)
            diceRoller.Roll();
    }
}
    private void Awake()
    {
        if (diceRoller == null)
            Debug.LogError("GameManager requires a DiceRoller reference.", this);
        if (spiritCardManager == null)
            Debug.LogError("GameManager requires a SpiritCardManager reference.", this);
    }

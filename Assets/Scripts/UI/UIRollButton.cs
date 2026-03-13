using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Roll button. Subscribes to EventBus for rolling state.
/// Only direct reference: GameManager (to call RequestRoll).
/// </summary>
[RequireComponent(typeof(Button))]
public class UIRollButton : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMP_Text    label;

    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        if (gameManager == null)
            Debug.LogError("UIRollButton requires a GameManager reference.", this);
        _btn.onClick.AddListener(() => gameManager?.RequestRoll());
    }

    private void OnEnable()
    {
        GameEventBus.OnRollStarted   += OnRollStarted;
        GameEventBus.OnRollCompleted += OnRollCompleted;
    }

    private void OnDisable()
    {
        GameEventBus.OnRollStarted   -= OnRollStarted;
        GameEventBus.OnRollCompleted -= OnRollCompleted;
    }

    private void OnRollStarted()      => SetState(true);
    private void OnRollCompleted(int _) => SetState(false);

    private void SetState(bool rolling)
    {
        _btn.interactable = !rolling;
        if (label) label.text = rolling ? "Rolling..." : "ROLL DICE";
    }
}

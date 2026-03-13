using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Debug panel to force roll results. Editor/Dev builds only.</summary>
public class UIDebugPanel : MonoBehaviour
{
    [SerializeField] private DiceRoller diceRoller;
    [SerializeField] private Button     force3Btn;
    [SerializeField] private Button     force6Btn;
    [SerializeField] private Button     clearBtn;
    [SerializeField] private TMP_Text   statusLabel;

    private void Awake()
    {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        gameObject.SetActive(false);
        return;
#endif
        force3Btn?.onClick.AddListener(() => SetForce(3));
        force6Btn?.onClick.AddListener(() => SetForce(6));
        clearBtn?.onClick.AddListener(Clear);
    }

    private void SetForce(int v)
    {
        Debug.Log($"UIDebugPanel: Forcing roll result to {v}");
        diceRoller?.SetDebugForce(true, v);
        if (statusLabel) statusLabel.text = $"Forcing: {v}";
    }

    private void Clear()
    {
        diceRoller?.SetDebugForce(false, 1);
        if (statusLabel) statusLabel.text = "Random";
    }
}

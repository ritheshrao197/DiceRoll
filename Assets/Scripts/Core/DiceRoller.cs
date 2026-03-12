using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the dice roll sequence:
///   1. Plays a rapid-number spin animation.
///   2. Eases into the final result.
///   3. Fires OnRollComplete so GameCalculator can process the result.
/// Disables the Roll button while rolling to prevent re-entrancy.
/// </summary>
public class DiceRoller : MonoBehaviour
{
    // ── Events ───────────────────────────────────────────────────────────────
    public  event Action<int> OnRollComplete;

    // ── Inspector ────────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private Button            rollButton;
    [SerializeField] private TextMeshProUGUI   diceFaceLabel;
    [SerializeField] private RectTransform     diceRect;
    [SerializeField] private GameCalculator    calculator;
    [SerializeField] private SpiritCardManager spiritCardManager;

    [Header("Roll Animation")]
    [SerializeField] private float rollDuration      = 1.2f;
    [SerializeField] private float fastTickInterval  = 0.07f;
    [SerializeField] private float slowTickInterval  = 0.25f;
    [SerializeField] private float shakeStrength     = 12f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   rollTickSfx;
    [SerializeField] private AudioClip   settleSfx;

    // ── Runtime state ────────────────────────────────────────────────────────
    private bool      _isRolling     = false;
    private bool      _debugForce    = false;
    private int       _debugValue    = 6;
    private Vector3   _diceOriginPos;
    private Coroutine _rollRoutine;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _diceOriginPos = diceRect != null ? diceRect.anchoredPosition3D : Vector3.zero;
    }

    private void OnEnable()  => rollButton.onClick.AddListener(BeginRoll);
    private void OnDisable() => rollButton.onClick.RemoveListener(BeginRoll);

    /// <summary>Called by the Roll button.</summary>
    public void BeginRoll()
    {
        if (_isRolling) return;

        int result = _debugForce
            ? _debugValue
            : UnityEngine.Random.Range(1, 7);

        if (_rollRoutine != null) StopCoroutine(_rollRoutine);
        _rollRoutine = StartCoroutine(RollSequence(result));
    }

    /// <summary>Used by DebugRollPanel to override the next roll result.</summary>
    public void SetDebugForce(bool active, int value)
    {
        _debugForce = active;
        _debugValue = Mathf.Clamp(value, 1, 6);
    }

    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator RollSequence(int finalResult)
    {
        _isRolling = true;
        SetRollButtonInteractable(false);
        spiritCardManager?.ResetAllCards();

        float elapsed   = 0f;
        float tickTimer = 0f;

        while (elapsed < rollDuration)
        {
            elapsed   += Time.deltaTime;
            tickTimer += Time.deltaTime;

            float progress     = elapsed / rollDuration;
            float tickInterval = Mathf.Lerp(fastTickInterval, slowTickInterval, EaseIn(progress));

            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                SetDiceFace(UnityEngine.Random.Range(1, 7));
                PlayTickSound();
            }

            float shakeAmt = shakeStrength * (1f - progress) * Mathf.Sin(elapsed * 8f);
            if (diceRect)
                diceRect.anchoredPosition3D = _diceOriginPos + new Vector3(shakeAmt, shakeAmt * 0.5f, 0f);

            yield return null;
        }

        if (diceRect) diceRect.anchoredPosition3D = _diceOriginPos;
        SetDiceFace(finalResult);
        PlaySettleSound();

        yield return new WaitForSeconds(0.25f);
        yield return PunchScale(diceRect, 1.35f, 0.18f);

        calculator.ApplyDiceResult(finalResult);
        OnRollComplete?.Invoke(finalResult);

        _isRolling = false;
        SetRollButtonInteractable(true);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetDiceFace(int value)
    {
        if (diceFaceLabel) diceFaceLabel.text = value.ToString();
    }

    private void SetRollButtonInteractable(bool state)
    {
        if (rollButton) rollButton.interactable = state;
    }

    private void PlayTickSound()
    {
        if (audioSource && rollTickSfx)
            audioSource.PlayOneShot(rollTickSfx, 0.4f);
    }

    private void PlaySettleSound()
    {
        if (audioSource && settleSfx)
            audioSource.PlayOneShot(settleSfx);
    }

    private IEnumerator PunchScale(RectTransform target, float peakScale, float duration)
    {
        if (!target) yield break;
        Vector3 baseScale = target.localScale;
        float   half      = duration * 0.5f;
        float   t         = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(baseScale, baseScale * peakScale, t / half);
            yield return null;
        }
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(baseScale * peakScale, baseScale, t / half);
            yield return null;
        }
        target.localScale = baseScale;
    }

    private float EaseIn(float t) => t * t;
}

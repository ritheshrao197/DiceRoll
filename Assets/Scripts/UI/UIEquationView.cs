using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Responsible solely for displaying and animating the equation:
///   Points × Multiplier = Total
///
/// Subscribes to GameCalculator.OnEquationChanged and runs:
///   - Count-up tween for each number
///   - Bounce-scale when a value changes
///   - Color flash on the Total when the equation finalises
/// </summary>
public class UIEquationView : MonoBehaviour
{
    [Header("Equation Labels")]
    [SerializeField] private TextMeshProUGUI pointsLabel;
    [SerializeField] private TextMeshProUGUI multiplierLabel;
    [SerializeField] private TextMeshProUGUI totalLabel;

    [Header("Operator / Separator Labels (optional)")]
    [SerializeField] private TextMeshProUGUI timesLabel;   // "×"
    [SerializeField] private TextMeshProUGUI equalsLabel;  // "="

    [Header("Count-Up Settings")]
    [Tooltip("Duration of the count-up tween in seconds.")]
    [SerializeField] private float countUpDuration = 0.55f;

    [Header("Flash Colors")]
    [SerializeField] private Color normalColor    = Color.white;
    [SerializeField] private Color changedColor   = new Color(1f, 0.92f, 0.2f, 1f);  // yellow flash
    [SerializeField] private Color totalFlashColor = new Color(0.3f, 1f, 0.45f, 1f); // green pop
    [SerializeField] private float flashDuration  = 0.4f;

    // ── Coroutine handles so new values cancel old tweens ────────────────────
    private Coroutine _pointsRoutine;
    private Coroutine _multiplierRoutine;
    private Coroutine _totalRoutine;

    // Cached previous values to detect changes
    private int _prevPoints     = 0;
    private int _prevMultiplier = 10;
    private int _prevTotal      = 0;

    // ─────────────────────────────────────────────────────────────────────────

    private void OnEnable()  => GameCalculator.Instance.OnEquationChanged += HandleEquationChanged;
    private void OnDisable() => GameCalculator.Instance.OnEquationChanged -= HandleEquationChanged;

    private void Start()
    {
        // Set initial display
        SetLabelImmediate(pointsLabel,     0);
        SetLabelImmediate(multiplierLabel, 10);
        SetLabelImmediate(totalLabel,      0);
    }

    // ─────────────────────────────────────────────────────────────────────────

    private void HandleEquationChanged(int points, int multiplier, int total)
    {
        bool pointsChanged     = points     != _prevPoints;
        bool multiplierChanged = multiplier != _prevMultiplier;
        bool totalChanged      = total      != _prevTotal;

        if (pointsChanged)
        {
            if (_pointsRoutine != null) StopCoroutine(_pointsRoutine);
            _pointsRoutine = StartCoroutine(AnimateValue(pointsLabel, _prevPoints, points, changedColor, false));
        }

        if (multiplierChanged)
        {
            if (_multiplierRoutine != null) StopCoroutine(_multiplierRoutine);
            _multiplierRoutine = StartCoroutine(AnimateValue(multiplierLabel, _prevMultiplier, multiplier, changedColor, false));
        }

        if (totalChanged)
        {
            if (_totalRoutine != null) StopCoroutine(_totalRoutine);
            // Total gets the special green "result finalised" treatment
            _totalRoutine = StartCoroutine(AnimateValue(totalLabel, _prevTotal, total, totalFlashColor, true));

            // Brief emphasis on the "=" sign
            if (equalsLabel) StartCoroutine(FlashLabel(equalsLabel, changedColor, flashDuration));
        }

        _prevPoints     = points;
        _prevMultiplier = multiplier;
        _prevTotal      = total;
    }

    // ── Animation coroutines ─────────────────────────────────────────────────

    /// <summary>
    /// Tweens the displayed number from <paramref name="from"/> to <paramref name="to"/>
    /// with a colour flash and an optional scale punch.
    /// </summary>
    private IEnumerator AnimateValue(TextMeshProUGUI label, int from, int to,
                                     Color flashColor, bool bigPunch)
    {
        if (!label) yield break;

        RectTransform rect      = label.rectTransform;
        Vector3       baseScale = rect.localScale;

        // Colour flash start
        label.color = flashColor;

        float elapsed = 0f;

        while (elapsed < countUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countUpDuration);

            // Count-up: ease-out curve
            int displayed = Mathf.RoundToInt(Mathf.Lerp(from, to, EaseOut(t)));
            label.text = displayed.ToString();

            // Scale bounce (peaks at 40% of tween, back to 1 by end)
            float scaleCurve = bigPunch ? 1f + 0.35f * Mathf.Sin(t * Mathf.PI)
                                        : 1f + 0.18f * Mathf.Sin(t * Mathf.PI);
            rect.localScale = baseScale * scaleCurve;

            // Colour fades from flashColor → normalColor
            label.color = Color.Lerp(flashColor, normalColor, t);

            yield return null;
        }

        label.text  = to.ToString();
        label.color = normalColor;
        rect.localScale = baseScale;
    }

    private IEnumerator FlashLabel(TextMeshProUGUI label, Color flashColor, float duration)
    {
        if (!label) yield break;
        label.color = flashColor;
        yield return new WaitForSeconds(duration);
        label.color = normalColor;
    }

    private void SetLabelImmediate(TextMeshProUGUI label, int value)
    {
        if (label) label.text = value.ToString();
    }

    private float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}

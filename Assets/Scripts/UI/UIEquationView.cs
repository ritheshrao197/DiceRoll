using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays "Points x Multiplier = Total".
/// Subscribes to GameEventBus.OnEquationChanged with no polling or singleton access.
/// Animates: count-up tween + bounce scale + gold flash on Total.
/// </summary>
public class UIEquationView : MonoBehaviour
{
    [Header("Labels - assign in Inspector")]
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text multiplierText;
    [SerializeField] private TMP_Text totalText;

    [Header("Animation")]
    [SerializeField] private float countDuration = 0.4f;
    [SerializeField] private float bouncePeak    = 1.25f;
    [SerializeField] private Color flashColor    = new Color(1f, 0.85f, 0.1f);
    [SerializeField] private float flashDuration = 0.55f;

    // Track previous values so we only animate what changed
    private int _pts = 0, _mult = 10, _total = 0;
    private Coroutine _pointsRoutine;
    private Coroutine _multiplierRoutine;
    private Coroutine _totalRoutine;
    private Coroutine _flashRoutine;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (pointsText == null || multiplierText == null || totalText == null)
            Debug.LogError("UIEquationView requires all TMP label references.", this);
        SetImmediate(pointsText,     0);
        SetImmediate(multiplierText, 10);
        SetImmediate(totalText,      0);
    }

    private void OnEnable() => GameEventBus.SubscribeEquationChanged(OnEquationChanged, this);
    private void OnDisable() => GameEventBus.UnsubscribeEquationChanged(OnEquationChanged);

    // ── Handler ───────────────────────────────────────────────────────────────

    private void OnEquationChanged(int pts, int mult, int total)
    {
        if (pts != _pts)
            RestartRoutine(ref _pointsRoutine, Animate(pointsText, _pts, pts, false));
        if (mult != _mult)
            RestartRoutine(ref _multiplierRoutine, Animate(multiplierText, _mult, mult, false));
        if (total != _total)
            RestartRoutine(ref _totalRoutine, Animate(totalText, _total, total, true));

        _pts = pts; _mult = mult; _total = total;
    }

    // ── Animation coroutine ───────────────────────────────────────────────────

    private IEnumerator Animate(TMP_Text label, int from, int to, bool doFlash)
    {
        if (label == null) yield break;

        Vector3 baseScale = label.transform.localScale;
        float   elapsed   = 0f;

        while (elapsed < countDuration)
        {
            elapsed += Time.deltaTime;
            float t    = Mathf.Clamp01(elapsed / countDuration);
            float ease = 1f - Mathf.Pow(1f - t, 3f);   // ease-out cubic

            label.text = Mathf.RoundToInt(Mathf.Lerp(from, to, ease)).ToString();

            // Bounce scale: peaks at midpoint then returns to normal
            float s = 1f + (bouncePeak - 1f) * Mathf.Sin(t * Mathf.PI);
            label.transform.localScale = baseScale * s;
            yield return null;
        }

        label.text = to.ToString();
        label.transform.localScale = baseScale;

        if (doFlash)
            RestartRoutine(ref _flashRoutine, Flash(label));
    }

    private IEnumerator Flash(TMP_Text label)
    {
        if (label == null) yield break;
        Color original = label.color;
        float inD  = flashDuration * 0.35f;
        float outD = flashDuration * 0.65f;
        float e    = 0f;

        while (e < inD)  { e += Time.deltaTime; label.color = Color.Lerp(original, flashColor, e/inD);  yield return null; }
        e = 0f;
        while (e < outD) { e += Time.deltaTime; label.color = Color.Lerp(flashColor, original, e/outD); yield return null; }
        label.color = original;
    }

    private static void SetImmediate(TMP_Text label, int value)
    {
        if (label != null) label.text = value.ToString();
    }

    private void RestartRoutine(ref Coroutine routine, IEnumerator enumerator)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(enumerator);
    }
}


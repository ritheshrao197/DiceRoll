using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Displays "Points × Multiplier = Total".
/// Subscribes to GameEventBus.OnEquationChanged — no polling, no singleton access.
/// Animates: count-up tween + bounce scale + gold flash on Total.
///
/// FIX: subscribes in Start (not OnEnable) to guarantee scene is fully initialised.
/// </summary>
public class UIEquationView : MonoBehaviour
{
    [Header("Labels — assign in Inspector")]
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

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        // Subscribe here (not OnEnable) so the bus exists and labels are assigned
        GameEventBus.OnEquationChanged += OnEquationChanged;

        // Show defaults immediately
        SetImmediate(pointsText,     0);
        SetImmediate(multiplierText, 10);
        SetImmediate(totalText,      0);
    }

    private void OnDestroy() => GameEventBus.OnEquationChanged -= OnEquationChanged;

    // ── Handler ───────────────────────────────────────────────────────────────

    private void OnEquationChanged(int pts, int mult, int total)
    {
        if (pts   != _pts)   StartCoroutine(Animate(pointsText,     _pts,   pts,   false));
        if (mult  != _mult)  StartCoroutine(Animate(multiplierText, _mult,  mult,  false));
        if (total != _total) StartCoroutine(Animate(totalText,      _total, total, true));

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

        if (doFlash) StartCoroutine(Flash(label));
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
}

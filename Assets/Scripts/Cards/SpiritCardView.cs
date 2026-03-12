using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles all visual feedback for a single Spirit Card:
/// idle state, activation glow/pulse, and particle burst.
/// Attach to the card's root UI GameObject.
/// </summary>
public class SpiritCardView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpiritCardData data;
    [SerializeField] private Image          cardBackground;
    [SerializeField] private Image          glowImage;       // separate overlay image for glow
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI descLabel;
    [SerializeField] private ParticleSystem  burstParticles;
    [SerializeField] private AudioSource     audioSource;
    [SerializeField] private AudioClip       activationSfx;

    [Header("Idle Visuals")]
    [SerializeField] private Color idleColor   = new Color(0.15f, 0.15f, 0.25f, 1f);
    [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0.1f, 1f);

    [Header("Animation Timings")]
    [SerializeField] private float pulseDuration   = 0.25f;
    [SerializeField] private float pulseScaleMax   = 1.25f;
    [SerializeField] private float glowFadeDuration = 1.5f;

    private RectTransform _rect;
    private Vector3       _baseScale;
    private Coroutine     _activationRoutine;

    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _rect      = GetComponent<RectTransform>();
        _baseScale = _rect.localScale;

        // Populate labels from ScriptableObject data
        if (data != null)
        {
            if (nameLabel) nameLabel.text = data.cardName;
            if (descLabel) descLabel.text = data.description;
            if (cardBackground) cardBackground.color = idleColor;
        }

        SetGlowAlpha(0f);
    }

    /// <summary>
    /// Called by SpiritCardManager after dice evaluation.
    /// If activated == true, runs the full activation sequence.
    /// </summary>
    public void SetActivationState(bool activated)
    {
        if (_activationRoutine != null)
            StopCoroutine(_activationRoutine);

        if (activated)
            _activationRoutine = StartCoroutine(PlayActivation());
        else
            ResetToIdle();
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private void ResetToIdle()
    {
        _rect.localScale = _baseScale;
        SetGlowAlpha(0f);
        if (cardBackground) cardBackground.color = idleColor;
    }

    private IEnumerator PlayActivation()
    {
        // 1) Flash card background to accent color
        if (cardBackground) cardBackground.color = data != null ? data.cardAccentColor : activeColor;

        // 2) Scale punch up
        yield return ScalePunch(pulseScaleMax, pulseDuration);

        // 3) Play burst particles
        if (burstParticles)
        {
            // Tint particles to match card accent
            var main = burstParticles.main;
            if (data != null) main.startColor = data.cardAccentColor;
            burstParticles.Play();
        }

        // 4) SFX
        if (audioSource && activationSfx)
            audioSource.PlayOneShot(activationSfx);

        // 5) Glow fade-out
        yield return GlowPulse();

        // Keep card accent color to show it stayed active this round
    }

    private IEnumerator ScalePunch(float targetScale, float duration)
    {
        float half = duration * 0.5f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            _rect.localScale = Vector3.Lerp(_baseScale, _baseScale * targetScale, EaseOut(t));
            yield return null;
        }
        _rect.localScale = _baseScale * targetScale;

        elapsed = 0f;
        // Scale back
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / half;
            _rect.localScale = Vector3.Lerp(_baseScale * targetScale, _baseScale, EaseOut(t));
            yield return null;
        }
        _rect.localScale = _baseScale;
    }

    private IEnumerator GlowPulse()
    {
        // Quick fade-in
        float fadeIn = glowFadeDuration * 0.15f;
        yield return FadeGlow(0f, 1f, fadeIn);

        // Hold briefly
        yield return new WaitForSeconds(0.15f);

        // Fade out
        yield return FadeGlow(1f, 0f, glowFadeDuration * 0.85f);
    }

    private IEnumerator FadeGlow(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetGlowAlpha(Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }
        SetGlowAlpha(to);
    }

    private void SetGlowAlpha(float alpha)
    {
        if (!glowImage) return;
        var c = glowImage.color;
        c.a = alpha;
        glowImage.color = c;
    }

    private float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}

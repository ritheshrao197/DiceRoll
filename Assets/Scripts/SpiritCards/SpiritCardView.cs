using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Visual for one Spirit Card. Subscribes to EventBus using its own index.
/// No reference to DiceRoller or GameManager needed.
/// </summary>
public class SpiritCardView : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private int cardIndex;  // must match index in SpiritCardManager.cards[]

    [Header("Data")]
    [SerializeField] private SpiritCardData cardData;
    public SpiritCardData Data => cardData;

    [Header("UI")]
    [SerializeField] private Image    background;
    [SerializeField] private Image    glowBorder;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private TMP_Text triggerLabel;
    [SerializeField] private TMP_Text descLabel;

    [Header("FX")]
    [SerializeField] private ParticleSystem burstFX;

    [Header("Colors")]
    [SerializeField] private Color activeColor   = new Color(1f,   0.95f, 0.8f,  1f);
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f,  0.5f,  1f);
    [SerializeField] private Color glowColor     = new Color(1f,   0.78f, 0f,    1f);

    private Vector3 _baseScale;

    private void Awake()  => _baseScale = transform.localScale;

    private void Start()
    {
        PopulateText();
        SetGlow(0f);
        SetBody(inactiveColor);
    }

    private void OnEnable()
    {
        GameEventBus.OnSpiritCardActivated += OnActivated;
        GameEventBus.OnSpiritCardIdle      += OnIdle;
    }

    private void OnDisable()
    {
        GameEventBus.OnSpiritCardActivated -= OnActivated;
        GameEventBus.OnSpiritCardIdle      -= OnIdle;
    }

    private void OnActivated(int idx) { if (idx == cardIndex) Activate(); }
    private void OnIdle(int idx)      { if (idx == cardIndex) Idle(); }

    // ── Animations ────────────────────────────────────────────────────────────

    private void Activate()
    {
        StopAllCoroutines();
        StartCoroutine(ActivateRoutine());
    }

    private void Idle()
    {
        StopAllCoroutines();
        StartCoroutine(IdleRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        SetBody(activeColor);
        burstFX?.Play();

        // Scale pulse
        float dur = 0.35f, e = 0f;
        while (e < dur)
        {
            e += Time.deltaTime;
            float s = 1f + 0.2f * Mathf.Sin((e / dur) * Mathf.PI);
            transform.localScale = _baseScale * s;
            yield return null;
        }
        transform.localScale = _baseScale;

        // Glow fade
        SetGlow(1f);
        e = 0f;
        while (e < 1.2f)
        {
            e += Time.deltaTime;
            SetGlow(1f - e / 1.2f);
            yield return null;
        }
        SetGlow(0f);
    }

    private IEnumerator IdleRoutine()
    {
        float e = 0f;
        Color from = background != null ? background.color : activeColor;
        while (e < 0.3f)
        {
            e += Time.deltaTime;
            SetBody(Color.Lerp(from, inactiveColor, e / 0.3f));
            yield return null;
        }
        SetBody(inactiveColor);
        SetGlow(0f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void PopulateText()
    {
        if (cardData == null) return;
        if (nameLabel)    nameLabel.text    = cardData.cardName;
        if (triggerLabel) triggerLabel.text = $"Roll: {cardData.triggerOnDiceValue}";
        if (descLabel)    descLabel.text    = cardData.description;
    }

    private void SetBody(Color c)  { if (background) background.color = c; }
    private void SetGlow(float a)
    {
        if (!glowBorder) return;
        var c = glowColor; c.a = a;
        glowBorder.color = c;
    }
}

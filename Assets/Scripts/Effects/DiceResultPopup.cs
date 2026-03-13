using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>Spring-pop number when dice settles. Subscribes to EventManager.</summary>
public class DiceResultPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private void Start()     { if (label) label.alpha = 0f; }
    private void OnEnable()  => EventManager.OnGameEvent += HandleGameEvent;
    private void OnDisable() => EventManager.OnGameEvent -= HandleGameEvent;

    private void HandleGameEvent(GameEvent evt)
    {
        if (evt is RollCompletedEvent rollCompletedEvent)
            Show(rollCompletedEvent.FaceValue);
    }

    private void Show(int v) { StopAllCoroutines(); StartCoroutine(Pop(v)); }

    private IEnumerator Pop(int v)
    {
        if (!label) yield break;
        label.text  = v.ToString();
        label.alpha = 1f;
        label.transform.localScale = Vector3.zero;

        // Spring in
        float e = 0f, dur = 0.22f;
        while (e < dur)
        {
            e += Time.deltaTime;
            float t = Mathf.Clamp01(e / dur);
            float s = Mathf.Max(0f, EaseOutBack(t) * 1.4f);
            label.transform.localScale = Vector3.one * s;
            yield return null;
        }
        label.transform.localScale = Vector3.one;

        yield return new WaitForSeconds(0.55f);

        // Fade
        e = 0f;
        while (e < 0.3f) { e += Time.deltaTime; label.alpha = 1f - e/0.3f; yield return null; }
        label.alpha = 0f;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t-1f,3f) + c1 * Mathf.Pow(t-1f,2f);
    }
}


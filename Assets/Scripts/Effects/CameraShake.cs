using System.Collections;
using UnityEngine;

/// <summary>Camera shake on roll start. Subscribes to EventManager.</summary>
public class CameraShake : MonoBehaviour
{
    [SerializeField] private float magnitude = 0.08f;
    [SerializeField] private float frequency = 28f;
    [SerializeField] private float duration  = 1.0f;

    private Vector3 _origin;

    private void Awake()     => _origin = transform.localPosition;
    private void OnEnable()  => EventManager.OnGameEvent += HandleGameEvent;
    private void OnDisable() => EventManager.OnGameEvent -= HandleGameEvent;

    private void HandleGameEvent(GameEvent evt)
    {
        if (evt is RollStartedEvent)
            StartShake();
    }

    private void StartShake() { StopAllCoroutines(); StartCoroutine(Shake()); }

    private IEnumerator Shake()
    {
        float e = 0f;
        while (e < duration)
        {
            e += Time.deltaTime;
            float decay = 1f - e / duration;
            transform.localPosition = _origin + new Vector3(
                Mathf.Sin(e * frequency)        * magnitude * decay,
                Mathf.Cos(e * frequency * 1.3f) * magnitude * decay, 0f);
            yield return null;
        }
        transform.localPosition = _origin;
    }
}


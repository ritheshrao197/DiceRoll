using UnityEngine;

/// <summary>Enables trail while rolling. Subscribes to EventBus.</summary>
[RequireComponent(typeof(TrailRenderer))]
public class DiceTrailEffect : MonoBehaviour
{
    private TrailRenderer _trail;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();
        _trail.time       = 0.2f;
        _trail.startWidth = 0.2f;
        _trail.endWidth   = 0f;
        _trail.startColor = new Color(1f, 0.82f, 0.2f, 0.8f);
        _trail.endColor   = new Color(1f, 0.4f,  0.1f, 0f);
        var mat = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));
        if (mat) _trail.material = mat;
        _trail.emitting = false;
    }

    // Named methods so unsubscription actually works (lambdas create new delegates)
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

    private void OnRollStarted()        => _trail.emitting = true;
    private void OnRollCompleted(int _) => _trail.emitting = false;
}

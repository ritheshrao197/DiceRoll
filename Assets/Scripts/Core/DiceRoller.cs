using System.Collections;
using UnityEngine;

/// <summary>
/// Realistic physics-based dice roller.
/// Publishes to GameEventBus; no direct references to any other system.
///
/// Roll sequence:
///   1. Spawn above table with random rotation
///   2. Apply downward impulse + random torque so physics runs freely
///   3. Wait until velocity drops below threshold (or timeout)
///   4. Read which face is up (dot product against World.up)
///   5. Smooth-slerp to clean face orientation
///   6. Publish GameEventBus.RollCompleted(faceValue)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class DiceRoller : MonoBehaviour
{
    private static readonly WaitForSeconds InitialRollDelay = new WaitForSeconds(0.35f);
    private static readonly WaitForFixedUpdate FixedUpdateYield = new WaitForFixedUpdate();

    [Header("Launch")]
    [SerializeField] private float launchHeight   = 2.5f;
    [SerializeField] private float launchForce    = 4f;
    [SerializeField] private float torqueMin      = 8f;
    [SerializeField] private float torqueMax      = 18f;
    [SerializeField] private float spreadRadius   = 0.3f;

    [Header("Settle")]
    [SerializeField] private float velocityThreshold = 0.05f;
    [SerializeField] private float angularThreshold  = 0.12f;
    [SerializeField] private float maxWaitTime       = 5f;
    [SerializeField] private float snapDuration      = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool  forceResult  = false;
    [SerializeField][Range(1,6)] private int forcedValue = 6;

    public bool IsRolling { get; private set; }

    private Rigidbody _rb;
    private Vector3   _restPos;
    private BoxCollider _boxCollider;
    private static PhysicsMaterial s_diceMaterial;

    private void Awake()
    {
        _rb      = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
        _restPos = transform.position;
        SetupPhysics();
    }

    private void OnValidate()
    {
        launchHeight = Mathf.Max(0f, launchHeight);
        launchForce = Mathf.Max(0f, launchForce);
        torqueMin = Mathf.Max(0f, torqueMin);
        torqueMax = Mathf.Max(torqueMin, torqueMax);
        spreadRadius = Mathf.Max(0f, spreadRadius);
        velocityThreshold = Mathf.Max(0.001f, velocityThreshold);
        angularThreshold = Mathf.Max(0.001f, angularThreshold);
        maxWaitTime = Mathf.Max(0.1f, maxWaitTime);
        snapDuration = Mathf.Max(0.01f, snapDuration);
        forcedValue = Mathf.Clamp(forcedValue, 1, 6);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Roll()
    {
        if (IsRolling) return;
        StartCoroutine(RollSequence());
    }

    public void SetDebugForce(bool on, int val)
    {
        Debug.Log($"DiceRoller: SetDebugForce({on}, {val})");
        forceResult  = on;
        forcedValue  = Mathf.Clamp(val, 1, 6);
    }

    // ── Sequence ──────────────────────────────────────────────────────────────

    private IEnumerator RollSequence()
    {
        IsRolling = true;
        GameEventBus.RollStarted();

        // 1. Teleport above table, random rotation
        _rb.isKinematic = true;
        float sx = Random.Range(-spreadRadius, spreadRadius);
        float sz = Random.Range(-spreadRadius, spreadRadius);
        transform.position = _restPos + new Vector3(sx, launchHeight, sz);
        transform.rotation = Random.rotation;

        // 2. Release with impulse
        _rb.isKinematic     = false;
        _rb.linearVelocity  = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        Vector3 dir = (Vector3.down + new Vector3(Random.Range(-.25f,.25f), 0, Random.Range(-.25f,.25f))).normalized;
        _rb.AddForce(dir * launchForce, ForceMode.VelocityChange);
        _rb.AddTorque(Random.insideUnitSphere.normalized * Random.Range(torqueMin, torqueMax), ForceMode.VelocityChange);

        // 3. Wait until settled
        yield return InitialRollDelay;   // let it start moving first
        float timer = 0f;
        while (timer < maxWaitTime)
        {
            if (_rb.linearVelocity.magnitude < velocityThreshold &&
                _rb.angularVelocity.magnitude < angularThreshold)
                break;
            timer += Time.fixedDeltaTime;
            yield return FixedUpdateYield;
        }

        // 4. Freeze & read face
        _rb.isKinematic     = true;
        _rb.linearVelocity  = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        int face = forceResult ? forcedValue : ReadFace();

        // 5. Snap to clean orientation
        yield return SnapToFace(face);

        // 6. Publish result
        IsRolling = false;
        GameEventBus.RollCompleted(face);
    }

    // ── Face reading ──────────────────────────────────────────────────────────

    // Standard die: +Y=1  -Y=6  +X=2  -X=5  +Z=3  -Z=4
    private int ReadFace()
    {
        float upDot = Vector3.Dot(transform.up, Vector3.up);
        float downDot = Vector3.Dot(-transform.up, Vector3.up);
        float rightDot = Vector3.Dot(transform.right, Vector3.up);
        float leftDot = Vector3.Dot(-transform.right, Vector3.up);
        float forwardDot = Vector3.Dot(transform.forward, Vector3.up);
        float backDot = Vector3.Dot(-transform.forward, Vector3.up);

        float best = upDot;
        int result = 1;

        if (downDot > best) { best = downDot; result = 6; }
        if (rightDot > best) { best = rightDot; result = 2; }
        if (leftDot > best) { best = leftDot; result = 5; }
        if (forwardDot > best) { best = forwardDot; result = 3; }
        if (backDot > best) { result = 4; }

        return result;
    }

    private System.Collections.IEnumerator SnapToFace(int face)
    {
        Quaternion from = transform.rotation;
        Quaternion to   = FaceRot(face);
        float e = 0f;
        while (e < snapDuration)
        {
            e += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0,1, e/snapDuration));
            yield return null;
        }
        transform.rotation = to;
        transform.position = new Vector3(_restPos.x, transform.position.y, _restPos.z);
    }

    private static Quaternion FaceRot(int f) => f switch {
        1 => Quaternion.Euler(  0, 0,   0),
        6 => Quaternion.Euler(180, 0,   0),
        2 => Quaternion.Euler(  0, 0, 90),
        5 => Quaternion.Euler(  0, 0,  -90),
        3 => Quaternion.Euler( 270, 0,   0),
        4 => Quaternion.Euler(90, 0,   0),
        _ => Quaternion.identity,
    };

    // ── Physics setup ─────────────────────────────────────────────────────────

    private void SetupPhysics()
    {
        _rb.mass                   = 0.08f;
        _rb.linearDamping          = 0.05f;
        _rb.angularDamping         = 0.08f;
        _rb.interpolation          = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (s_diceMaterial == null)
        {
            s_diceMaterial = new PhysicsMaterial("Dice") {
                bounciness      = 0.3f,
                dynamicFriction = 0.5f,
                staticFriction  = 0.6f,
                frictionCombine = PhysicsMaterialCombine.Average,
                bounceCombine   = PhysicsMaterialCombine.Maximum,
            };
        }

        _boxCollider.material = s_diceMaterial;
    }
}


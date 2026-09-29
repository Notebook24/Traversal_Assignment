using UnityEngine;
using UnityEngine.InputSystem; // Unity 6 default Input System

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpSpeed = 6f;

    [Header("Falling")]
    [Tooltip("If ticked, the fall limit is worked out automatically from the lowest part of the level.")]
    public bool autoFallLimit = true;
    [Tooltip("Auto mode: how far below the lowest platform counts as 'fell off the map'.")]
    public float fallMargin = 10f;
    [Tooltip("Manual mode: if the player's Y goes below this, the level resets.")]
    public float fallLimitY = -10f;

    Rigidbody rb;
    Collider col;
    Vector3 moveInput;
    bool jumpQueued;

    Vector3 spawnPos;
    Quaternion spawnRot;
    Transform originalParent;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.useGravity = true;                              // GRAVITY
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.sleepThreshold = 0f;                            // keeps OnTriggerStay firing on switches

        spawnPos = transform.position;
        spawnRot = transform.rotation;
        originalParent = transform.parent;
    }

    void Start()
    {
        if (autoFallLimit && LevelManager.Instance != null && LevelManager.Instance.levelRoot != null)
        {
            float lowest = float.MaxValue;
            foreach (var r in LevelManager.Instance.levelRoot.GetComponentsInChildren<Renderer>())
                lowest = Mathf.Min(lowest, r.bounds.min.y);
            if (lowest < float.MaxValue) fallLimitY = lowest - fallMargin;
        }
    }

    void Update()
    {
        var k = Keyboard.current;
        if (k == null) return;

        float x = 0, z = 0;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed)  x -= 1;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1;
        if (k.sKey.isPressed || k.downArrowKey.isPressed)  z -= 1;
        if (k.wKey.isPressed || k.upArrowKey.isPressed)    z += 1;
        // Move relative to the camera so W = away from the camera, D = right on screen
        Transform cam = Camera.main != null ? Camera.main.transform : null;
        if (cam != null)
        {
            Vector3 fwd = cam.forward; fwd.y = 0; fwd.Normalize();
            Vector3 right = cam.right; right.y = 0; right.Normalize();
            moveInput = (fwd * z + right * x).normalized;
        }
        else
        {
            moveInput = new Vector3(x, 0, z).normalized;
        }

        if (k.spaceKey.wasPressedThisFrame) jumpQueued = true;
        if (k.rKey.wasPressedThisFrame) LevelManager.Instance.ResetLevel("Level reset");

        // Fell off the level
        if (transform.position.y < fallLimitY)
            LevelManager.Instance.ResetLevel("You fell! Level reset");
    }

    void FixedUpdate()
    {
        Vector3 v = rb.linearVelocity;
        v.x = moveInput.x * moveSpeed;
        v.z = moveInput.z * moveSpeed;

        if (jumpQueued && IsGrounded()) v.y = jumpSpeed;
        jumpQueued = false;

        rb.linearVelocity = v;
    }

    bool IsGrounded()
    {
        float dist = col.bounds.extents.y + 0.1f;
        return Physics.Raycast(col.bounds.center, Vector3.down, dist,
                               ~0, QueryTriggerInteraction.Ignore);
    }

    // Ride sliding platforms by parenting to them
    PlatformMover GetCarrier(Collision c)
    {
        var m = c.collider.GetComponentInParent<PlatformMover>();
        return (m != null && m.motion == PlatformMover.Motion.Slide) ? m : null;
    }

    void OnCollisionEnter(Collision c)
    {
        var m = GetCarrier(c);
        if (m != null && c.contactCount > 0 && c.GetContact(0).normal.y > 0.5f)
            transform.SetParent(m.transform);
    }

    void OnCollisionExit(Collision c)
    {
        var m = GetCarrier(c);
        if (m != null && transform.parent == m.transform)
            transform.SetParent(originalParent);
    }

    public void Respawn()
    {
        transform.SetParent(originalParent);
        transform.SetPositionAndRotation(spawnPos, spawnRot);
        rb.position = spawnPos;
        rb.rotation = spawnRot;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Physics.SyncTransforms(); // update collider bounds right now, not next physics step
    }
}
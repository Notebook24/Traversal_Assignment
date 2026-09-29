using UnityEngine;

// Put this on every platform that moves. A switch turns it on/off.
public class PlatformMover : MonoBehaviour
{
    public enum Motion
    {
        Slide,  // moves back and forth (left/right or up/down)
        Swing,  // tilts back and forth around an axis (X or Z rotation)
        Spin    // rotates continuously around an axis
    }

    public Motion motion = Motion.Slide;

    [Tooltip("Seconds to travel one way (2 = 2 sec right, 2 sec left, ...)")]
    public float secondsPerWay = 2f;

    [Header("Slide")]
    [Tooltip("Slide direction, e.g. (1,0,0) left-right, (0,1,0) up-down")]
    public Vector3 direction = Vector3.right;
    public float distance = 4f;

    [Header("Swing / Spin")]
    [Tooltip("Rotation axis: X=(1,0,0)  Y=(0,1,0)  Z=(0,0,1)")]
    public Vector3 axis = Vector3.right;
    [Tooltip("Swing: tilts from -angle to +angle")]
    public float swingAngle = 30f;
    [Tooltip("Spin: degrees per second")]
    public float spinSpeed = 45f;

    public bool startActive = false;

    Vector3 startPos;
    Quaternion startRot;
    float t;
    bool active;

    void Awake()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
        active = startActive;
    }

    void Update()
    {
        if (!active) return;
        t += Time.deltaTime;

        // 0 -> 1 -> 0 -> 1 ... one way takes secondsPerWay seconds
        float p = Mathf.PingPong(t / secondsPerWay, 1f);

        switch (motion)
        {
            case Motion.Slide:
                transform.localPosition = startPos + direction.normalized * distance * p;
                break;

            case Motion.Swing:
                float a = Mathf.Lerp(-swingAngle, swingAngle, p);
                transform.localRotation = startRot * Quaternion.AngleAxis(a, axis.normalized);
                break;

            case Motion.Spin:
                transform.Rotate(axis.normalized, spinSpeed * Time.deltaTime, Space.Self);
                break;
        }
    }

    public bool IsActive => active;
    public void SetActive(bool value) => active = value;

    // keepActive = true: platform keeps its on/off state (used when the player falls)
    // keepActive = false: back to the original state (full reset)
    public void ResetState(bool keepActive = true)
    {
        t = 0;
        if (!keepActive) active = startActive;
        transform.localPosition = startPos;
        transform.localRotation = startRot;
    }
}
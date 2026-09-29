using UnityEngine;

// Put this on the end platform's Cube (the one with the Mesh Renderer).
// While the player is on/near the platform, the victory screen is shown.
public class GoalZone : MonoBehaviour
{
    [Tooltip("How far above the platform still counts as 'on it' (in units)")]
    public float extraHeight = 3f;

    Renderer rend;
    Collider playerCol;

    void Start()
    {
        rend = GetComponent<Renderer>();
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null) playerCol = player.GetComponent<Collider>();

        if (rend == null) Debug.LogError("GoalZone: no Renderer on this object. Put GoalZone on the end platform's Cube.");
        if (playerCol == null) Debug.LogError("GoalZone: player not found.");
    }

    void Update()
    {
        if (rend == null || playerCol == null) return;

        Bounds zone = rend.bounds;
        zone.Expand(new Vector3(0.5f, extraHeight, 0.5f));

        LevelManager.Instance.SetOnGoal(zone.Intersects(playerCol.bounds));
    }
}
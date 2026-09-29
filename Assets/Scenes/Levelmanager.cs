using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Tooltip("The 'Level' object that contains all platforms and switches.")]
    public Transform levelRoot;
    public PlayerController player;

    struct Saved { public Transform t; public Vector3 pos; public Quaternion rot; }
    readonly List<Saved> saved = new List<Saved>();

    bool onGoal;              // true while the player is on the end platform
    float ignoreGoalUntil;    // brief pause after a reset so stale physics data can't re-trigger it
    string message = "";
    float messageUntil;

    void Awake()
    {
        Instance = this;

        // Remember the starting position/rotation of everything in the level
        foreach (var t in levelRoot.GetComponentsInChildren<Transform>())
        {
            if (player != null && t.IsChildOf(player.transform)) continue; // player handled separately
            saved.Add(new Saved { t = t, pos = t.localPosition, rot = t.localRotation });
        }
    }

    public void ResetLevel(string msg = "Level reset", bool fullReset = false)
    {
        bool wasOnGoal = onGoal;
        if (wasOnGoal) fullReset = true; // restarting from the victory screen = fresh start

        player.Respawn();

        foreach (var s in saved)
            s.t.SetLocalPositionAndRotation(s.pos, s.rot);

        foreach (var mp in FindObjectsByType<PlatformMover>(FindObjectsSortMode.None))
            mp.ResetState(!fullReset);

        onGoal = false;
        ignoreGoalUntil = Time.time + 0.5f;

        if (wasOnGoal)
        {
            message = "";
            messageUntil = 0f;
        }
        else
        {
            ShowMessage(msg, 2f);
        }
    }

    // Called every frame by GoalZone: true while the player is on the end platform
    public void SetOnGoal(bool value)
    {
        if (Time.time < ignoreGoalUntil) value = false;
        onGoal = value;
    }

    public void ShowMessage(string msg, float seconds)
    {
        message = msg;
        messageUntil = Time.time + seconds;
    }

    // Simple on-screen notifications (no UI Canvas setup needed)
    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.white;

        if (onGoal)
        {
            style.fontSize = 56;
            GUI.Label(new Rect(0, Screen.height * 0.25f, Screen.width, 100), "LEVEL CLEARED!", style);
            style.fontSize = 28;
            GUI.Label(new Rect(0, Screen.height * 0.25f + 100, Screen.width, 50), "Press R to restart", style);
        }
        else if (Time.time < messageUntil)
        {
            style.fontSize = 32;
            GUI.Label(new Rect(0, Screen.height * 0.1f, Screen.width, 60), message, style);
        }
    }
}
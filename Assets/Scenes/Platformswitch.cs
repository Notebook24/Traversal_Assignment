using System.Globalization;
using UnityEngine;

// Step on the switch -> turns movement of the target platforms on / off
public class PlatformSwitch : MonoBehaviour
{
    public enum SwitchAction { Activate, Deactivate, Toggle }

    public SwitchAction action = SwitchAction.Activate;
    public PlatformMover[] targets;

    [Tooltip("Message shown when stepped on. Leave empty to build it from the switch name (X_rotate_activate -> Activate X-Rotate)")]
    public string customMessage = "";

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null) return;

        if (LevelManager.Instance != null)
            LevelManager.Instance.ShowMessage(GetMessage(), 2f);

        foreach (var m in targets)
        {
            if (m == null) continue;
            if (action == SwitchAction.Activate) m.SetActive(true);
            else if (action == SwitchAction.Deactivate) m.SetActive(false);
            else m.SetActive(!m.IsActive);
        }
    }

    string GetMessage()
    {
        if (!string.IsNullOrEmpty(customMessage)) return customMessage;

        // "X_rotate_activate" -> "Activate X-Rotate"
        string[] parts = name.Split('_');
        if (parts.Length < 2) return name;

        var ti = CultureInfo.InvariantCulture.TextInfo;
        string verb = ti.ToTitleCase(parts[parts.Length - 1]);
        string rest = ti.ToTitleCase(string.Join("-", parts, 0, parts.Length - 1));
        return verb + " " + rest;
    }
}
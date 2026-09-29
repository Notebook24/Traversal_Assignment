using System.Collections.Generic;
using UnityEngine;

// Put on the Start platform's Cube and the End platform's Cube (objects with a Mesh Renderer).
// Builds invisible walls around the edges so the player can't fall off.
// One side has a narrow GAP so the player can still walk to / from the other platforms.
public class PlatformBarrier : MonoBehaviour
{
    public enum OpenSide { None, PlusX, MinusX, PlusZ, MinusZ }

    [Tooltip("Side (in WORLD directions) that has the gap. 'None' = fully walled in.")]
    public OpenSide openSide = OpenSide.MinusZ;
    [Tooltip("Width of the gap. 0 = the whole side is open.")]
    public float gapWidth = 4f;
    [Tooltip("Slide the gap along the edge (world units; + is toward +X or +Z).")]
    public float gapOffset = 0f;

    public float wallThickness = 0.5f;
    public float wallHeight = 3f;

    Renderer rend;

    struct Wall { public Vector3 center; public Vector3 size; }

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("PlatformBarrier: no Renderer on " + name + ". Put it on the platform's Cube.");
            return;
        }

        var root = new GameObject("Barrier_" + transform.parent.name);  // scene root: platform scale can't distort it
        foreach (var w in GetWalls(rend.bounds))
        {
            var g = new GameObject("Wall");
            g.transform.SetParent(root.transform, false);
            g.transform.position = w.center;
            g.AddComponent<BoxCollider>().size = w.size;
        }
    }

    List<Wall> GetWalls(Bounds b)
    {
        float t = wallThickness, h = wallHeight;
        float y = b.max.y + h / 2f;
        var list = new List<Wall>();

        // Walls on the +X / -X sides run along Z (extended by t so corners are closed)
        AddSide(list, true,  b.max.x + t / 2f, b.min.z - t, b.max.z + t, b.center.z, y, openSide == OpenSide.PlusX);
        AddSide(list, true,  b.min.x - t / 2f, b.min.z - t, b.max.z + t, b.center.z, y, openSide == OpenSide.MinusX);
        // Walls on the +Z / -Z sides run along X
        AddSide(list, false, b.max.z + t / 2f, b.min.x, b.max.x, b.center.x, y, openSide == OpenSide.PlusZ);
        AddSide(list, false, b.min.z - t / 2f, b.min.x, b.max.x, b.center.x, y, openSide == OpenSide.MinusZ);
        return list;
    }

    // alongZ = true: wall at x = fixed, spanning z from lineMin..lineMax
    // alongZ = false: wall at z = fixed, spanning x from lineMin..lineMax
    void AddSide(List<Wall> list, bool alongZ, float fixedCoord, float lineMin, float lineMax,
                 float lineCenter, float y, bool hasGap)
    {
        if (!hasGap)
        {
            AddSegment(list, alongZ, fixedCoord, lineMin, lineMax, y);
            return;
        }
        if (gapWidth <= 0f) return; // whole side open

        float gapMin = lineCenter + gapOffset - gapWidth / 2f;
        float gapMax = gapMin + gapWidth;
        AddSegment(list, alongZ, fixedCoord, lineMin, Mathf.Min(gapMin, lineMax), y);
        AddSegment(list, alongZ, fixedCoord, Mathf.Max(gapMax, lineMin), lineMax, y);
    }

    void AddSegment(List<Wall> list, bool alongZ, float fixedCoord, float a, float b, float y)
    {
        float len = b - a;
        if (len < 0.01f) return;
        float mid = (a + b) / 2f;
        float t = wallThickness, h = wallHeight;

        if (alongZ)
            list.Add(new Wall { center = new Vector3(fixedCoord, y, mid), size = new Vector3(t, h, len) });
        else
            list.Add(new Wall { center = new Vector3(mid, y, fixedCoord), size = new Vector3(len, h, t) });
    }

    // Select the platform's Cube in the editor to preview the walls (green boxes)
    void OnDrawGizmosSelected()
    {
        var r = GetComponent<Renderer>();
        if (r == null) return;
        Gizmos.color = Color.green;
        foreach (var w in GetWalls(r.bounds))
            Gizmos.DrawWireCube(w.center, w.size);
    }
}
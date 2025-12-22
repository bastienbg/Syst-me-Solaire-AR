using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlanetInfoEntry
{
    public string planetId;         // "Earth", "Mars", ...
    public string title;            // "Terre"
    [TextArea(3, 10)] public string description;
}

public class PlanetInfoDatabase : MonoBehaviour
{
    public List<PlanetInfoEntry> entries = new List<PlanetInfoEntry>();

    private Dictionary<string, PlanetInfoEntry> map;

    private void Awake()
    {
        map = new Dictionary<string, PlanetInfoEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in entries)
        {
            if (e == null || string.IsNullOrEmpty(e.planetId)) continue;
            map[e.planetId] = e;
        }
    }

    public bool TryGet(string planetId, out PlanetInfoEntry entry)
    {
        entry = null;
        if (string.IsNullOrEmpty(planetId) || map == null) return false;
        return map.TryGetValue(planetId, out entry);
    }
}

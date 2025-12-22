using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(ARMarkerStatus))]
public class SolarSystemCenter : MonoBehaviour
{
    public static SolarSystemCenter Instance { get; private set; }

    [Tooltip("Modèle 3D du Soleil (enfant de l'ImageTarget).")]
    public Transform sunModel;

    private ARMarkerStatus markerStatus;

    public bool IsActive => markerStatus != null && markerStatus.IsTracked;

    public Transform CenterTransform => sunModel != null ? sunModel : transform;

    //  Liste de toutes les planètes instanciées
    private readonly List<GameObject> orbitingPlanets = new List<GameObject>();

    private readonly Dictionary<string, GameObject> namedPlanets = new Dictionary<string, GameObject>();

    private int planetCount = 0;
    public int PlanetCount => planetCount;

    // pour détecter le changement de state (visible / pas visible)
    private bool lastActiveState = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        markerStatus = GetComponent<ARMarkerStatus>();
    }

    private void Update()
    {
        bool currentActive = IsActive;

        // si l'état a changé depuis la dernière frame → on met à jour les planètes
        if (currentActive != lastActiveState)
        {
            lastActiveState = currentActive;
            SetPlanetsActive(currentActive);
        }

        // petit nettoyage des références null si certaines planètes sont détruites
        for (int i = orbitingPlanets.Count - 1; i >= 0; i--)
        {
            if (orbitingPlanets[i] == null)
                orbitingPlanets.RemoveAt(i);
        }
        //var names = orbitingPlanets
        // .Select(p => p != null ? p.name : "null")
        // .ToArray();

        //Debug.Log("Planètes en orbite : " + string.Join(", ", names));
    }

    /// <summary>
    /// Instancie une planète et l'enregistre comme "en orbite".
    /// </summary>
    public Transform SpawnPlanet(GameObject planetPrefab, float orbitRadius, float orbitSpeedDegPerSec)
    {
        if (!IsActive || planetPrefab == null)
            return null;

        float angleDeg = Random.Range(0f, 360f);
        float angleRad = angleDeg * Mathf.Deg2Rad;

        Vector3 centerPos = CenterTransform.position;
        Vector3 spawnPos = centerPos + new Vector3(
            Mathf.Cos(angleRad) * orbitRadius,
            0f,
            Mathf.Sin(angleRad) * orbitRadius
        );

        GameObject instance = Instantiate(planetPrefab, spawnPos, Quaternion.identity);

        PlanetOrbit orbit = instance.GetComponent<PlanetOrbit>();
        if (orbit == null)
            orbit = instance.AddComponent<PlanetOrbit>();

        orbit.InitializeOrbit(CenterTransform, orbitRadius, angleRad, orbitSpeedDegPerSec);

        orbitingPlanets.Add(instance);
        planetCount++;

        return instance.transform;
    }

    /// <summary>
    /// Active / désactive toutes les planètes instanciées quand le Soleil apparaît / disparaît.
    /// </summary>
    private void SetPlanetsActive(bool active)
    {
        foreach (var planet in orbitingPlanets)
        {
            if (planet == null) continue;
            planet.SetActive(active);
        }
    }

    public void RegisterNamedPlanet(string id, GameObject planet)
    {
        if (string.IsNullOrEmpty(id) || planet == null) return;

        namedPlanets[id] = planet;
    }

    public GameObject GetNamedPlanet(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        namedPlanets.TryGetValue(id, out var planet);
        return planet;
    }

    public void UnregisterPlanet(GameObject planet)
    {
        if (planet == null) return;

        orbitingPlanets.Remove(planet);

        // On supprime aussi les entrées du dictionnaire qui pointent vers cette planète
        List<string> keysToRemove = null;
        foreach (var kvp in namedPlanets)
        {
            if (kvp.Value == planet)
            {
                keysToRemove ??= new List<string>();
                keysToRemove.Add(kvp.Key);
            }
        }

        if (keysToRemove != null)
        {
            foreach (var key in keysToRemove)
                namedPlanets.Remove(key);
        }

        planetCount = Mathf.Max(0, planetCount - 1);
    }


}

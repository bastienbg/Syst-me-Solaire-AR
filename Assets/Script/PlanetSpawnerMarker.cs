using UnityEngine;
using Vuforia;

[RequireComponent(typeof(ObserverBehaviour))]
public class PlanetSpawnerMarker : MonoBehaviour
{
    [Tooltip("Prefab de la planète à instancier en orbite autour du Soleil.")]
    public GameObject planetPrefab;

    [Tooltip("Rayon de l'orbite autour du Soleil (mètres).")]
    public float orbitRadius = 0.25f;

    [Tooltip("Vitesse orbitale (degrés par seconde).")]
    public float orbitSpeedDegPerSec = 20f;

    [Tooltip("Ne spawn qu'une seule fois pour ce marqueur.")]
    public bool spawnOnlyOnce = true;

    private ObserverBehaviour observer;
    private bool wasTracked = false;
    private bool hasSpawned = false;

    [Header("ID de la planète dans le système solaire")]
    [Tooltip("Ex: Earth, Mars, Jupiter... utilisé par PlanetProximityLink pour cibler la bonne planète.")]
    public string planetId;

    private void Awake()
    {
        observer = GetComponent<ObserverBehaviour>();
    }

    private void Update()
    {
        if (observer == null || SolarSystemCenter.Instance == null)
            return;

        var status = observer.TargetStatus.Status;
        bool isTracked = status == Status.TRACKED || status == Status.EXTENDED_TRACKED;

        if (isTracked && !wasTracked)
        {
            OnMarkerJustAppeared();
        }

        wasTracked = isTracked;
    }

    private void OnMarkerJustAppeared()
    {
        if (spawnOnlyOnce && hasSpawned)
            return;

        if (!SolarSystemCenter.Instance.IsActive)
        {
            Debug.Log("[PlanetSpawnerMarker] Soleil non présent, impossible de créer la planète.");
            return;
        }

        if (planetPrefab == null)
        {
            Debug.LogWarning("[PlanetSpawnerMarker] Aucun prefab de planète assigné.");
            return;
        }

        Transform planet = SolarSystemCenter.Instance.SpawnPlanet(
            planetPrefab,
            orbitRadius,
            orbitSpeedDegPerSec
        );

        if (planet != null && !string.IsNullOrEmpty(planetId))
        {
            SolarSystemCenter.Instance.RegisterNamedPlanet(planetId, planet.gameObject);
        }


        if (planet != null)
        {
            hasSpawned = true;
        }

        if (ARGameManager.Instance != null && !string.IsNullOrEmpty(planetId))
            ARGameManager.Instance.RegisterSpawn(planetId);

    }
}

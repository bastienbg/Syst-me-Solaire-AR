using UnityEngine;
using TMPro;

public class PlanetProximityLink : MonoBehaviour
{
    [Header("Références planètes (ImageTargets ou modèles)")]
    public GameObject planetA;
    public GameObject planetB;

    [Header("Canvas pour le texte de distance")]
    public Canvas canvas;
    public Material lineMaterial;

    [Header("Explosion")]
    [Tooltip("Distance (m) en dessous de laquelle les planètes explosent")]
    public float explosionDistance = 0.07f;
    [Tooltip("Prefab d'explosion")]
    public GameObject explosionPrefab;
    [Tooltip("Temps avant de réafficher les planètes après l'explosion")]
    public float respawnDelay = 2f;

    [Tooltip("Décalage vertical de l'explosion")]
    public float explosionHeightOffset = 0.1f;

    [Header("Système solaire")]
    [Tooltip("ID de la planète à supprimer du système solaire (ex: Earth, Mars). Doit correspondre au planetId du spawner.")]
    public string planetIdToRemove;

    private ARMarkerStatus statusA;
    private ARMarkerStatus statusB;

    private GameObject lineObject;
    private LineRenderer line;
    private TextMeshProUGUI distanceText;

    private Camera mainCam;

    private bool hasExploded = false;
    private float respawnTimer = 0f;

    // Empêche de re-exploser tant qu'on ne s'est pas assez éloigné
    private bool canExplode = true;
    [Tooltip("Facteur d'hystérésis pour ré-autoriser une explosion (ex: 1.3 = 30% plus loin).")]
    public float rearmFactor = 1.3f;

    [Header("Audio")]
    [Tooltip("Audio source")]
    public AudioSource explosionAudioSource;

    private void Start()
    {
        if (planetA == null || planetB == null)
        {
            Debug.LogWarning("[PlanetProximityLink] planetA ou planetB n'est pas assigné.");
            enabled = false;
            return;
        }

        statusA = planetA.GetComponent<ARMarkerStatus>();
        statusB = planetB.GetComponent<ARMarkerStatus>();

        if (statusA == null || statusB == null)
        {
            Debug.LogWarning("[PlanetProximityLink] Les planètes doivent avoir un ARMarkerStatus.");
            enabled = false;
            return;
        }

        mainCam = Camera.main;
    }

    private void Update()
    {
        // Gestion du timer de respawn après explosion (pour les MARKERS)
        if (hasExploded)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f)
            {
                ResetAfterExplosion();
            }
            return;
        }

        // Si un des deux markers n'est pas suivi → on cache les visuels, on réarme l'explosion
        if (!statusA.IsTracked || !statusB.IsTracked)
        {
            SetVisualsActive(false);
            canExplode = true;
            return;
        }

        Vector3 posA = planetA.transform.position;
        Vector3 posB = planetB.transform.position;
        float dist = Vector3.Distance(posA, posB);

        // Explosion : seulement si autorisée et suffisamment proche
        if (canExplode && dist < explosionDistance)
        {
            TriggerExplosion(posA, posB);
            return;
        }

        // Si on s'est suffisamment éloigné, on ré-autorise une future explosion
        if (!canExplode && dist > explosionDistance * rearmFactor)
        {
            canExplode = true;
        }

        // Comportement normal : ligne + distance
        SetVisualsActive(true);
        UpdateLine(posA, posB);
        UpdateDistanceText(posA, posB, dist);
    }

    private void SetVisualsActive(bool active)
    {
        if (lineObject != null)
            lineObject.SetActive(active);

        if (distanceText != null)
            distanceText.gameObject.SetActive(active);
    }

    private void UpdateLine(Vector3 posA, Vector3 posB)
    {
        if (lineObject == null)
        {
            lineObject = new GameObject("PlanetLinkLine");
            lineObject.transform.SetParent(transform, false);

            line = lineObject.AddComponent<LineRenderer>();
            line.widthMultiplier = 0.005f;
            line.useWorldSpace = true;

            if (lineMaterial != null)
                line.material = lineMaterial;
        }

        if (line == null) return;

        line.positionCount = 2;
        line.SetPosition(0, posA);
        line.SetPosition(1, posB);
    }

    private void UpdateDistanceText(Vector3 posA, Vector3 posB, float dist)
    {
        if (canvas == null || mainCam == null)
            return;

        if (distanceText == null)
        {
            GameObject textGO = new GameObject("DistanceText");
            textGO.transform.SetParent(canvas.transform, false);
            distanceText = textGO.AddComponent<TextMeshProUGUI>();
            distanceText.fontSize = 24;
            distanceText.alignment = TextAlignmentOptions.Center;
        }

        float distCm = dist * 100f;
        distanceText.text = distCm.ToString("F1") + " cm";

        Vector3 middle = (posA + posB) * 0.5f;
        Vector3 screenPos = mainCam.WorldToScreenPoint(middle);
        distanceText.transform.position = screenPos;
    }

    private void TriggerExplosion(Vector3 posA, Vector3 posB)
    {
        hasExploded = true;
        canExplode = false;
        respawnTimer = respawnDelay;

        // On cache ligne + texte + markers pendant un court instant
        SetVisualsActive(false);
        planetA.SetActive(false);
        planetB.SetActive(false);

        // Explosion entre les deux MARKERS (Terre Card & Asteroid Card)
        Vector3 middle = (posA + posB) * 0.5f;
        Vector3 spawnPos = middle + new Vector3(0f, explosionHeightOffset, 0f);

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, spawnPos, Quaternion.identity);
            if (explosionAudioSource != null)
                explosionAudioSource.Play();

            Destroy(explosion, 3f);
        }
        else
        {
            Debug.LogWarning("[PlanetProximityLink] Pas de prefab d'explosion assigné.");
        }

        // 💥 Explosion à la position de la planète en orbite + suppression du système solaire
        if (SolarSystemCenter.Instance != null && !string.IsNullOrEmpty(planetIdToRemove))
        {
            GameObject planetInOrbit = SolarSystemCenter.Instance.GetNamedPlanet(planetIdToRemove);
            if (planetInOrbit != null)
            {
                Vector3 planetPos = planetInOrbit.transform.position + new Vector3(0f, explosionHeightOffset, 0f);

                if (explosionPrefab != null)
                {
                    GameObject planetExplosion = Instantiate(explosionPrefab, planetPos, Quaternion.identity);
                    if (explosionAudioSource != null)
                        explosionAudioSource.Play();

                    Destroy(planetExplosion, 3f);
                }

                if (ARGameManager.Instance != null && !string.IsNullOrEmpty(planetIdToRemove))
                    ARGameManager.Instance.RegisterDestroy(planetIdToRemove);

                SolarSystemCenter.Instance.UnregisterPlanet(planetInOrbit);
                Destroy(planetInOrbit);
            }
        }
    }

    private void ResetAfterExplosion()
    {
        hasExploded = false;

        // On réactive seulement les MARKERS (Terre / Astéroïde)
        planetA.SetActive(true);
        planetB.SetActive(true);
    }
}

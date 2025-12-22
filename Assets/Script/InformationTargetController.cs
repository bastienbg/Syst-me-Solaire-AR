using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ARMarkerStatus))]
public class InformationTargetController : MonoBehaviour
{
    [Header("Références")]
    public PlanetInfoDatabase database;
    public PlanetInfoUI ui;

    [Tooltip("Liste des ImageTargets de planètes (ceux qui ont PlanetSpawnerMarker).")]
    public List<PlanetSpawnerMarker> planetCards = new List<PlanetSpawnerMarker>();

    [Header("Comportement")]
    [Tooltip("Si plusieurs cartes planètes sont visibles, on prend la plus proche du marqueur Info.")]
    public bool pickClosestVisiblePlanet = true;

    private ARMarkerStatus infoStatus;

    private string lastScoredPlanetId = null;


    private void Awake()
    {
        infoStatus = GetComponent<ARMarkerStatus>();
    }

    private void Update()
    {
        if (database == null || ui == null)
            return;

        if (infoStatus == null || !infoStatus.IsTracked)
        {
            ui.Hide();
            lastScoredPlanetId = null;
            return;
        }

        string planetId = FindTrackedPlanetId();

        if (string.IsNullOrEmpty(planetId))
        {
            ui.Show("Information", "Montre une carte planète en même temps que la target Information.");
            return;
        }

        if (database.TryGet(planetId, out var entry))
        {
            ui.Show(entry.title, entry.description);
        }
        else
        {
            ui.Show("Information", $"Aucune fiche trouvée pour l'ID : {planetId}");
        }
        if (ARGameManager.Instance != null && lastScoredPlanetId != planetId)
        {
            ARGameManager.Instance.RegisterInfo(planetId);
            lastScoredPlanetId = planetId;
        }

    }

    private string FindTrackedPlanetId()
    {
        PlanetSpawnerMarker best = null;
        float bestDist = float.MaxValue;

        foreach (var card in planetCards)
        {
            if (card == null) continue;

            var status = card.GetComponent<ARMarkerStatus>();
            if (status == null || !status.IsTracked) continue;

            if (!pickClosestVisiblePlanet)
                return card.planetId;

            float d = Vector3.Distance(transform.position, card.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = card;
            }
        }

        return best != null ? best.planetId : null;
    }
}

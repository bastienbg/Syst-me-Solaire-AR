using UnityEngine;

/// <summary>
/// Contrôle simple pour une planète : 
/// - rotation sur elle-même quand le marqueur est suivi.
/// </summary>
[RequireComponent(typeof(ARMarkerStatus))]
public class PlanetController : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Le modèle 3D de la planète (enfant de l'ImageTarget).")]
    public Transform planetModel;

    [Header("Paramètres")]
    [Tooltip("Vitesse de rotation en degrés par seconde.")]
    public float selfRotationSpeed = 15f;

    private ARMarkerStatus markerStatus;

    private void Awake()
    {
        markerStatus = GetComponent<ARMarkerStatus>();
    }

    private void Update()
    {
        if (markerStatus == null || !markerStatus.IsTracked)
            return;

        if (planetModel != null)
        {
            // rotation simple autour de l'axe Y local
            planetModel.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}

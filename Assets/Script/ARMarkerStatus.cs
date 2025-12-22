using UnityEngine;
using Vuforia;

/// <summary>
/// Brique de base : donne un booléen IsTracked pour un marqueur Vuforia.
/// À ajouter sur chaque ImageTarget.
/// </summary>
[RequireComponent(typeof(ObserverBehaviour))]
public class ARMarkerStatus : MonoBehaviour
{
    private ObserverBehaviour observer;

    /// <summary>
    /// Vrai si le marqueur est actuellement suivi par Vuforia.
    /// </summary>
    public bool IsTracked { get; private set; }

    private void Awake()
    {
        observer = GetComponent<ObserverBehaviour>();
    }

    private void Update()
    {
        if (observer == null)
        {
            IsTracked = false;
            return;
        }

        var status = observer.TargetStatus.Status;

        IsTracked =
            status == Status.TRACKED ||
            status == Status.EXTENDED_TRACKED;
    }
}

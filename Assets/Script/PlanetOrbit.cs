using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{
    [Tooltip("Centre de l'orbite (souvent le Soleil).")]
    public Transform orbitCenter;

    [Tooltip("Rayon de l'orbite (mètres en AR).")]
    public float orbitRadius = 0.2f;

    [Tooltip("Vitesse de rotation autour du centre (degrés par seconde).")]
    public float orbitSpeedDegPerSec = 15f;

    [Tooltip("Vitesse de rotation sur elle-même (degrés par seconde).")]
    public float selfRotationSpeedDegPerSec = 30f;

    private float currentAngleRad;
    private bool initialized;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void InitializeOrbit(Transform center, float radius, float startAngleRad, float speedDegPerSec)
    {
        orbitCenter = center;
        orbitRadius = radius;
        currentAngleRad = startAngleRad;
        orbitSpeedDegPerSec = speedDegPerSec;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || orbitCenter == null)
            return;

        float orbitSpeedRadPerSec = orbitSpeedDegPerSec * Mathf.Deg2Rad;
        currentAngleRad += orbitSpeedRadPerSec * Time.deltaTime;

        Vector3 centerPos = orbitCenter.position;
        Vector3 newPos = centerPos + new Vector3(
            Mathf.Cos(currentAngleRad) * orbitRadius,
            0f,
            Mathf.Sin(currentAngleRad) * orbitRadius
        );

        transform.position = newPos;

        if (selfRotationSpeedDegPerSec != 0f)
        {
            transform.Rotate(Vector3.up, selfRotationSpeedDegPerSec * Time.deltaTime, Space.Self);
        }

        // hook pour l'Animator si tu veux un état "IsOrbiting"
        if (animator != null)
        {
            animator.SetBool("IsOrbiting", true);
        }
    }
}

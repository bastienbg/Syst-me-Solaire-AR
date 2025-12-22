using UnityEngine;

[RequireComponent(typeof(ARMarkerStatus))]
public class LevelStarterOnSun : MonoBehaviour
{
    private ARMarkerStatus status;
    private bool startedOnce = false;

    private void Awake()
    {
        status = GetComponent<ARMarkerStatus>();
    }

    private void Update()
    {
        if (startedOnce) return;

        if (status != null && status.IsTracked && ARGameManager.Instance != null)
        {
            startedOnce = true;
            ARGameManager.Instance.StartLevel();
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DetectorTool : MonoBehaviour
{
    public DetectionManager detectionManager;

    private void Start()
    {
        if (detectionManager == null)
            detectionManager = DetectionManager.Instance;
    }

    private void Update()
    {
        if (detectionManager == null) return;
        detectionManager.UpdateDetection(transform.position);
    }
}

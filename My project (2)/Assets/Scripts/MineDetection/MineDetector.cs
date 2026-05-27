using UnityEngine;

public class MineDetector : MonoBehaviour
{
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private DetectionManager detectionManager;
    [SerializeField] private bool createVisibleModel = true;

    private void Awake()
    {
        if (rayOrigin == null)
            rayOrigin = transform;

        if (createVisibleModel)
            EnsureVisibleModel();

        if (detectionManager == null)
            detectionManager = DetectionManager.Instance;

        if (detectionManager == null)
        {
            GameObject managerObject = new GameObject("DetectionManager");
            detectionManager = managerObject.AddComponent<DetectionManager>();
        }
    }

    private void Update()
    {
        if (detectionManager == null || rayOrigin == null)
            return;

        detectionManager.UpdateDetection(rayOrigin.position);
    }

    private void EnsureVisibleModel()
    {
        if (GetComponentInChildren<Renderer>() != null)
            return;

        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        handle.name = "DetectorVisual";
        handle.transform.SetParent(transform, false);
        handle.transform.localPosition = new Vector3(0f, 0f, 0f);
        handle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        handle.transform.localScale = new Vector3(0.08f, 0.08f, 1.5f);

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "DetectorHead";
        head.transform.SetParent(transform, false);
        head.transform.localPosition = new Vector3(1.55f, 0f, 0f);
        head.transform.localScale = new Vector3(0.55f, 0.55f, 0.12f);

        ApplyMaterial(handle, Color.yellow);
        ApplyMaterial(head, Color.black);
    }

    private void ApplyMaterial(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
            return;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        if (shader != null)
            renderer.material = new Material(shader) { color = color };
    }
}

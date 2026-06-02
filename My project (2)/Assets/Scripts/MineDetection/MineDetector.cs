using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MineDetector : MonoBehaviour
{
    private const string DefaultModelPath = "Assets/Models/metal-detector/source/Metal Detector.fbx";

    [SerializeField] private Transform rayOrigin;
    [SerializeField] private DetectionManager detectionManager;
    [SerializeField] private bool createVisibleModel = true;
    [SerializeField] private bool detectOnlyWhileHeld;
    [SerializeField] private GameObject detectorModelPrefab;
    [SerializeField] private Vector3 modelLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 modelLocalRotation = Vector3.zero;
    [SerializeField] private Vector3 modelLocalScale = Vector3.one;

    private void Awake()
    {
        if (rayOrigin == null)
            rayOrigin = transform;

        if (createVisibleModel)
            EnsureVisibleModel();

        EnsureGrabSetup();

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

        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        if (detectOnlyWhileHeld && (grabInteractable == null || !grabInteractable.isSelected))
        {
            detectionManager.StopDetection();
            return;
        }

        detectionManager.UpdateDetection(rayOrigin.position);
    }

    public void EnsureVisibleModel()
    {
        if (transform.Find("DetectorModel") != null && HasVisibleRenderer(transform.Find("DetectorModel")))
            return;

        RemoveVisibleModel();

        GameObject modelPrefab = detectorModelPrefab;
#if UNITY_EDITOR
        if (modelPrefab == null)
            modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultModelPath);
#endif

        if (modelPrefab != null)
        {
            GameObject model = Instantiate(modelPrefab, transform);
            model.name = "DetectorModel";
            model.transform.localPosition = modelLocalPosition;
            model.transform.localRotation = Quaternion.Euler(modelLocalRotation);
            model.transform.localScale = modelLocalScale;
            return;
        }

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

    public void RebuildVisibleModel()
    {
        RemoveVisibleModel();
        EnsureVisibleModel();
        EnsureGrabSetup();
    }

    public void SetDetectorModelPrefab(GameObject prefab)
    {
        detectorModelPrefab = prefab;
    }

    public void SetDetectOnlyWhileHeld(bool value)
    {
        detectOnlyWhileHeld = value;
    }

    public void EnsureGrabSetup()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null)
            body = gameObject.AddComponent<Rigidbody>();

        body.useGravity = false;
        body.isKinematic = true;

        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0f, 0.35f);
            box.size = new Vector3(0.35f, 0.2f, 0.9f);
        }

        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();

        grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grabInteractable.throwOnDetach = false;
        grabInteractable.useDynamicAttach = true;
    }

    private void RemoveVisibleModel()
    {
        DestroyChildIfExists("DetectorModel");
        DestroyChildIfExists("DetectorVisual");
        DestroyChildIfExists("DetectorHead");
    }

    private bool HasVisibleRenderer(Transform root)
    {
        if (root == null)
            return false;

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        return renderers != null && renderers.Length > 0;
    }

    private void DestroyChildIfExists(string childName)
    {
        Transform child = transform.Find(childName);
        if (child == null)
            return;

        if (Application.isPlaying)
            Destroy(child.gameObject);
        else
            DestroyImmediate(child.gameObject);
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

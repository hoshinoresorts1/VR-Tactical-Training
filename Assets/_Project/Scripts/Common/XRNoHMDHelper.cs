using UnityEngine;

[ExecuteAlways]
public class XRNoHMDHelper : MonoBehaviour
{
    [Tooltip("Optional mesh to use for controller visualizers.")]
    public Mesh controllerMesh;
    public Material controllerMaterial;
    public float controllerScale = 0.1f;
    public float cameraHeight = 1.6f;

    private void Awake()
    {
        SetupControllerVisuals();
        AlignCameraHeight();
        EnsureCameraExists();
    }

    private void SetupControllerVisuals()
    {
        if (controllerMaterial == null)
        {
            controllerMaterial = new Material(Shader.Find("Standard"));
            controllerMaterial.color = Color.cyan;
        }

        AddVisualizer("LeftHand", Color.blue);
        AddVisualizer("RightHand", Color.red);
        AddVisualizer("Left", Color.blue);
        AddVisualizer("Right", Color.red);
    }

    private void AddVisualizer(string nameContains, Color color)
    {
        var transforms = GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
        {
            if (!t.name.Contains(nameContains))
                continue;

            if (t.GetComponentInChildren<MeshFilter>() != null)
                continue;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = nameContains + "Visual";
            visual.transform.SetParent(t, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one * controllerScale;

            var meshRenderer = visual.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                var mat = new Material(controllerMaterial);
                mat.color = color;
                meshRenderer.sharedMaterial = mat;
            }

            var collider = visual.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
        }
    }

    private void AlignCameraHeight()
    {
        var cam = Camera.main;
        if (cam == null) return;

        if (cam.transform.localPosition.y < 0.1f || cam.transform.localPosition.y > 5f)
        {
            cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, cameraHeight, cam.transform.localPosition.z);
        }
    }

    private void EnsureCameraExists()
    {
        if (Camera.main != null)
            return;

        var cameraObj = new GameObject("Main Camera");
        var cam = cameraObj.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cameraObj.transform.position = new Vector3(0f, cameraHeight, -2f);
        cameraObj.transform.rotation = Quaternion.identity;
    }
}


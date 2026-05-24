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
    }

    private void SetupControllerVisuals()
    {
        if (controllerMesh == null)
        {
            controllerMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        }

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
            if (t.name.Contains(nameContains) && t.childCount == 0)
            {
                var visual = new GameObject(nameContains + "Visual");
                visual.transform.SetParent(t, false);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one * controllerScale;

                var meshFilter = visual.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = controllerMesh;

                var meshRenderer = visual.AddComponent<MeshRenderer>();
                var mat = new Material(controllerMaterial);
                mat.color = color;
                meshRenderer.sharedMaterial = mat;
                return;
            }
        }
    }

    private void AlignCameraHeight()
    {
        var cam = Camera.main;
        if (cam == null) return;

        var root = cam.transform;
        while (root.parent != null && root.parent.GetComponent<Camera>() == null)
        {
            root = root.parent;
        }

        if (cam.transform.localPosition.y < 0.1f || cam.transform.localPosition.y > 5f)
        {
            cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, cameraHeight, cam.transform.localPosition.z);
        }
    }
}

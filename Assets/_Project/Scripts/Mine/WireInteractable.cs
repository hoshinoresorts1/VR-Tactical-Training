using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WireInteractable : MonoBehaviour, IPointerClickHandler
{
    private const float WireLength = 0.12f;
    private const float WireThickness = 0.00625f;
    private const int CurveSegmentCount = 9;
    private const float CurveAmplitude = 0.9f;

    public int wireIndex = 0;
    public WireDefusal wireDefusal;

    private XRSimpleInteractable xrInteractable;
    private int lastCutFrame = -1;
    private bool isCut;

    private void Awake()
    {
        EnsureSetup();
    }

    private void OnEnable()
    {
        EnsureSetup();

        if (xrInteractable != null)
            xrInteractable.selectEntered.AddListener(OnXRSelectEntered);
    }

    private void OnDisable()
    {
        if (xrInteractable != null)
            xrInteractable.selectEntered.RemoveListener(OnXRSelectEntered);
    }

    // Simple editor/test interaction
    private void OnMouseDown()
    {
        CutWire();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CutWire();
    }

    public void CutWire()
    {
        if (isCut || lastCutFrame == Time.frameCount)
            return;

        lastCutFrame = Time.frameCount;

        if (wireDefusal == null)
            wireDefusal = GetComponentInParent<WireDefusal>();

        if (wireDefusal != null)
            wireDefusal.SelectWire(wireIndex);
    }

    public void ConfigureSurfaceLayout()
    {
        transform.localPosition = new Vector3(0f, 0.035f, (wireIndex - 1) * 0.1f);
        transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        transform.localScale = new Vector3(WireThickness, WireLength, WireThickness);

        CreateCurvedVisual();
    }

    public void ShowCutVisual()
    {
        if (isCut)
            return;

        isCut = true;

        Renderer renderer = GetComponent<Renderer>();
        Material material = renderer != null ? renderer.material : null;
        if (renderer != null)
            renderer.enabled = false;

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        if (xrInteractable != null)
            xrInteractable.enabled = false;

        Transform curvedVisual = transform.Find("CurvedVisual");
        if (curvedVisual != null)
            curvedVisual.gameObject.SetActive(false);

        CreateCutHalf("LeftHalf", -1f, -7f, material);
        CreateCutHalf("RightHalf", 1f, 7f, material);
    }

    private void CreateCutHalf(string halfName, float direction, float angle, Material material)
    {
        GameObject half = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        half.name = name + "_" + halfName;
        half.transform.SetParent(transform.parent, false);

        Vector3 wireDirection = transform.localRotation * Vector3.up;
        half.transform.localPosition = transform.localPosition + wireDirection * direction * 0.075f;
        half.transform.localRotation = transform.localRotation * Quaternion.Euler(0f, 0f, angle);
        half.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 0.42f, transform.localScale.z);

        Renderer halfRenderer = half.GetComponent<Renderer>();
        if (halfRenderer != null && material != null)
            halfRenderer.material = material;

        Collider halfCollider = half.GetComponent<Collider>();
        if (halfCollider != null)
            Destroy(halfCollider);
    }

    private void CreateCurvedVisual()
    {
        Transform existingVisual = transform.Find("CurvedVisual");
        if (existingVisual != null)
            return;

        Renderer renderer = GetComponent<Renderer>();
        Material material = renderer != null ? renderer.material : null;
        if (renderer != null)
            renderer.enabled = false;

        GameObject visualRoot = new GameObject("CurvedVisual");
        visualRoot.transform.SetParent(transform, false);

        Vector3 previousPoint = GetCurvePoint(0);
        for (int i = 1; i <= CurveSegmentCount; i++)
        {
            Vector3 nextPoint = GetCurvePoint(i);
            CreateCurveSegment(visualRoot.transform, previousPoint, nextPoint, material);
            previousPoint = nextPoint;
        }
    }

    private Vector3 GetCurvePoint(int index)
    {
        float t = index / (float)CurveSegmentCount;
        float y = Mathf.Lerp(-1f, 1f, t);
        float z = Mathf.Sin(t * Mathf.PI * 2f) * CurveAmplitude;
        return new Vector3(0f, y, z);
    }

    private void CreateCurveSegment(Transform parent, Vector3 start, Vector3 end, Material material)
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        segment.name = "CurveSegment";
        segment.transform.SetParent(parent, false);

        Vector3 direction = end - start;
        segment.transform.localPosition = (start + end) * 0.5f;
        segment.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
        segment.transform.localScale = new Vector3(1f, direction.magnitude * 0.5f, 1f);

        Renderer renderer = segment.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.material = material;

        Collider collider = segment.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
    }

    private void OnXRSelectEntered(SelectEnterEventArgs args)
    {
        CutWire();
    }

    private void EnsureSetup()
    {
        if (wireDefusal == null)
            wireDefusal = GetComponentInParent<WireDefusal>();

        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();

        xrInteractable = GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null)
            xrInteractable = gameObject.AddComponent<XRSimpleInteractable>();
    }
}

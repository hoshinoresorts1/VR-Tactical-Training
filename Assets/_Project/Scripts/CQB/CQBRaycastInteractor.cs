using UnityEngine;
using UnityEngine.InputSystem;

// CQB shooting: raycast-based hit detection driven by the VR controller.
// - Fires on the right-hand VR controller trigger (auto-bound, no scene wiring
//   needed). Mouse/Space kept as desktop fallback.
// - Aims from `aimSource` (assign the controller); falls back to the camera.
// - Shows an aim line + reticle every frame so the player can see where they
//   point (the "aim UI").
public class CQBRaycastInteractor : MonoBehaviour
{
    public Camera playerCamera;
    public CQBScoreManager scoreManager;
    public float rayDistance = 100f;

    [Tooltip("Where the aim ray starts/points from. Assign the right controller for gun-pointing; if empty, the camera is used (look-to-aim).")]
    public Transform aimSource;

    [Tooltip("Optional explicit fire action. If empty, the right-hand trigger is auto-bound.")]
    public InputActionProperty fireAction;

    [Header("Aim visuals")]
    public Color aimColor = new Color(1f, 0.2f, 0.2f, 1f);
    public float reticleSize = 0.05f;

    private InputAction fire;
    private bool ownsAction;
    private LineRenderer line;
    private Transform reticle;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        SetupVisuals();
    }

    private void OnEnable()
    {
        if (fireAction.action != null && fireAction.action.bindings.Count > 0)
        {
            fire = fireAction.action;
            ownsAction = false;
        }
        else
        {
            // Auto-bind to the controller trigger so VR works with no wiring.
            // Both triggerButton (Vive/most) and trigger (float) + left hand for safety.
            fire = new InputAction("CQBFire", InputActionType.Button);
            fire.AddBinding("<XRController>{RightHand}/triggerButton");
            fire.AddBinding("<XRController>{RightHand}/trigger");
            fire.AddBinding("<XRController>{LeftHand}/triggerButton");
            ownsAction = true;
        }
        fire.performed += OnFire;
        fire.Enable();
    }

    private void OnDisable()
    {
        if (fire != null)
        {
            fire.performed -= OnFire;
            if (ownsAction) fire.Disable();
        }
    }

    private Transform AimT => aimSource != null ? aimSource : (playerCamera != null ? playerCamera.transform : transform);

    private void Update()
    {
        // Desktop fallback
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) FireRaycast();
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) FireRaycast();

        UpdateAimVisual();
    }

    private void OnFire(InputAction.CallbackContext ctx) => FireRaycast();

    private void UpdateAimVisual()
    {
        Transform a = AimT;
        if (a == null || line == null) return;
        Vector3 origin = a.position;
        Vector3 dir = a.forward;
        Vector3 end = origin + dir * rayDistance;
        bool hit = Physics.Raycast(origin, dir, out RaycastHit info, rayDistance);
        if (hit) end = info.point;
        line.SetPosition(0, origin);
        line.SetPosition(1, end);
        if (reticle != null)
        {
            reticle.gameObject.SetActive(hit);
            if (hit)
            {
                reticle.position = info.point + info.normal * 0.01f;
                reticle.rotation = Quaternion.LookRotation(info.normal);
            }
        }
    }

    private void FireRaycast()
    {
        Transform a = AimT;
        Ray ray = new Ray(a.position, a.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            CQBTarget target = hit.collider.GetComponentInParent<CQBTarget>();
            if (target != null && scoreManager != null) target.Hit(scoreManager);
        }
    }

    private void SetupVisuals()
    {
        line = GetComponent<LineRenderer>();
        if (line == null) line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.widthMultiplier = 0.006f;
        line.useWorldSpace = true;
        line.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        line.material.color = aimColor;
        line.startColor = line.endColor = aimColor;

        GameObject r = GameObject.CreatePrimitive(PrimitiveType.Quad);
        r.name = "CQBReticle";
        var col = r.GetComponent<Collider>();
        if (col != null) Destroy(col);
        r.transform.localScale = Vector3.one * reticleSize;
        var mr = r.GetComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mr.material.color = aimColor;
        reticle = r.transform;
        reticle.gameObject.SetActive(false);
    }
}

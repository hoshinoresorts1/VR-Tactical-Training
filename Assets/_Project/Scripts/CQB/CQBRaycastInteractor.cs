using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// CQB shooting: center-screen hit detection driven by the VR controller.
// - Fires on HTC Vive / XR controller interaction buttons.
// - Aims from the camera center instead of drawing a controller ray.
// - Mouse/Space kept as desktop fallback.
public class CQBRaycastInteractor : MonoBehaviour
{
    public Camera playerCamera;
    public CQBScoreManager scoreManager;
    public float rayDistance = 100f;

    [Tooltip("Where the aim ray starts/points from. Assign the right controller for gun-pointing; if empty, the camera is used (look-to-aim).")]
    public Transform aimSource;

    [Tooltip("Optional explicit fire action. If empty, the right-hand trigger is auto-bound.")]
    public InputActionProperty fireAction;

    [Header("Sound")]
    public AudioClip fireSound;
    [Range(0f, 1f)] public float fireVolume = 1f;

    private InputAction fire;
    private bool ownsAction;
    private AudioSource audioSource;
    private GameObject aimReticle;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        EnsureAimReticle();
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
            // Auto-bind common Vive/OpenXR interaction controls.
            fire = new InputAction("CQBFire", InputActionType.Button);
            fire.AddBinding("<XRController>{RightHand}/triggerButton");
            fire.AddBinding("<XRController>{RightHand}/trigger");
            fire.AddBinding("<XRController>{RightHand}/gripButton");
            fire.AddBinding("<XRController>{RightHand}/grip");
            fire.AddBinding("<XRController>{RightHand}/primary2DAxisClick");
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

    private void Update()
    {
        if (aimReticle != null && scoreManager != null)
            aimReticle.SetActive(!scoreManager.IsMissionComplete);

        // Desktop fallback
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) FireRaycast();
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) FireRaycast();
    }

    private void OnFire(InputAction.CallbackContext ctx) => FireRaycast();

    private Ray BuildAimRay()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        Transform source = playerCamera != null ? playerCamera.transform : (aimSource != null ? aimSource : transform);
        return new Ray(source.position, source.forward);
    }

    private void EnsureAimReticle()
    {
        if (playerCamera == null || playerCamera.transform.Find("CQBAimReticle") != null)
            return;

        aimReticle = new GameObject("CQBAimReticle", typeof(RectTransform), typeof(Canvas));
        aimReticle.transform.SetParent(playerCamera.transform, false);
        aimReticle.transform.localPosition = new Vector3(0f, 0f, 1.5f);
        aimReticle.transform.localRotation = Quaternion.identity;
        aimReticle.transform.localScale = Vector3.one * 0.001f;

        Canvas canvas = aimReticle.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = playerCamera;
        canvas.sortingOrder = 1000;

        RectTransform canvasRect = aimReticle.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(80f, 80f);

        CreateReticleLine("HorizontalOutline", new Vector2(42f, 6f), Color.black);
        CreateReticleLine("VerticalOutline", new Vector2(6f, 42f), Color.black);
        CreateReticleLine("Horizontal", new Vector2(38f, 2f), Color.white);
        CreateReticleLine("Vertical", new Vector2(2f, 38f), Color.white);
    }

    private void CreateReticleLine(string objectName, Vector2 size, Color color)
    {
        GameObject lineObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        lineObject.transform.SetParent(aimReticle.transform, false);

        RectTransform rect = lineObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        Image image = lineObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private void FireRaycast()
    {
        if (fireSound != null && audioSource != null)
            audioSource.PlayOneShot(fireSound, fireVolume);

        Ray ray = BuildAimRay();
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            CQBTarget target = hit.collider.GetComponentInParent<CQBTarget>();
            if (target != null && scoreManager != null) target.Hit(scoreManager);
        }
    }
}

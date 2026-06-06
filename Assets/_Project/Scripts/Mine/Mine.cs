using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Mine : MonoBehaviour
{
    [Tooltip("Distance from camera/player allowed to start defusal.")]
    public float interactionDistance = 3f;

    [Tooltip("Seconds the left controller trigger must be held while aiming at this mine.")]
    public float defusalHoldSeconds = 3f;

    [Tooltip("Trigger radius used for stepping on the mine.")]
    public float triggerRadius = 0.45f;

    [Tooltip("Hide the mine model until it is detected.")]
    public bool startsHidden = true;

    // Optional link to parent logical cell (assigned by MineGrid)
    [HideInInspector]
    public MineCell parentCell;

    private static Mine activeDefusalMine;
    private bool isDefused;
    private bool hasExploded;
    private bool isRevealed = true;
    private float defusalHoldTimer;
    private Renderer[] mineRenderers;
    private XRSimpleInteractable xrInteractable;
#if ENABLE_INPUT_SYSTEM
    private InputAction startDefusalAction;
#endif

    private void Awake()
    {
        gameObject.tag = "Mine";
        mineRenderers = GetComponentsInChildren<Renderer>(true);
        isRevealed = !startsHidden;
        SetMineVisible(isRevealed);
        EnsureTriggerCollider();
        EnsureXRInteractable();
    }

    private void OnEnable()
    {
        EnsureXRInteractable();

        if (xrInteractable != null)
            xrInteractable.selectEntered.AddListener(OnXRSelectEntered);

#if ENABLE_INPUT_SYSTEM
        SetupStartDefusalAction();
#endif
    }

    private void OnDisable()
    {
        if (xrInteractable != null)
            xrInteractable.selectEntered.RemoveListener(OnXRSelectEntered);

#if ENABLE_INPUT_SYSTEM
        startDefusalAction?.Dispose();
        startDefusalAction = null;
#endif
    }

    private void Update()
    {
        if (isDefused || hasExploded)
            return;

        if (IsPlayerStandingOnMine())
        {
            Explode();
            return;
        }

        UpdateTriggerHoldDefusal();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDefused || hasExploded)
            return;

        if (IsMineDetectorCollider(other))
            return;

        if (other.CompareTag("Player") || other.GetComponentInParent<CharacterController>() != null)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDefused || hasExploded)
            return;

        if (IsMineDetectorCollider(collision.collider))
            return;

        if (collision.collider.CompareTag("Player") || collision.collider.GetComponentInParent<CharacterController>() != null)
        {
            Explode();
        }
    }

    private bool IsMineDetectorCollider(Collider collider)
    {
        return collider != null && collider.GetComponentInParent<MineDetector>() != null;
    }

    private void EnsureTriggerCollider()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = triggerRadius;
            sphere.center = Vector3.up * 0.1f;
            sphere.isTrigger = true;
            return;
        }

        collider.isTrigger = true;
    }

    private void EnsureXRInteractable()
    {
        xrInteractable = GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null)
            xrInteractable = gameObject.AddComponent<XRSimpleInteractable>();
    }

    private void OnXRSelectEntered(SelectEnterEventArgs args)
    {
        ResetDefusalHold();
    }

    private bool IsDefusalHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return startDefusalAction != null && startDefusalAction.IsPressed();
#else
        return false;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private void SetupStartDefusalAction()
    {
        if (startDefusalAction != null)
            return;

        startDefusalAction = new InputAction("StartMineDefusal", InputActionType.Button);
        startDefusalAction.AddBinding("<XRController>{LeftHand}/triggerButton");
        startDefusalAction.AddBinding("<XRController>{LeftHand}/trigger");
        startDefusalAction.Enable();
    }
#endif

    private void UpdateTriggerHoldDefusal()
    {
        if (!isRevealed)
        {
            ResetDefusalHold();
            return;
        }

        bool canDefuse = (activeDefusalMine == null || activeDefusalMine == this)
            && IsDefusalHeld()
            && IsAimedByLeftController();

        if (!canDefuse)
        {
            ResetDefusalHold();
            return;
        }

        activeDefusalMine = this;
        defusalHoldTimer += Time.deltaTime;
        VRMineHUD.GetOrCreate().SetDefusalInfo("DEFUSING", Mathf.CeilToInt(Mathf.Max(0f, defusalHoldSeconds - defusalHoldTimer)));

        if (defusalHoldTimer >= defusalHoldSeconds)
        {
            BeginInstantDefusal();
        }
    }

    private void ResetDefusalHold()
    {
        if (activeDefusalMine == this)
        {
            activeDefusalMine = null;
            VRMineHUD.GetOrCreate().ClearDefusalInfo();
        }

        defusalHoldTimer = 0f;
    }

    private bool IsAimedByLeftController()
    {
        Transform leftController = FindLeftControllerTransform();
        if (leftController == null)
            return false;

        if (Physics.Raycast(leftController.position, leftController.forward, out RaycastHit hit, interactionDistance))
            return hit.collider.GetComponentInParent<Mine>() == this;

        return false;
    }

    private Transform FindLeftControllerTransform()
    {
        string[] names =
        {
            "Left Controller",
            "LeftHand Controller",
            "LeftHand Controller Stabilized",
            "Left Controller Stabilized"
        };

        foreach (string controllerName in names)
        {
            GameObject controller = GameObject.Find(controllerName);
            if (controller != null)
                return controller.transform;
        }

        var rayInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>(true);
        foreach (var rayInteractor in rayInteractors)
        {
            if (rayInteractor != null && rayInteractor.name.ToLowerInvariant().Contains("left"))
                return rayInteractor.transform;
        }

        return null;
    }

    private bool IsPlayerStandingOnMine()
    {
        Transform player = null;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else if (Camera.main != null)
        {
            player = Camera.main.transform;
        }

        if (player == null)
            return false;

        Vector2 mineXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 playerXZ = new Vector2(player.position.x, player.position.z);
        return Vector2.Distance(mineXZ, playerXZ) <= triggerRadius;
    }

    public void StartDefusal()
    {
        BeginInstantDefusal();
    }

    private void BeginInstantDefusal()
    {
        if (!isRevealed || isDefused || hasExploded)
            return;

        HandleDefusalSuccess();
    }

    private void HandleDefusalSuccess()
    {
        Debug.Log("Mine defused: " + name);
        isDefused = true;
        activeDefusalMine = null;
        if (parentCell != null)
        {
            parentCell.hasMine = false;
            parentCell.Reveal();
        }

        MineDamageManager.GetOrCreate().NotifyMineDefused();
        DetectionManager.Instance?.UnregisterMine(this);
        Destroy(gameObject);
    }

    private void HandleDefusalFailed()
    {
        Debug.Log("Defusal failed on mine: " + name);
        activeDefusalMine = null;
        if (parentCell != null)
        {
            parentCell.isRevealed = true;
            parentCell.UpdateVisual();
        }

        Explode();
    }

    public void Explode()
    {
        if (hasExploded || isDefused)
            return;

        hasExploded = true;
        activeDefusalMine = null;
        Reveal();
        Debug.Log("Mine exploded: " + name);
        var r = GetComponentInChildren<Renderer>();
        if (r != null)
        {
            r.material.color = Color.red;
        }

        MineDamageManager.GetOrCreate().ApplyMineHit(transform.position);
    }

    public bool CanBeDetected()
    {
        return this != null && !isDefused && !hasExploded && !isRevealed;
    }

    public bool CanEmitDetectionSignal()
    {
        return this != null && !isDefused && !hasExploded;
    }

    public bool IsRevealed()
    {
        return isRevealed;
    }

    public bool IsDefusalInProgress()
    {
        return activeDefusalMine == this;
    }

    public bool IsNeutralized()
    {
        return isDefused || hasExploded;
    }

    public void Reveal()
    {
        if (isRevealed)
            return;

        isRevealed = true;
        SetMineVisible(true);
    }

    public void HideIfNotActive()
    {
        if (!startsHidden || !isRevealed || isDefused || hasExploded || IsDefusalInProgress())
            return;

        isRevealed = false;
        SetMineVisible(false);
    }

    private void SetMineVisible(bool visible)
    {
        if (mineRenderers == null)
            return;

        foreach (Renderer renderer in mineRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = visible;
            }
        }
    }
}

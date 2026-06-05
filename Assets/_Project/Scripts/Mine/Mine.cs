using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Mine : MonoBehaviour
{
    [Tooltip("Prefab of the defusal UI/object (WireDefusal) to spawn when starting defusal")]
    public GameObject defusalPrefab;

    [Tooltip("Local spawn offset for the defusal UI")]
    public Vector3 defusalOffset = new Vector3(0f, 0.3f, 0f);

    [Tooltip("Distance from camera/player allowed to start defusal.")]
    public float interactionDistance = 3f;

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
    private GameObject activeDefusalObject;
    private Renderer[] mineRenderers;
    private XRSimpleInteractable xrInteractable;

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
    }

    private void OnDisable()
    {
        if (xrInteractable != null)
            xrInteractable.selectEntered.RemoveListener(OnXRSelectEntered);
    }

    private void Update()
    {
        if (isDefused || hasExploded || activeDefusalMine != null)
            return;

        if (IsPlayerStandingOnMine())
        {
            Explode();
            return;
        }

        if (IsDefusalPressed() && CanStartDefusalFromCamera())
        {
            StartDefusal();
        }
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
        StartDefusal();
    }

    private bool IsDefusalPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F);
#endif
    }

    private bool CanStartDefusalFromCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return isRevealed && Vector3.Distance(transform.position, Vector3.zero) <= interactionDistance;

        if (!isRevealed)
            return false;

        if (Vector3.Distance(cam.transform.position, transform.position) > interactionDistance)
            return false;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            return hit.collider.GetComponentInParent<Mine>() == this;
        }

        Vector3 toMine = (transform.position - cam.transform.position).normalized;
        return Vector3.Dot(cam.transform.forward, toMine) > 0.75f;
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
        if (!isRevealed || isDefused || hasExploded || activeDefusalObject != null)
            return;

        activeDefusalMine = this;
        Vector3 spawnPos = transform.position + defusalOffset;

        // If a defusal prefab is assigned, use it
        if (defusalPrefab != null)
        {
            var instance = Instantiate(defusalPrefab, spawnPos, Quaternion.identity, transform);
            activeDefusalObject = instance;
            var wire = instance.GetComponent<WireDefusal>();
            if (wire != null)
            {
                wire.onDefusalSuccess.AddListener(HandleDefusalSuccess);
                wire.onDefusalFailed.AddListener(HandleDefusalFailed);
                wire.BeginDefusal(5f);
            }
            return;
        }

        // No prefab: create a simple inline 3-wire defusal UI at runtime
        var root = new GameObject("WireDefusalRuntime");
        root.transform.SetParent(transform, false);
        root.transform.position = spawnPos;
        activeDefusalObject = root;

        var wireDef = root.AddComponent<WireDefusal>();
        wireDef.correctSequence = new int[] { 0, 1, 2 };
        wireDef.onDefusalSuccess.AddListener(HandleDefusalSuccess);
        wireDef.onDefusalFailed.AddListener(HandleDefusalFailed);

        // Create 3 simple colored wire objects
        Color[] colors = new Color[] { Color.red, Color.blue, Color.green };
        for (int i = 0; i < 3; i++)
        {
            GameObject wire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wire.name = "Wire_" + i;
            wire.transform.SetParent(root.transform, false);
            wire.transform.localScale = new Vector3(0.025f, 0.3f, 0.025f);
            wire.transform.localPosition = new Vector3(0f, 0.035f, (i - 1) * 0.1f);
            wire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

            var mr = wire.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Standard");

                if (shader != null)
                {
                    mr.sharedMaterial = new Material(shader) { color = colors[i] };
                }
            }

            var inter = wire.AddComponent<WireInteractable>();
            inter.wireIndex = i;
            inter.wireDefusal = wireDef;

            var col = wire.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        wireDef.BeginDefusal(5f);
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
        return activeDefusalObject != null;
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

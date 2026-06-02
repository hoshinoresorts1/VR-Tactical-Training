using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Direct-to-bone equip model:
// - Each gear item knows which Humanoid bone it should attach to (e.g. Helmet → Neck).
// - When the user points the controller ray at the item and presses Trigger, the
//   manager looks up that bone on the player's character Animator and re-parents
//   the item there. No intermediate "slot" GameObjects are needed.
// - If the target bone has a non-unit lossy scale (our head bone is shrunk to hide
//   the face mesh), the item compensates so it still renders at its authored size.
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class GearItem : MonoBehaviour
{
    [SerializeField] private GearType gearType;
    [SerializeField] private HumanBodyBones attachBone = HumanBodyBones.Head;
    [SerializeField] private Vector3 localPositionOffset;
    [SerializeField] private Vector3 localEulerOffset;
    [SerializeField] private Rigidbody itemRigidbody;
    [SerializeField] private XRGrabInteractable grabInteractable;

    public GearType GearType => gearType;
    public HumanBodyBones AttachBone => attachBone;
    public Vector3 LocalPositionOffset => localPositionOffset;
    public Vector3 LocalEulerOffset => localEulerOffset;
    public bool IsEquipped { get; private set; }

    private Transform originalParent;
    private Vector3 originalLocalScale = Vector3.one;    private int originalLayer;

    private GearEquipManager equipManager;
    private Transform attachedBone;
    private Vector3 activePositionOffset;
    private Vector3 activeEulerOffset;

    private void Reset()
    {
        CacheComponents();
    }

    private void Awake()
    {
        CacheComponents();
        originalParent = transform.parent;
        originalLocalScale = transform.localScale;
        originalLayer = gameObject.layer;
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
        }
    }

    // New direct-to-bone equip. Re-parents the item under the given bone and
    // compensates for the bone's lossy scale so the item renders at original size.
    // LateUpdate re-asserts parent/transform/kinematic so the XR Toolkit's drop
    // logic can't pull the item back to gravity once we're equipped.
    public void EquipToBone(Transform bone, Vector3 positionOffset, Vector3 eulerOffset, GearEquipManager manager)
    {
        if (bone == null || IsEquipped)
        {
            return;
        }

        IsEquipped = true;
        equipManager = manager;
        attachedBone = bone;
        activePositionOffset = positionOffset;
        activeEulerOffset = eulerOffset;

        if (itemRigidbody != null)
        {
            if (!itemRigidbody.isKinematic)
            {
                itemRigidbody.velocity = Vector3.zero;
                itemRigidbody.angularVelocity = Vector3.zero;
            }
            itemRigidbody.isKinematic = true;
            itemRigidbody.useGravity = false;
        }

        ApplyEquippedTransform();

        // Move the worn item onto the character's layer (PlayerCharacter) so the
        // mirror, which culls to that layer only, reflects the equipped gear.
        SetLayerRecursively(gameObject, bone.gameObject.layer);
    }

    private void ApplyEquippedTransform()
    {
        if (attachedBone == null)
        {
            return;
        }
        transform.SetParent(attachedBone, false);
        transform.localPosition = activePositionOffset;
        transform.localRotation = Quaternion.Euler(activeEulerOffset);

        // Compensate for any non-unit parent scale.
        Vector3 parentLossy = attachedBone.lossyScale;
        if (Mathf.Abs(parentLossy.x) > 0.0001f &&
            Mathf.Abs(parentLossy.y) > 0.0001f &&
            Mathf.Abs(parentLossy.z) > 0.0001f)
        {
            transform.localScale = new Vector3(
                originalLocalScale.x / parentLossy.x,
                originalLocalScale.y / parentLossy.y,
                originalLocalScale.z / parentLossy.z);
        }
    }

    private void LateUpdate()
    {
        if (!IsEquipped)
        {
            return;
        }
        // The XR Interaction Toolkit's Drop() runs on its internal selectExited and
        // resets isKinematic to whatever it captured at grab start. Re-assert here.
        if (itemRigidbody != null && !itemRigidbody.isKinematic)
        {
            itemRigidbody.isKinematic = true;
            itemRigidbody.useGravity = false;
        }
        // If something (XRI / parent reparenting) pulled us off the bone, snap back.
        if (attachedBone != null && transform.parent != attachedBone)
        {
            ApplyEquippedTransform();
        }
    }

    public void Unequip()
    {
        if (!IsEquipped)
        {
            return;
        }

        IsEquipped = false;
        attachedBone = null;

        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
            itemRigidbody.useGravity = true;
        }

        transform.SetParent(originalParent, true);
        transform.localScale = originalLocalScale;

        // Restore the original layer so the un-equipped item is no longer
        // reflected by the PlayerCharacter-only mirror.
        SetLayerRecursively(gameObject, originalLayer);

        if (equipManager != null)
        {
            GearEquipManager manager = equipManager;
            equipManager = null;
            manager.NotifyUnequipped(this);
        }
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (IsEquipped)
        {
            Unequip();
        }
        else
        {
            if (equipManager == null)
            {
                equipManager = FindFirstObjectByType<GearEquipManager>();
            }
            if (equipManager != null)
            {
                equipManager.RequestEquip(this);
            }
        }

        // Force-end the selection so the XR interactor doesn't keep holding the item.
        // Disabling/re-enabling the XRGrabInteractable cancels the active selection.
        if (isActiveAndEnabled)
        {
            StartCoroutine(EndSelectionNextFrame());
        }
    }

    // Wait long enough for the user to physically release the trigger before we
    // re-enable interaction. 1 frame (~16ms) was way too short, so each press was
    // re-triggering RequestEquip → toggling state multiple times and ending up equipped.
    private IEnumerator EndSelectionNextFrame()
    {
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }
        yield return new WaitForSeconds(0.4f);
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }
    }

    private void CacheComponents()
    {
        if (itemRigidbody == null)
        {
            itemRigidbody = GetComponent<Rigidbody>();
        }

        if (grabInteractable == null)
        {
            grabInteractable = GetComponent<XRGrabInteractable>();
        }
    }
}

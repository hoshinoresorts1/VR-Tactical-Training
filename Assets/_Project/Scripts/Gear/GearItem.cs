using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public class GearItem : MonoBehaviour
{
    [SerializeField] private GearType gearType;
    [SerializeField] private Rigidbody itemRigidbody;
    [SerializeField] private XRGrabInteractable grabInteractable;

    public GearType GearType => gearType;
    public bool IsEquipped { get; private set; }

    private Transform originalParent;
    private GearEquipManager equipManager;
    private bool wasEquippedAtGrab;

    private void Reset()
    {
        CacheComponents();
    }

    private void Awake()
    {
        CacheComponents();
        originalParent = transform.parent;
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    public void EquipToSlot(Transform slotTransform, GearEquipManager manager)
    {
        if (slotTransform == null || IsEquipped)
        {
            return;
        }

        IsEquipped = true;
        equipManager = manager;

        // grabInteractable stays enabled so the user can re-grab to unequip.
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = true;
            itemRigidbody.useGravity = false;
            itemRigidbody.velocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
        }

        transform.SetParent(slotTransform, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        if (!IsEquipped)
        {
            return;
        }

        IsEquipped = false;

        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
            itemRigidbody.useGravity = true;
        }

        // Keep current world position; XR grab takes over if the user is holding it,
        // otherwise physics applies.
        transform.SetParent(originalParent, true);

        if (equipManager != null)
        {
            GearEquipManager manager = equipManager;
            equipManager = null;
            manager.NotifyUnequipped(this);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (IsEquipped)
        {
            wasEquippedAtGrab = true;
            Unequip();
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!wasEquippedAtGrab)
        {
            return;
        }

        wasEquippedAtGrab = false;
        // XRGrabInteractable restores its captured Rigidbody state in its own OnSelectExited
        // implementation (which runs after this UnityEvent fires). Defer one frame so our
        // reset wins, otherwise the item would stay kinematic mid-air after release.
        if (isActiveAndEnabled)
        {
            StartCoroutine(ResetRigidbodyNextFrame());
        }
    }

    private IEnumerator ResetRigidbodyNextFrame()
    {
        yield return null;
        if (itemRigidbody != null && !IsEquipped)
        {
            itemRigidbody.isKinematic = false;
            itemRigidbody.useGravity = true;
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

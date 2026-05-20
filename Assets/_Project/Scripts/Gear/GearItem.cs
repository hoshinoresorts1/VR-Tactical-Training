using UnityEngine;
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

    private void Reset()
    {
        CacheComponents();
    }

    private void Awake()
    {
        CacheComponents();
    }

    public void EquipToSlot(Transform slotTransform)
    {
        if (slotTransform == null || IsEquipped)
        {
            return;
        }

        IsEquipped = true;

        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

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

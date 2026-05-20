using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GearSlot : MonoBehaviour
{
    [SerializeField] private GearType acceptedType;
    [SerializeField] private Transform attachPoint;
    [SerializeField] private GearEquipManager equipManager;

    public GearType AcceptedType => acceptedType;
    public Transform AttachPoint => attachPoint != null ? attachPoint : transform;

    private void Reset()
    {
        EnsureTriggerCollider();
        if (attachPoint == null)
        {
            attachPoint = transform;
        }
    }

    private void Awake()
    {
        EnsureTriggerCollider();

        if (attachPoint == null)
        {
            attachPoint = transform;
        }

        if (equipManager == null)
        {
            equipManager = FindFirstObjectByType<GearEquipManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GearItem gearItem = other.GetComponentInParent<GearItem>();
        if (gearItem == null || gearItem.IsEquipped || gearItem.GearType != acceptedType || equipManager == null)
        {
            return;
        }

        equipManager.TryEquip(gearItem, this);
    }

    private void EnsureTriggerCollider()
    {
        Collider slotCollider = GetComponent<Collider>();
        if (slotCollider != null)
        {
            slotCollider.isTrigger = true;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GearSlot : MonoBehaviour
{
    [SerializeField] private GearType acceptedType;
    [SerializeField] private Transform attachPoint;
    [SerializeField] private GearEquipManager equipManager;
    [SerializeField] private GearSlotVisual slotVisual;

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

        if (slotVisual == null)
        {
            slotVisual = GetComponent<GearSlotVisual>();
        }
    }

    private void OnEnable()
    {
        if (equipManager != null)
        {
            equipManager.EquippedStateChanged += OnEquippedStateChanged;
        }
    }

    private void OnDisable()
    {
        if (equipManager != null)
        {
            equipManager.EquippedStateChanged -= OnEquippedStateChanged;
        }
    }

    private void Start()
    {
        // Default to showing the idle guide. The manager will reconcile via the event right after.
        if (slotVisual != null)
        {
            slotVisual.ShowIdle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GearItem gearItem = other.GetComponentInParent<GearItem>();
        if (gearItem == null || gearItem.IsEquipped || gearItem.GearType != acceptedType || equipManager == null)
        {
            return;
        }

        if (equipManager.TryEquip(gearItem, this) && slotVisual != null)
        {
            slotVisual.FlashEquipped();
        }
    }

    private void OnEquippedStateChanged(IReadOnlyDictionary<GearType, bool> state, bool isReady)
    {
        if (slotVisual == null || state == null)
        {
            return;
        }

        bool myTypeEquipped = state.TryGetValue(acceptedType, out bool value) && value;
        if (!myTypeEquipped)
        {
            // Slot is empty (or just became empty via unequip) — show the guide again.
            slotVisual.ShowIdle();
        }
        // If equipped: FlashEquipped() (triggered above on successful TryEquip) handles the
        // visual and hides the guide at the end of its flash routine.
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

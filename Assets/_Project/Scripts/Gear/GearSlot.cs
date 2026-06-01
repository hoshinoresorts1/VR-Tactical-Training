using System.Collections.Generic;
using UnityEngine;

// In the new Ray+Trigger interaction model the slot no longer needs a trigger
// collider — the manager (via GearItem.selectEntered) drives equip directly.
// We keep optional trigger support for backward compatibility but never require it.
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
        // Legacy trigger-based equip path is no longer used in the direct-to-bone model.
        // The GearItem now drives equip via its selectEntered handler (Ray + Trigger).
        // Kept intentionally empty so existing trigger colliders cause no errors.
        _ = other;
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
        // Optional: if a collider exists (legacy slots), make sure it's a trigger.
        // Bone-attached slots typically have no collider at all.
        Collider slotCollider = GetComponent<Collider>();
        if (slotCollider != null)
        {
            slotCollider.isTrigger = true;
        }
    }
}

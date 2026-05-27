using System;
using System.Collections.Generic;
using UnityEngine;

public class GearEquipManager : MonoBehaviour
{
    [SerializeField] private GearChecklistUI checklistUI;
    [SerializeField] private int completionScore = 20;

    private readonly Dictionary<GearType, bool> equippedState = new Dictionary<GearType, bool>();
    private bool scoreAwarded;

    public event Action<IReadOnlyDictionary<GearType, bool>, bool> EquippedStateChanged;

    private void Awake()
    {
        InitializeState();

        if (checklistUI == null)
        {
            checklistUI = FindFirstObjectByType<GearChecklistUI>();
        }
    }

    private void Start()
    {
        NotifyStateChanged();
    }

    public bool TryEquip(GearItem item, GearSlot slot)
    {
        if (item == null || slot == null || item.IsEquipped)
        {
            return false;
        }

        GearType gearType = item.GearType;
        if (gearType != slot.AcceptedType || IsEquipped(gearType))
        {
            return false;
        }

        item.EquipToSlot(slot.AttachPoint, this);
        equippedState[gearType] = true;
        NotifyStateChanged();
        AwardScoreIfComplete();
        return true;
    }

    // Called by GearItem when the user re-grabs an equipped item to detach it.
    // Score is intentionally NOT refunded: once the trainee completes the gear set, the score stays.
    public void NotifyUnequipped(GearItem item)
    {
        if (item == null)
        {
            return;
        }

        if (!equippedState.TryGetValue(item.GearType, out bool isCurrentlyEquipped) || !isCurrentlyEquipped)
        {
            return;
        }

        equippedState[item.GearType] = false;
        NotifyStateChanged();
    }

    public bool IsEquipped(GearType gearType)
    {
        return equippedState.TryGetValue(gearType, out bool isEquipped) && isEquipped;
    }

    public bool IsTrainingReady()
    {
        foreach (GearType gearType in Enum.GetValues(typeof(GearType)))
        {
            if (!IsEquipped(gearType))
            {
                return false;
            }
        }

        return true;
    }

    public IReadOnlyDictionary<GearType, bool> GetEquippedState()
    {
        return equippedState;
    }

    private void InitializeState()
    {
        equippedState.Clear();
        foreach (GearType gearType in Enum.GetValues(typeof(GearType)))
        {
            equippedState[gearType] = false;
        }
    }

    private void NotifyStateChanged()
    {
        bool isReady = IsTrainingReady();
        checklistUI?.Refresh(equippedState, isReady);
        EquippedStateChanged?.Invoke(equippedState, isReady);
    }

    private void AwardScoreIfComplete()
    {
        if (scoreAwarded || !IsTrainingReady())
        {
            return;
        }

        scoreAwarded = true;
        if (TrainingScoreManager.Instance != null)
        {
            TrainingScoreManager.Instance.AddGearScore(completionScore);
        }
    }
}

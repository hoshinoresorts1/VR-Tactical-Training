using System;
using System.Collections.Generic;
using UnityEngine;

public class GearEquipManager : MonoBehaviour
{
    [SerializeField] private GearChecklistUI checklistUI;
    [SerializeField] private int completionScore = 20;
    [SerializeField] private Animator characterAnimator;

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

    // Ray + Trigger interaction: GearItem calls this on selectEntered.
    // We look up the item's desired Humanoid bone on the character Animator and
    // re-parent the item there. No intermediate GearSlot is involved.
    public bool RequestEquip(GearItem item)
    {
        if (item == null || item.IsEquipped)
        {
            return false;
        }
        if (IsEquipped(item.GearType))
        {
            return false;
        }
        EnsureAnimatorCached();
        if (characterAnimator == null || !characterAnimator.isHuman)
        {
            return false;
        }
        Transform bone = characterAnimator.GetBoneTransform(item.AttachBone);
        if (bone == null)
        {
            return false;
        }

        item.EquipToBone(bone, item.LocalPositionOffset, item.LocalEulerOffset, this);
        equippedState[item.GearType] = true;
        NotifyStateChanged();
        AwardScoreIfComplete();
        return true;
    }

    private void EnsureAnimatorCached()
    {
        if (characterAnimator != null)
        {
            return;
        }
        // Lazy lookup: any Humanoid Animator in the scene (typically PlayerCharacter).
        Animator[] all = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].isHuman)
            {
                characterAnimator = all[i];
                return;
            }
        }
    }

    // UI-triggered single-item unequip (called from the checklist row buttons).
    public bool RequestUnequip(GearType type)
    {
        GearItem[] items = FindObjectsByType<GearItem>(FindObjectsSortMode.None);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].GearType == type && items[i].IsEquipped)
            {
                items[i].Unequip();
                return true;
            }
        }
        return false;
    }

    // UI-triggered "take everything off" — called from the UNEQUIP ALL button.
    public int RequestUnequipAll()
    {
        int count = 0;
        GearItem[] items = FindObjectsByType<GearItem>(FindObjectsSortMode.None);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].IsEquipped)
            {
                items[i].Unequip();
                count++;
            }
        }
        return count;
    }

    // Called by GearItem when the user re-selects an equipped item to detach it.
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

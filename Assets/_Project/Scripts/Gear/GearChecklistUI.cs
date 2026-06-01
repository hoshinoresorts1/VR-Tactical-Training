using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Two display modes:
// 1) Per-row buttons (preferred): assign `entries` with one Button + label per GearType.
//    Each button is clickable via ray; clicking unequips that piece. Buttons are
//    only `interactable` while the matching gear is equipped.
// 2) Legacy single text: if `entries` is empty, falls back to drawing the whole
//    checklist into `checklistText` so old setups still work.
//
// An optional `unequipAllButton` triggers RequestUnequipAll. `statusLabel` shows
// READY when complete. `scoreLabel` mirrors TrainingScoreManager.GearScore.
public class GearChecklistUI : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public GearType gearType;
        public Button button;
        public TMP_Text label;
    }

    [SerializeField] private GearEquipManager equipManager;
    [SerializeField] private List<Entry> entries = new List<Entry>();
    [SerializeField] private Button unequipAllButton;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_Text checklistText;

    private static readonly Dictionary<GearType, string> Labels = new Dictionary<GearType, string>
    {
        { GearType.Helmet, "Helmet" },
        { GearType.Vest, "Vest" },
        { GearType.Belt, "Belt" },
        { GearType.Canteen, "Canteen" },
        { GearType.Grenade, "Grenade" }
    };

    private void Awake()
    {
        if (checklistText == null)
        {
            checklistText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void Start()
    {
        if (entries != null)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry e = entries[i];
                if (e == null || e.button == null) continue;
                GearType captured = e.gearType;
                e.button.onClick.AddListener(() =>
                {
                    if (equipManager != null)
                    {
                        equipManager.RequestUnequip(captured);
                    }
                });
            }
        }

        if (unequipAllButton != null)
        {
            unequipAllButton.onClick.AddListener(() =>
            {
                if (equipManager != null)
                {
                    equipManager.RequestUnequipAll();
                }
            });
        }
    }

    public void Refresh(IReadOnlyDictionary<GearType, bool> equippedState, bool isTrainingReady)
    {
        if (equippedState == null)
        {
            return;
        }

        bool usePerRow = entries != null && entries.Count > 0;

        if (usePerRow)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry e = entries[i];
                if (e == null) continue;
                bool eq = equippedState.TryGetValue(e.gearType, out bool value) && value;
                if (e.label != null)
                {
                    e.label.text = $"{(eq ? "[X]" : "[ ]")} {Labels[e.gearType]}";
                }
                if (e.button != null)
                {
                    e.button.interactable = eq;
                }
            }

            if (statusLabel != null)
            {
                statusLabel.text = isTrainingReady ? "READY" : "";
            }
        }
        else if (checklistText != null)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Gear Checklist");
            AppendLine(builder, equippedState, GearType.Helmet);
            AppendLine(builder, equippedState, GearType.Vest);
            AppendLine(builder, equippedState, GearType.Belt);
            AppendLine(builder, equippedState, GearType.Canteen);
            AppendLine(builder, equippedState, GearType.Grenade);
            if (isTrainingReady)
            {
                builder.AppendLine();
                builder.Append("READY");
            }
            checklistText.text = builder.ToString();
        }

        if (scoreLabel != null && TrainingScoreManager.Instance != null)
        {
            scoreLabel.text = $"Score: {TrainingScoreManager.Instance.GearScore}";
        }

        if (unequipAllButton != null)
        {
            // disable when nothing is equipped
            bool anyEquipped = false;
            foreach (var kv in equippedState)
            {
                if (kv.Value) { anyEquipped = true; break; }
            }
            unequipAllButton.interactable = anyEquipped;
        }
    }

    private static void AppendLine(StringBuilder builder, IReadOnlyDictionary<GearType, bool> equippedState, GearType gearType)
    {
        bool isEquipped = equippedState.TryGetValue(gearType, out bool value) && value;
        string marker = isEquipped ? "[X]" : "[ ]";
        builder.AppendLine($"{marker} {Labels[gearType]}");
    }
}

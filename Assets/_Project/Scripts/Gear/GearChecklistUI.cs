using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GearChecklistUI : MonoBehaviour
{
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

    public void Refresh(IReadOnlyDictionary<GearType, bool> equippedState, bool isTrainingReady)
    {
        if (checklistText == null || equippedState == null)
        {
            return;
        }

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

    private static void AppendLine(StringBuilder builder, IReadOnlyDictionary<GearType, bool> equippedState, GearType gearType)
    {
        bool isEquipped = equippedState.TryGetValue(gearType, out bool value) && value;
        string marker = isEquipped ? "[X]" : "[ ]";
        builder.AppendLine($"{marker} {Labels[gearType]}");
    }
}

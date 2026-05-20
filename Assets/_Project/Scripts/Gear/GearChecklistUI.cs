using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class GearChecklistUI : MonoBehaviour
{
    [SerializeField] private TMP_Text checklistText;

    private static readonly Dictionary<GearType, string> Labels = new Dictionary<GearType, string>
    {
        { GearType.Helmet, "헬멧" },
        { GearType.Vest, "방탄조끼" },
        { GearType.Belt, "탄띠" },
        { GearType.Canteen, "수통" },
        { GearType.Grenade, "수류탄" }
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
        builder.AppendLine("군장 착용 체크리스트");

        AppendLine(builder, equippedState, GearType.Helmet);
        AppendLine(builder, equippedState, GearType.Vest);
        AppendLine(builder, equippedState, GearType.Belt);
        AppendLine(builder, equippedState, GearType.Canteen);
        AppendLine(builder, equippedState, GearType.Grenade);

        if (isTrainingReady)
        {
            builder.AppendLine();
            builder.Append("훈련 준비 완료");
        }

        checklistText.text = builder.ToString();
    }

    private static void AppendLine(StringBuilder builder, IReadOnlyDictionary<GearType, bool> equippedState, GearType gearType)
    {
        bool isEquipped = equippedState.TryGetValue(gearType, out bool value) && value;
        string marker = isEquipped ? "✅" : "□";
        builder.AppendLine($"{marker} {Labels[gearType]}");
    }
}

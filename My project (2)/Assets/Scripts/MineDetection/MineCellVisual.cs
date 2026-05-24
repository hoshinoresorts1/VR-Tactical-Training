using UnityEngine;

public class MineCellVisual : MonoBehaviour
{
    private MineCell cellData;
    private TextMesh textMesh;

    public void Initialize(MineCell cell)
    {
        cellData = cell;
        textMesh = CreateTextMesh();
        UpdateDisplay();
    }

    private TextMesh CreateTextMesh()
    {
        GameObject textRoot = new GameObject("CellNumber");
        textRoot.transform.SetParent(transform, false);
        textRoot.transform.localPosition = new Vector3(0f, 0.11f, 0f);
        textRoot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        TextMesh tm = textRoot.AddComponent<TextMesh>();
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.characterSize = 0.08f;
        tm.fontSize = 64;
        tm.color = Color.black;
        tm.text = string.Empty;
        return tm;
    }

    public void UpdateDisplay()
    {
        if (textMesh == null || cellData == null)
            return;

        if (!cellData.isRevealed)
        {
            textMesh.text = string.Empty;
            return;
        }

        if (cellData.hasMine)
        {
            textMesh.text = "X";
            textMesh.color = Color.red;
            return;
        }

        if (cellData.adjacentMineCount > 0)
        {
            textMesh.text = cellData.adjacentMineCount.ToString();
            textMesh.color = GetNumberColor(cellData.adjacentMineCount);
        }
        else
        {
            textMesh.text = string.Empty;
        }
    }

    private Color GetNumberColor(int mineCount)
    {
        return mineCount switch
        {
            1 => Color.blue,
            2 => Color.green,
            3 => Color.red,
            4 => new Color(0f, 0f, 0.5f),
            5 => new Color(0.5f, 0f, 0f),
            6 => Color.cyan,
            7 => Color.black,
            8 => Color.gray,
            _ => Color.white
        };
    }
}

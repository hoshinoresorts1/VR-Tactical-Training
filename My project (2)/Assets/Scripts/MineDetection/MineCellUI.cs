using UnityEngine;
using TMPro;

public class MineCellUI : MonoBehaviour
{
    private TextMeshProUGUI numberText;
    private MineCell cellData;

    private void Start()
    {
        // TextMeshPro 텍스트 생성
        GameObject textObj = new GameObject("CellNumber");
        textObj.transform.parent = transform;
        textObj.transform.localPosition = Vector3.zero;
        
        numberText = textObj.AddComponent<TextMeshProUGUI>();
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 100);
    }

    public void UpdateDisplay(MineCell cell)
    {
        cellData = cell;
        if (numberText == null) return;

        if (!cell.isRevealed)
        {
            numberText.text = "";
        }
        else if (cell.hasMine)
        {
            numberText.text = "💣";
            numberText.color = Color.red;
        }
        else if (cell.adjacentMineCount > 0)
        {
            numberText.text = cell.adjacentMineCount.ToString();
            numberText.color = GetNumberColor(cell.adjacentMineCount);
        }
        else
        {
            numberText.text = "";
        }
    }

    private Color GetNumberColor(int mineCount)
    {
        return mineCount switch
        {
            1 => Color.blue,
            2 => Color.green,
            3 => Color.red,
            4 => new Color(0, 0, 0.5f),
            5 => new Color(0.5f, 0, 0),
            6 => Color.cyan,
            7 => Color.black,
            8 => Color.gray,
            _ => Color.white
        };
    }
}

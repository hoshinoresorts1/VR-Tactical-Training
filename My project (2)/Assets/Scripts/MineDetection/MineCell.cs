using UnityEngine;

public class MineCell
{
    public int gridX;
    public int gridY;
    public bool hasMine;
    public bool isRevealed;
    public int adjacentMineCount;
    public GameObject cellVisualObject;
    public GameObject mineObject;
    public Material revealedMaterial;
    public Material unrevealedMaterial;

    public MineCell(int x, int y)
    {
        gridX = x;
        gridY = y;
        hasMine = false;
        isRevealed = false;
        adjacentMineCount = 0;
    }

    public void Reveal()
    {
        if (!isRevealed)
        {
            isRevealed = true;
            UpdateVisual();
        }
    }

    public void UpdateVisual()
    {
        if (cellVisualObject != null)
        {
            Renderer renderer = cellVisualObject.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material = isRevealed && revealedMaterial != null ? revealedMaterial : unrevealedMaterial;
            }

            if (mineObject != null)
            {
                mineObject.SetActive(isRevealed && hasMine);
            }

            MineCellVisual visual = cellVisualObject.GetComponent<MineCellVisual>() ?? cellVisualObject.GetComponentInChildren<MineCellVisual>();
            if (visual != null)
            {
                visual.UpdateDisplay();
            }
        }
    }

    public void Reset()
    {
        hasMine = false;
        isRevealed = false;
        adjacentMineCount = 0;
    }
}

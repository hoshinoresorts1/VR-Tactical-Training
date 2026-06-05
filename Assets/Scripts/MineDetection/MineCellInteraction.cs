using UnityEngine;

public class MineCellInteraction : MonoBehaviour
{
    private MineGrid gridReference;
    private MineCell cellData;

    public void Initialize(MineGrid grid, MineCell cell)
    {
        gridReference = grid;
        cellData = cell;
    }

    private void OnMouseDown()
    {
        SelectCell();
    }

    public void SelectCell()
    {
        if (gridReference == null || cellData == null)
            return;

        gridReference.OnCellClicked(cellData.gridX, cellData.gridY);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gridReference == null || cellData == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (cellData.hasMine)
        {
            // Activate mine visual and play explosion effect if available
            if (cellData.mineObject != null)
            {
                cellData.mineObject.SetActive(true);
                var mineComp = cellData.mineObject.GetComponent<global::Mine>();
                if (mineComp != null)
                    mineComp.Explode();
            }

            gridReference.OnPlayerSteppedMine();
        }
        else
        {
            gridReference.OnCellClicked(cellData.gridX, cellData.gridY);
        }
    }
}

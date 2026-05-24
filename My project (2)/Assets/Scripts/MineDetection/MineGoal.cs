using UnityEngine;

public class MineGoal : MonoBehaviour
{
    public MineGrid mineGrid;
    [SerializeField] private string playerTag = "Player";

    private void Awake()
    {
        if (mineGrid == null)
        {
            mineGrid = FindObjectOfType<MineGrid>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        mineGrid?.OnGoalReached();
    }
}

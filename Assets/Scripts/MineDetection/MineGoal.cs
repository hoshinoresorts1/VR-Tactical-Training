using UnityEngine;

public class MineGoal : MonoBehaviour
{
    public MineGrid mineGrid;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Vector3 triggerSize = new Vector3(3f, 2f, 3f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureGoalObjectSetup()
    {
        GameObject goalObject = GameObject.Find("MineGoal");
        if (goalObject == null)
            return;

        MineGoal goal = goalObject.GetComponent<MineGoal>();
        if (goal == null)
            goal = goalObject.AddComponent<MineGoal>();

        goal.EnsureTrigger();
    }

    private void Awake()
    {
        if (mineGrid == null)
        {
            mineGrid = FindObjectOfType<MineGrid>();
        }

        EnsureTrigger();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        mineGrid?.OnGoalReached();
        MineDamageManager.GetOrCreate().CompleteTraining();
    }

    private void EnsureTrigger()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        if (trigger == null)
            trigger = gameObject.AddComponent<BoxCollider>();

        trigger.isTrigger = true;
        trigger.center = Vector3.up;
        trigger.size = triggerSize;
    }
}

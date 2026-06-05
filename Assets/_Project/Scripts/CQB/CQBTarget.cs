using UnityEngine;

public class CQBTarget : MonoBehaviour
{
    public enum TargetType
    {
        Enemy,
        Civilian
    }

    public TargetType targetType;

    public void Hit(CQBScoreManager scoreManager)
    {
        if (targetType == TargetType.Enemy)
        {
            if (scoreManager != null)
            {
                scoreManager.HitEnemy();
            }

            Destroy(gameObject);
        }
        else if (targetType == TargetType.Civilian)
        {
            if (scoreManager != null)
            {
                scoreManager.HitCivilian();
            }
        }
    }
}
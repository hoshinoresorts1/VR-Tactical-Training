using UnityEngine;

public class CQBEnemyExposure : MonoBehaviour
{
    public enum ViewAxis
    {
        Forward,
        Backward,
        Right,
        Left
    }

    [Header("References")]
    public Transform player;
    public CQBScoreManager scoreManager;

    [Header("View Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public float enemyEyeHeight = 1.6f;
    public float playerEyeHeight = 1.6f;
    public ViewAxis viewAxis = ViewAxis.Forward;

    [Header("Penalty Settings")]
    public float exposureLimit = 3f;
    public int penaltyScore = 5;
    public float penaltyCooldown = 3f;

    [Header("Debug")]
    public bool showDebugRay = true;

    private float exposedTime = 0f;
    private float cooldownTimer = 0f;

    void Update()
    {
        if (player == null || scoreManager == null)
            return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (CanSeePlayer())
        {
            exposedTime += Time.deltaTime;

            if (exposedTime >= exposureLimit && cooldownTimer <= 0f)
            {
                scoreManager.AddPenalty(penaltyScore);
                Debug.Log("Exposure Penalty -" + penaltyScore);

                exposedTime = 0f;
                cooldownTimer = penaltyCooldown;
            }
        }
        else
        {
            exposedTime = 0f;
        }
    }

    bool CanSeePlayer()
    {
        Vector3 enemyEye = transform.position + Vector3.up * enemyEyeHeight;
        Vector3 playerEye = player.position + Vector3.up * playerEyeHeight;

        Vector3 dirToPlayer = playerEye - enemyEye;
        float distanceToPlayer = dirToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return false;

        Vector3 viewDirection = GetViewDirection();
        float angle = Vector3.Angle(viewDirection, dirToPlayer.normalized);

        if (showDebugRay)
        {
            Debug.DrawRay(enemyEye, viewDirection * viewDistance, Color.green);
            Debug.DrawRay(enemyEye, dirToPlayer.normalized * distanceToPlayer, Color.red);
        }

        if (angle > viewAngle * 0.5f)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(
            enemyEye,
            dirToPlayer.normalized,
            distanceToPlayer
        );

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.transform;

            if (hitTransform == transform || hitTransform.IsChildOf(transform))
                continue;

            if (hitTransform == player || hitTransform.IsChildOf(player))
                continue;

            if (hitTransform.CompareTag("Enemy") || hitTransform.CompareTag("Civilian"))
                continue;

            Debug.Log("Enemy sight blocked by: " + hitTransform.name);
            return false;
        }

        Debug.Log("Enemy can see player");
        return true;
    }

    Vector3 GetViewDirection()
    {
        switch (viewAxis)
        {
            case ViewAxis.Backward:
                return -transform.forward;
            case ViewAxis.Right:
                return transform.right;
            case ViewAxis.Left:
                return -transform.right;
            default:
                return transform.forward;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 eye = transform.position + Vector3.up * enemyEyeHeight;
        Vector3 viewDir = GetViewDirection();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eye, viewDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(eye, viewDir * viewDistance);

        Vector3 leftDir = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * viewDir;
        Vector3 rightDir = Quaternion.Euler(0, viewAngle * 0.5f, 0) * viewDir;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(eye, leftDir * viewDistance);
        Gizmos.DrawRay(eye, rightDir * viewDistance);
    }
}
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ShootingTarget : MonoBehaviour
{
    [Header("Score Area")]
    [Tooltip("Fine-tunes the bullseye center inside the target collider.")]
    public Vector2 centerOffset = new Vector2(0f, -0.015f);

    [Tooltip("Score awarded when the target is hit outside the main rings.")]
    public int minimumHitScore = 1;

    private BoxCollider targetCollider;

    private void Awake()
    {
        targetCollider = GetComponent<BoxCollider>();
    }

    public bool TryCalculateScoreFromRay(Ray ray, float maxDistance, out int score, out Vector3 scorePoint)
    {
        EnsureCollider();

        score = 0;
        scorePoint = Vector3.zero;

        if (targetCollider == null)
            return false;

        if (!targetCollider.Raycast(ray, out RaycastHit hit, maxDistance))
            return false;

        scorePoint = hit.point;
        score = CalculateEllipseScore(hit.point);
        return true;
    }

    public int CalculateEllipseScore(Vector3 hitPoint)
    {
        EnsureCollider();

        if (targetCollider == null)
            return 0;

        Vector3 localHit = transform.InverseTransformPoint(hitPoint);
        Vector3 colliderCenter = targetCollider.center;
        Vector3 colliderSize = targetCollider.size;

        float radiusX = Mathf.Max(0.001f, colliderSize.x * 0.5f);
        float radiusY = Mathf.Max(0.001f, colliderSize.y * 0.5f);
        float targetCenterX = colliderCenter.x + centerOffset.x;
        float targetCenterY = colliderCenter.y + centerOffset.y;

        float normX = (localHit.x - targetCenterX) / radiusX;
        float normY = (localHit.y - targetCenterY) / radiusY;
        float ellipseValue = (normX * normX) + (normY * normY);

        if (ellipseValue <= 0.025f) return 10;
        if (ellipseValue <= 0.110f) return 9;
        if (ellipseValue <= 0.310f) return 8;
        if (ellipseValue <= 0.680f) return 7;
        if (ellipseValue <= 1.000f) return Mathf.Max(1, minimumHitScore);

        return Mathf.Max(1, minimumHitScore);
    }

    private void EnsureCollider()
    {
        if (targetCollider == null)
            targetCollider = GetComponent<BoxCollider>();
    }
}

using UnityEngine;

[RequireComponent(typeof(BoxCollider))] // Box Collider가 반드시 있어야 에러가 안 납니다.
public class ShootingTarget : MonoBehaviour
{
    [Header("타원 중심점 오프셋 (X점의 정확한 위치)")]
    [Tooltip("종이판 정중앙(0) 기준으로 빨간 X점의 상대 위치입니다. 미세 오차가 있다면 이 Y값을 수정하세요.")]
    public Vector2 centerOffset = new Vector2(0f, -0.015f); 

    private BoxCollider targetCollider;

    private void Awake()
    {
        // 오브젝트에 붙어있는 Box Collider를 자동으로 가져옵니다.
        targetCollider = GetComponent<BoxCollider>();
    }

    // 주원님이 세팅한 X: 0.3312098 / Y: 0.5731006 크기를 기반으로 타원형 점수를 계산합니다.
    public int CalculateEllipseScore(Vector3 hitPoint)
    {
        if (targetCollider == null) return 0;

        // 1. 레이캐스트가 맞은 월드 좌표를 이 사격지 보드의 로컬 좌표계로 변환
        Vector3 localHit = transform.InverseTransformPoint(hitPoint);

        // 2. 주원님이 인스펙터에서 직접 조절한 콜라이더의 Center(중심) 값과 Size(크기) 값을 실시간 반영
        Vector3 colliderCenter = targetCollider.center;
        Vector3 colliderSize = targetCollider.size;

        // 3. 실제 조준해야 하는 사격지 이미지 상의 빨간 X(10점) 중심점 계산
        // (깎아둔 콜라이더 중심점에서 이미지 미세 오프셋만큼 더해줍니다)
        float targetCenterX = colliderCenter.x + centerOffset.x;
        float targetCenterY = colliderCenter.y + centerOffset.y;

        // 4. 탄착 지점과 10점 중심점 사이의 순수한 가로(X), 세로(Y) 거리 측정
        float diffX = localHit.x - targetCenterX;
        float diffY = localHit.y - targetCenterY;

        // 5. ★핵심 타원 방정식 공식 대입★
        // 주원님이 구하신 가로(0.331)와 세로(0.573)의 절반 값을 반지름(Radius)으로 기준 잡고 비율을 구합니다.
        float radiusX = colliderSize.x * 0.5f;
        float radiusY = colliderSize.y * 0.5f;

        float normX = diffX / radiusX;
        float normY = diffY / radiusY;

        // 타원의 공식 (x^2 / a^2) + (y^2 / b^2) 실행
        // 이 결과값은 중심(10점)일 때 0에 수렴하고, 종이 외곽 테두리로 갈수록 1에 가까워집니다.
        float ellipseValue = (normX * normX) + (normY * normY);

        // 6. 주원님 사격지 이미지 격선 비율에 정확히 맞춘 타원형 점수 산출
        // 플레이어가 눈으로 보는 타원 격선 경계선 수치입니다.
        if (ellipseValue <= 0.025f) return 10; // X 구역 (빨간 타원 정중앙)
        if (ellipseValue <= 0.110f) return 9;  // 9점 타원 안쪽
        if (ellipseValue <= 0.310f) return 8;  // 8점 타원 안쪽
        if (ellipseValue <= 0.680f) return 7;  // 7점 타원 안쪽 (검은 실루엣 대부분)

        // 7점선보다 밖이거나 흰색 종이 여백, 쇠 고리를 맞추면 무조건 0점 처리
        return 0;
    }
}
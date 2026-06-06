# 아키텍처 문서 · VR Tactical Training

## 1. 멀티씬 코스 구조

4개 훈련을 각각 독립 씬으로 두고, `SceneManager.LoadScene`(씬 이름 기반)으로 연결한다. 빌드 세팅에 등록된 4개 씬:

```
GearScene → MineMap → ShootingScene → CQBScene → (종합 평가/Result)
```

씬을 분리한 이유:
- 모듈별로 팀원이 독립 개발 → `.unitypackage`로 통합하기 쉬움
- 한 씬의 무게(에셋·라이팅)가 다른 씬에 영향을 주지 않음
- 씬 전환 시 메모리 정리가 자연스러움

## 2. 씬 간 데이터 공유 — DontDestroyOnLoad 싱글톤

씬이 바뀌면 일반 오브젝트는 파괴되므로, 코스 전체에서 유지되어야 하는 상태는 두 싱글톤이 보관한다.

### TrainingScoreManager

```csharp
public class TrainingScoreManager : MonoBehaviour
{
    public static TrainingScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // gear/mine/shooting/cqb 점수 보관 → GetTotalScore() / 결과 등급
}
```

- 첫 씬에서 1회 생성, 이후 모든 씬에서 `Instance`로 접근
- 각 모듈은 자기 점수만 기록(`SetGearScore`, `AddMineScore`, …)
- 마지막에 `GetTotalScore()`(최대 400)로 합산, 등급 산출

### TrainingFlowManager

코스 진행(현재 모듈, 다음 씬), **전체 경과 시간(타이머)**, 완료 여부를 관리. 타이머는 첫 모듈 진입 시 시작되어 코스 종료까지 누적된다.

## 3. 씬 이동 — SceneDoor (카메라 근접 트리거)

클릭/레이 선택형 포털은 VR에서 불편해, **"문에 걸어 들어가면 자동 전환"** 방식으로 전환했다.

- `Update()`에서 `Camera.main`과 도어 사이의 **수평 거리**를 측정
- 거리 ≤ `triggerRadius` 상태가 `dwell`초 유지되면 `SceneManager.LoadScene(targetSceneName)`
- 플레이어 태그/리그 구조 변경이 필요 없어 팀원 씬에 **최소 침습**으로 부착 가능

```
[플레이어 카메라] ── 수평거리 ≤ 반경, dwell초 유지 ──▶ LoadScene(targetSceneName)
```

각 씬에는 다른 모듈로 가는 도어(GEAR/MINE/SHOOTING/CQB)와, 팀원 씬에서 코스로 복귀하는 `TrainingSceneReturn` 도어를 배치한다.

## 4. VR 입력 구조

- **OpenXR + XR Interaction Toolkit 3.1.2** 기반. XR Origin(XR Rig) 아래 Left/Right Controller에 Tracked Pose Driver, Near-Far Interactor(주 레이), Poke/Teleport Interactor, Controller Visual(모델) 구성.
- **XR Input Modality Manager**가 컨트롤러/핸드 모델을 전환.
- OpenXR 인터랙션 프로파일에 **HTC Vive Controller Profile**을 활성화해 Vive 완드를 인식.
- CQB 사격은 Input Action을 컨트롤러 트리거(`<XRController>{RightHand}/triggerButton`)에 바인딩하고, 데스크톱 테스트용으로 마우스/스페이스 폴백을 둠.

## 5. 모듈별 점수 규칙 요약

| 모듈 | 점수 규칙 |
| --- | --- |
| Gear | 기본 100 · 장비 누락 −15 · 시간 초과 10초당 −10 (제한 1분) |
| Mine | 기본 100 · 지뢰 접촉 −15 · 시간 초과 10초당 −10 (제한 1분) |
| Shooting | 10발, 과녁 중심 거리별 1~10점 → 100점 환산 |
| CQB | 적 처치 +10 · 시야 노출 초당 −5 · 인질 사격 −5 · 시간 초과 20초당 −10 (제한 2분) |
| **종합** | 4개 합산 **400점**, 등급 산출 후 Result 표시 |

## 6. 알려진 환경 이슈

- **XR Device Simulator**: PC 테스트용. 실기기 빌드/플레이 시 반드시 비활성화(실제 컨트롤러 입력을 가로챔).
- **Burst 컴파일 경고**(glTFast·XR Toolkit): macOS/Metal 환경의 알려진 이슈로, 매니지드 폴백으로 동작하므로 플레이에는 영향 없음.
- **대용량 FBX(108MB)**: GitHub 100MB 제한 → Git LFS 필요.
- **지뢰 가시성**: `Mine.startsHidden = true`로 런타임에 숨겨지며(탐지 전), 모델이 납작(약 45×6.6cm)해 의도적으로 눈에 잘 띄지 않음 — 정상 동작.

# 예비군 훈련 시뮬레이션 (VR Tactical Training)

> **VR 기반 예비군 전술훈련 시뮬레이터** — 군장 착용·지뢰 탐지·사격·CQB 4개 훈련을 하나의 코스로 연결하고, 전 과정을 점수화·시간 측정하여 객관적으로 평가하는 HTC Vive Pro용 시뮬레이션.

실제 예비군 훈련은 소총탄·폭발물 등으로 사고 위험과 비용이 크다는 문제의식에서 출발했습니다. 위험·비용 없이 **반복 가능하고 장소·시간에 구애받지 않는** VR 훈련 환경을 만들고, 훈련 결과를 데이터로 남겨 학습 효과를 높이는 것을 목표로 했습니다.

---

## ℹ️ Information

| 항목 | 내용 |
| --- | --- |
| **개발 기간** | 2026.05.06 ~ 2026.06.08 (약 5주) |
| **팀 구성** | 4인 (이성수 외 3인) |
| **담당 업무** | **군장(Gear) 훈련 모듈 개발** · **멀티씬 시스템 통합·빌드 통합** (씬 이동·점수·타이머·결과 집계 아키텍처) · **통합 후 최종 버그 수정·QA** |
| **기술 스택** | C# · Unity 2022.3.62f3 LTS · URP · OpenXR · XR Interaction Toolkit |
| **플랫폼** | HTC Vive Pro (OpenXR + SteamVR) |
| **형상관리** | Git / GitHub |
| **배포 / 저장소** | `github.com/hoshinoresorts1/VR-Tactical-Training` |

### 기술 스택 상세

- **Engine**: Unity `2022.3.62f3` LTS
- **Rendering**: Universal Render Pipeline `14.0.12`
- **XR**: OpenXR `1.14.3`, XR Interaction Toolkit `3.1.2`, XR Management `4.5.1`
- **Input**: Input System `1.14.2` (Action 기반, VR 컨트롤러 바인딩)
- **Asset Pipeline**: glTFast `6.14.1` (glb/gltf 임포트), ProBuilder `5.2.4` (맵 모델링)
- **Device**: HTC Vive Pro / XR Device Simulator(PC 테스트용)

---

## 📝 Review

**배운 것**

- XR Interaction Toolkit 3.x의 상호작용 구조(Ray·Near-Far·Poke Interactor, Tracked Pose Driver, Input Modality Manager)와 OpenXR 인터랙션 프로파일을 실제 기기 기준으로 이해하게 됐습니다.
- 4명이 각자 만든 모듈을 **하나의 빌드로 통합**하면서, 멀티씬 아키텍처와 씬 간 데이터 공유(`DontDestroyOnLoad` 싱글톤) 설계를 직접 경험했습니다.

**성과**

- 군장 훈련 모듈을 **레이+트리거 상호작용 → Humanoid 본(bone)에 장비 부착 → 체크리스트·점수 연동**까지 완결된 형태로 구현.
- 흩어져 있던 4개 모듈을 **로그인→군장→지뢰→사격→CQB→종합 평가(400점)→이수**의 단일 코스로 연결하고, 전 과정 공유 타이머와 점수 집계 시스템을 완성.
- 통합 후 실기기에서 발생한 VR 버그(컨트롤러 미표시·CQB 클리어 후 이동 불가 등)를 직접 추적·수정해 **최종 빌드 안정화**까지 마무리.

**아쉬웠던 점 / 개선 방향**

- 팀원이 Git에 익숙하지 않아 `.unitypackage` 수동 통합 방식을 썼는데, 초기에 브랜치·LFS 전략을 합의했다면 통합 비용을 더 줄일 수 있었습니다.
- 결과(Result) 씬과 메인 허브의 점수 UI를 더 다듬지 못한 점이 아쉬워, 다음 이터레이션 과제로 남겨두었습니다.

---

# 프로젝트 진행 과정

## 01. 요구사항 정의

**배경**

- 2026년 5월, 경기 포천 예비군 훈련장에서 발생한 사망 사고 등으로 **훈련 안전관리·위험 저감**의 필요성이 사회적으로 제기됨.
- 실탄 사격·폭발물 취급 훈련은 실제 환경에서 사고 위험이 상존 → 동일 환경을 **반복 훈련 가능한 VR**로 대체할 필요.
- 선행 연구상 VR/AR 기반 훈련 체계는 전장과 유사한 실전 효과를 내면서 사고 예방·예산 절감에 기여한다고 평가됨.

**필요성 / 목적**

| 필요성 | 목적 |
| --- | --- |
| 실제 훈련장 구축 비용 절감 | VR 환경에서 예비군 전술훈련 제공 |
| 안전사고 위험 없이 반복 훈련 | 다양한 전투 상황 체험 및 대응 능력 향상 |
| 장소·시간에 구애받지 않는 교육 | 훈련 결과를 점수화하여 학습 효과 증대 |
| 훈련 데이터를 활용한 객관적 평가 | 전술 행동·상황 판단 능력 향상 |

## 02. 시스템 · 씬 플로우 설계

VR 기반 예비군 훈련 시뮬레이션 시스템의 전체 흐름:

```
로그인/시작 → 군장 착용 → 지뢰 탐지 → 사격 → CQB 전술 → 종합 평가 → 훈련 이수
   (시작)      GearScene   MineMap   Shooting   CQBScene   (400점 집계)  (Result)
```

- 각 훈련은 **독립된 Unity 씬**으로 구성하고, 플레이어가 씬 안의 **도어(Door)에 걸어 들어가면 다음 씬으로 전환**되는 방식으로 연결.
- 씬을 넘나들어도 **누적 점수·전체 타이머가 유지**되도록 씬 비파괴(`DontDestroyOnLoad`) 매니저로 상태를 공유.

| 씬 | 역할 | 최대 점수 |
| --- | --- | --- |
| `GearScene` | 군장 착용 훈련 | 100 |
| `MineMap` | 지뢰 탐지 훈련 | 100 |
| `ShootingScene` | 사격 훈련 | 100 |
| `CQBScene` | 근거리 전투(CQB) 훈련 | 100 |
| (종합 평가) | 4개 모듈 합산 | **400** |

## 03. 데이터 · 상태 관리 설계

웹 프로젝트의 DB 설계에 대응하는, 본 프로젝트의 **런타임 상태 관리 설계**입니다. 씬이 바뀌어도 데이터가 유지되어야 하므로 두 개의 싱글톤을 두었습니다.

- **`TrainingScoreManager`** — 모듈별 점수(`gear/mine/shooting/cqb`)를 보관하고 총점·결과 등급을 산출하는 싱글톤. 첫 씬에서 생성되어 `DontDestroyOnLoad`로 코스 전체에 걸쳐 유지됨.
- **`TrainingFlowManager`** — 코스 진행(현재 모듈, 다음 씬), 전체 경과 시간(타이머), 완료 여부를 관리.

```
[GearScene]      TrainingScoreManager.SetGearScore(100)  ─┐
[MineMap]        AddMineScore(...)                         │  DontDestroyOnLoad로
[ShootingScene]  AddShootingScore(...)                     ├─ 씬 전환에도 유지
[CQBScene]       AddCQBScore(...)                         ─┘
                          ↓
              GetTotalScore() / 결과 등급 산출 → Result
```

> 점수 등급 예: 90↑ 조기퇴근 · 70↑ 훈련 통과 · 50↑ 재훈련 권고 · 그 외 얼차려

## 04. 담당 기능 개발

### 🎖️ 군장(Gear) 훈련 모듈

VR 컨트롤러로 개인 전투 장비(**Helmet · Vest · Belt · Grenade · Bag**)를 집어 캐릭터에 착용하고, 모든 장비를 정확히 착용하면 다음 훈련으로 넘어가는 모듈입니다.

**훈련 내용**

1. 개인 전투 장비 착용 절차를 VR 환경에서 학습
2. VR 컨트롤러(레이 + 트리거)로 장비를 선택·이동·착용하며 기본 조작법 습득
3. 모든 장비를 정확히 착용하면 다음 훈련으로 이동

**점수 기준**

- 기본 점수 100점 / 제한 시간 1분
- 장비 누락: −15점 · 시간 초과: 10초당 −10점

**구현 포인트** — 별도의 슬롯 오브젝트를 두지 않고, 장비가 가야 할 **Humanoid 본을 캐릭터 Animator에서 찾아 직접 부착**하는 방식으로 단순화했습니다. 모든 장비 착용이 완료되는 순간 1회만 점수를 부여합니다.

```csharp
// GearEquipManager.cs — 레이+트리거로 선택된 장비를 해당 Humanoid 본에 부착
public bool RequestEquip(GearItem item)
{
    if (item == null || item.IsEquipped) return false;
    if (IsEquipped(item.GearType)) return false;

    EnsureAnimatorCached();
    if (characterAnimator == null || !characterAnimator.isHuman) return false;

    Transform bone = characterAnimator.GetBoneTransform(item.AttachBone);
    if (bone == null) return false;

    item.EquipToBone(bone, item.LocalPositionOffset, item.LocalEulerOffset, this);
    equippedState[item.GearType] = true;
    NotifyStateChanged();
    AwardScoreIfComplete();   // 모든 장비 착용 시 1회만 점수 부여
    return true;
}
```

체크리스트 UI(`GearChecklistUI`)는 장비별 행 버튼으로 착용 상태(`[X]/[ ]`)를 표시하고, 레이로 클릭 시 해당 장비 해제·`UNEQUIP ALL` 일괄 해제를 지원합니다.

| 주요 스크립트 | 역할 |
| --- | --- |
| `GearEquipManager` | 착용/해제 로직, 본 부착, 완료 시 점수 부여 |
| `GearChecklistUI` | 장비별 체크리스트·해제 버튼 UI |
| `GearItem` / `GearType` | 장비 오브젝트·부착 본·장비 종류(enum) |
| `GearSuccessBanner` | 전 장비 착용 완료 시 성공 UI |

### 🔗 멀티씬 시스템 통합 (Common)

4개 모듈을 하나의 코스로 묶는 공통 기반을 설계·구현했습니다.

| 주요 스크립트 | 역할 |
| --- | --- |
| `SceneDoor` | **카메라 근접 트리거형 도어** — 플레이어가 걸어 들어가면 목표 씬 로드(클릭 불필요) |
| `TrainingScoreManager` | 모듈별 점수 보관·총점·결과 등급 (DontDestroyOnLoad) |
| `TrainingFlowManager` | 코스 진행·전체 타이머·완료 상태 관리 |
| `TrainingSceneReturn` | 팀원 씬에서 코스로 복귀하는 도어 |

> **씬 이동 방식**: 초기엔 포털을 클릭/레이 선택으로 이동하게 했으나 VR에서 불편 → **"문처럼 걸어 들어가면 자동 전환"** 되는 `SceneDoor`(카메라와의 수평 거리 ≤ 반경, 일정 시간 머무르면 `SceneManager.LoadScene`)로 개선해 몰입감과 편의성을 높였습니다.

## 05. 형상관리

- **저장소**: `github.com/hoshinoresorts1/VR-Tactical-Training`
- **브랜치 전략**: 모듈별 기능 브랜치로 작업 (담당: `feature/gear`) 후 통합.
- **대용량 바이너리**: 108MB FBX 등 GitHub 100MB 제한을 넘는 에셋은 **Git LFS**로 관리(`*.fbx`, `*.psd`, `*.tga` 등).
- **Unity용 `.gitignore`**: `Library/`, `Temp/`, `Logs/`, `UserSettings/` 등 재생성 폴더 제외.

> **팀 통합 방식**: 팀원이 Git에 익숙하지 않아, 각 모듈 씬을 `.unitypackage`로 내보내 통합 담당(본인)이 임포트·병합하는 방식을 택했습니다. 이 과정에서 GUID 충돌·중복 스크립트 문제를 직접 해결했습니다(아래 99. 이슈 참고).

## 06. 발표 자료

- 팀 발표 PPT: `가상현실 3팀_예비군 훈련 시뮬레이션.pdf` (Notion에 임베드)
- 시연: 군장 착용 → 지뢰 탐지 → 사격 → CQB → 종합 평가 코스 플레이

---

# 99. 개발 중 이슈

## 🩹 협업 · 형상관리의 어려움 — `.unitypackage` 통합과 GUID 충돌

팀원들이 Git을 어려워해 모듈을 `.unitypackage`로 받아 통합했는데, 새 패키지가 기존 프로젝트와 **같은 클래스를 다른 경로에 추가**하면서 `CS0101`(중복 정의) 컴파일 에러가 발생했습니다.

- **원인**: 옛 임포트의 잔재(`Prefabs/Shooting/*.cs`)와 새 임포트(`Scripts/Shooting/*.cs`)에 동일 클래스가 공존.
- **해결**: 새 씬이 참조하는 GUID를 추적해, 어디서도 참조되지 않는 옛 스크립트만 안전하게 제거 → 컴파일 정상화. 점수 매니저 등 핵심 에셋은 GUID를 보존해 씬 참조가 깨지지 않도록 처리.

## 🛠 1GB 패키지 임포트 시 Unity 크래시 → 디스크 직접 재구성으로 우회

최종 통합본(약 1GB, 에셋 1,563개)을 Unity의 `Import Package`로 불러오는 순간 **에디터가 반복적으로 크래시**했습니다(대용량 일괄 임포트 중 메인 프로세스 다운).

- **분석**: 임포트 워커 로그상 특정 에셋이 아니라 메인 프로세스가 임포트 도중 종료 → 일괄 임포트의 메모리/안정성 한계로 판단.
- **해결**: `.unitypackage`가 GUID 폴더마다 `pathname`·`asset`·`asset.meta`를 담은 tar.gz임을 이용해, **임포터를 거치지 않고 디스크에서 직접 최종 Assets 구조로 재구성**. 체크섬 비교로 실제로 바뀐 파일만 반영(결과적으로 단 1개 파일 차이임을 확인)하여 크래시를 완전히 우회하고 무결성도 검증했습니다.

## ⏲ 실기기 테스트 디버깅 — VR 컨트롤러/이동 관련 버그

PC(시뮬레이터)에선 정상인데 **실제 Vive에서만 발생**하는 문제들을 추적·해결했습니다.

- **컨트롤러·레이가 안 보임**: 씬에 **XR Device Simulator가 활성 상태**로 남아 실제 컨트롤러 입력을 가로채던 것이 원인 → 4개 씬 모두에서 시뮬레이터를 비활성화해 실기기 트래킹 복구.
- **CQB 클리어 후 이동 불가**: 미션 완료 시 `Time.timeScale = 0`(전체 정지)이 걸려 결과창은 떠도 **VR 로코모션이 멈춰 도어로 못 가던** 버그 → 정지 로직을 제거해 클리어 후에도 이동 가능하도록 수정.

---

> 📌 *이 문서는 Notion 포트폴리오용으로 작성되었습니다. 표·코드 블록·콜아웃은 Notion 마크다운 임포트 시 자동 변환됩니다. 스크린샷(군장 체크리스트 UI, 지뢰 탐지기, 사격장, CQB)과 GitHub·발표자료 링크를 각 섹션에 임베드하면 완성됩니다.*

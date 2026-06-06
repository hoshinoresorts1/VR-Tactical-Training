# 예비군 훈련 시뮬레이션 · VR Tactical Training

VR 기반 예비군 전술훈련 시뮬레이터. **군장 착용 → 지뢰 탐지 → 사격 → CQB** 4개 훈련을 하나의 코스로 연결하고, 전 과정을 점수화(총 400점)·시간 측정하여 객관적으로 평가합니다. HTC Vive Pro(OpenXR) 대상.

실탄·폭발물 등으로 사고 위험과 비용이 큰 실제 예비군 훈련을, 위험·비용 없이 반복 가능한 VR 환경으로 대체하는 것을 목표로 합니다.

## 기술 스택

| 영역 | 사용 기술 |
| --- | --- |
| Engine | Unity `2022.3.62f3` LTS (VR Basic 템플릿) |
| Rendering | Universal Render Pipeline `14.0.12` |
| XR | OpenXR `1.14.3`, XR Interaction Toolkit `3.1.2`, XR Management `4.5.1` |
| Input | Input System `1.14.2` |
| Asset | glTFast `6.14.1`, ProBuilder `5.2.4` |
| Platform | HTC Vive Pro (OpenXR + SteamVR) |

## 훈련 코스

```
로그인/시작 → 군장 착용 → 지뢰 탐지 → 사격 → CQB 전술 → 종합 평가(400점) → 훈련 이수
```

| 씬 | 모듈 | 핵심 내용 | 점수 |
| --- | --- | --- | --- |
| `GearScene` | 군장 착용 | 장비 5종(Helmet/Vest/Belt/Grenade/Bag) 착용 | 100 |
| `MineMap` | 지뢰 탐지 | 탐지기로 지뢰 탐색·제거, 접촉 시 감점 | 100 |
| `ShootingScene` | 사격 | 과녁 10발 사격, 중심 근접도로 점수 | 100 |
| `CQBScene` | 근거리 전투 | 적 처치/시야 노출/인질 오인 사격 평가 | 100 |

## 실행 방법

### PC (XR Device Simulator)
1. Unity `2022.3.62f3`로 프로젝트 열기
2. `Assets/_Project/Scenes/GearScene.unity` 열고 Play
3. 마우스 우클릭+드래그로 시점, 키보드/마우스로 컨트롤러 시뮬레이션

### VR (HTC Vive Pro)
1. SteamVR 실행 (OpenXR 런타임으로 설정)
2. **각 씬의 XR Device Simulator 오브젝트는 비활성화** (실기기 입력과 충돌)
3. `GearScene`에서 Play → 도어로 걸어 들어가며 다음 모듈로 이동

## 문서
- 포트폴리오: [`docs/PORTFOLIO.md`](docs/PORTFOLIO.md)
- 아키텍처: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)

---

# 팀 협업 규칙 (Collaboration Rules)

## Project Info
- Unity Version: 2022.3.62f3 LTS
- Template: VR Basic
- Target Device: HTC Vive Pro Controller
- Collaboration: GitHub + separated feature scenes/prefabs

## Team Roles
- Gear Equipment: 이성수
- Shooting Training & Grenade Throwing: 최주원
- CQB: 김선우
- Mine Detection & Defusal: 길현우

## Branch Rules
- main: stable integration branch
- feature/gear: Gear Equipment
- feature/shooting: Shooting & Grenade
- feature/cqb: CQB
- feature/mine: Mine Detection

## Scene Rules
- MainScene is only for final integration.
- Each member works only in their own feature scene and feature folders.
- Do not edit another member's scene without discussion.
- Each feature should be converted into a prefab before final integration.

## Folder Rules
- Gear: Assets/_Project/Scripts/Gear, Assets/_Project/Prefabs/Gear
- Shooting: Assets/_Project/Scripts/Shooting, Assets/_Project/Prefabs/Weapons
- CQB: Assets/_Project/Scripts/CQB, Assets/_Project/Prefabs/CQB
- Mine: Assets/_Project/Scripts/Mine, Assets/_Project/Prefabs/Mine
- Common code: Assets/_Project/Scripts/Common
- UI code: Assets/_Project/Scripts/UI

## Git Rules
- Pull before starting work.
- Commit small changes often.
- Create a Pull Request when a feature is ready.
- Do not commit Library, Temp, Obj, Logs, UserSettings, Build, or Builds.
- Keep Unity version fixed at 2022.3.62f3.
- 100MB+ binaries (e.g. Beard_man.fbx) use Git LFS: `git lfs track "*.fbx" "*.psd" "*.tga"`

## Unity Settings
- Asset Serialization: Force Text
- Visible Meta Files enabled

## Final Integration
Each feature owner should provide:
- Feature Scene
- Feature Prefab
- Required scripts
- Short test instructions

# VR Tactical Training System

## 개요

**VR Tactical Training System**은 경찰 및 군사 훈련을 위한 VR 기반 시뮬레이션입니다.
4가지 훈련 모듈로 구성되며, 각 팀원이 독립적으로 개발합니다.

```
VR Tactical Training
├── 군장 착용 (Gear) - 이성수
├── 사격 & 수류탄 (Shooting) - 최주원
├── CQB (Close Quarter Battle) - 김선우
└── 지뢰 탐지 (Mine) - 길현우
```

---

## 주요 기능

### 1️⃣ 군장 착용 (Gear Training)
- 헬멧, 방탄조끼, 탄띠 등 장비 착용
- 착용 체크리스트 UI
- 모든 장비 완성 시 다음 훈련 진행

### 2️⃣ 사격 & 수류탄 (Shooting Training)
- 실시간 표적 쏘기
- 이동 표적 대응
- 수류탄 투척 및 폭발 메커니즘

### 3️⃣ CQB 훈련 (Close Quarter Battle)
- 어두운 공간 탐색
- 손전등 활용
- 적/민간인/목표 구분
- 조건부 감점 시스템

### 4️⃣ 지뢰 탐지 (Mine Detection)
- 지뢰 탐지기 조작
- 미니게임 기반 해제
- 탐지 및 해제 성공률 평가

---

## 시스템 요구사항

- **Unity**: 2022.3 LTS 이상
- **VR Headset**: HTC Vive / Meta Quest (OpenXR 지원)
- **OS**: Windows 10 이상
- **RAM**: 8GB 이상
- **GPU**: RTX 2070 이상 권장

---

## 프로젝트 구조

```
Assets/
├── _Project/
│   ├── Scenes/
│   │   ├── MainScene.unity          # 최종 통합 씬
│   │   ├── GearScene.unity          # 군장 착용
│   │   ├── ShootingScene.unity      # 사격/수류탄
│   │   ├── CQBScene.unity           # CQB 훈련
│   │   └── MineScene.unity          # 지뢰 탐지
│   │
│   ├── Scripts/
│   │   ├── Common/
│   │   │   ├── TrainingScoreManager.cs
│   │   │   └── VRManager.cs
│   │   ├── Gear/
│   │   ├── Shooting/
│   │   ├── CQB/
│   │   └── Mine/
│   │
│   ├── Prefabs/
│   │   ├── Gear/
│   │   ├── Weapons/
│   │   ├── CQB/
│   │   ├── Mine/
│   │   └── Common/
│   │
│   └── Models/
│       ├── Gear/
│       ├── Weapons/
│       └── Mine/
│
├── VRTemplateAssets/    # Unity VR Template
└── ...

.gitignore              # Git 무시 파일
COLLABORATION_GUIDE.md  # 팀 협업 가이드 (이 파일)
```

---

## 빠른 시작 가이드

### 1. 프로젝트 클론

```bash
git clone https://github.com/hoshinoresorts1/VR-Tactical-Training.git
cd VR-Tactical-Training
```

### 2. Unity에서 열기

1. Unity Hub 실행
2. "Add" → 프로젝트 폴더 선택
3. Unity 2022.3 LTS 이상으로 열기

### 3. VR 설정 확인

```
Edit > Project Settings > XR Plug-in Management
```

- ✅ OpenXR 활성화
- ✅ HTC Vive Interaction Profile 확인

### 4. 씬 실행 테스트

```
Assets/_Project/Scenes/MainScene.unity 더블클릭
Play 버튼 실행
```

---

## 팀 협업 방식

**자세한 협업 가이드는 [COLLABORATION_GUIDE.md](./COLLABORATION_GUIDE.md) 참고**

### 핵심 규칙

| 규칙 | 설명 |
|------|------|
| **브랜치 분리** | 각 팀원이 자신의 기능 브랜치에서만 작업 |
| **Scene 분리** | 같은 Scene을 동시에 수정하지 않음 |
| **Prefab 기반** | 완성된 기능은 Prefab으로 제작 |
| **통합 담당** | MainScene은 통합 담당자만 수정 |
| **Daily Pull** | 매일 작업 시작 전 `git pull origin main` |

---

## 개발 팀

| 역할 | 담당자 | 기능 | 상태 |
|------|--------|------|------|
| 군장 착용 | 이성수 | Gear | 📋 Ready |
| 사격/수류탄 | 최주원 | Shooting | 📋 Ready |
| CQB | 김선우 | CQB | 📋 Ready |
| 지뢰 탐지 | 길현우 | Mine | ✅ In Progress |
| 통합 담당 | TBD | MainScene | 📋 Ready |

---

## 브랜치 및 상태

| 브랜치 | 담당자 | 상태 |
|--------|--------|------|
| `main` | 통합 담당자 | 최종 통합용 |
| `feature/gear` | 이성수 | 준비 중 |
| `feature/shooting` | 최주원 | 준비 중 |
| `feature/cqb` | 김선우 | 준비 중 |
| `feature/mine` | 길현우 | ✅ 진행 중 |

---

## 점수 시스템

모든 훈련은 **100점 만점** 기준:

```
총점 = 군장 점수(25) + 사격 점수(25) + CQB 점수(25) + 지뢰 점수(25)
```

점수는 `TrainingScoreManager`를 통해 관리됩니다:

```csharp
TrainingScoreManager.Instance.AddShootingScore(10);  // 사격 +10점
TrainingScoreManager.Instance.GetTotalScore();        // 총점 조회
```

---

## 공통 태그 및 레이어

### Tags
```
Player, Target, Enemy, Civilian, Equipment, Mine, Door, Throwable, InteractableObject
```

### Layers
```
Player, Equipment, Target, Mine, Interactive
```

이 설정들은 모든 팀원이 동일하게 적용해야 합니다.

---

## 커밋 메시지 컨벤션

### 형식
```
[기능명] 구현 내용
```

### 예시
```
[Gear] 헬멧 착용 시스템 구현
[Shooting] 표적 Raycast 발사 시스템
[CQB] 손전등 전환 기능
[Mine] 지뢰 탐지 UI 업데이트
[Common] 점수 시스템 통합
```

---

## 자주 묻는 질문 (FAQ)

### Q1: Git 충돌이 발생했어요. 어떻게 하나요?

A: [COLLABORATION_GUIDE.md의 문제 해결](./COLLABORATION_GUIDE.md#문제-해결) 섹션을 참고하세요.

### Q2: 다른 팀원의 코드를 수정해야 해요.

A: 해당 팀원에게 먼저 알리고, 직접 수정하지 마세요. Pull Request로 요청하세요.

### Q3: Library 폴더가 Git에 올라가요.

A: `.gitignore`를 확인하세요. Library는 커밋하면 안 됩니다.

### Q4: 실수로 MainScene을 수정했어요.

A: 아래 명령으로 원상복구:
```bash
git checkout main -- Assets/_Project/Scenes/MainScene.unity
```

### Q5: Unity 버전이 다른데 괜찮을까요?

A: **안 됩니다.** 모두 Unity 2022.3 LTS 이상으로 통일해야 합니다.

---

## 문제 보고

버그나 개선사항은 **GitHub Issues**에서 보고해주세요:

```
제목: [Gear] 헬멧 충돌 감지 오류
본문: 
- 상황: GearScene에서 헬멧을 집으면 충돌
- 스택 트레이스: ...
- 예상 원인: Collider 설정
```

---

## 라이선스

이 프로젝트는 팀 내부 사용만 가능합니다.

---

## 참고 자료

- [Unity XR Plugin Management Docs](https://docs.unity3d.com/Manual/XRManagement.html)
- [OpenXR Specification](https://www.khronos.org/openxr/)
- [GitHub Collaboration Guide](https://docs.github.com/en/collaboration)

---

## 마지막 체크리스트

프로젝트 시작 전에 다음을 확인하세요:

```markdown
- [ ] Unity 2022.3 LTS 설치
- [ ] GitHub 저장소 Clone
- [ ] 자신의 Feature 브랜치 생성
- [ ] VR 설정 확인 (OpenXR)
- [ ] 자신의 Scene 파일 확인
- [ ] COLLABORATION_GUIDE.md 읽기
- [ ] 팀원과 슬랙/카톡 연락 확인
```

**모두 준비 완료되면, 행운을 빕니다! 🎮🚀**


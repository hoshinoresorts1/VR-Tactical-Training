# VR Tactical Training - 팀 협업 가이드

## 프로젝트 개요

**VR Tactical Training**은 경찰/군사 훈련을 위한 VR 시뮬레이션입니다.
각 팀원이 자신의 훈련 모듈을 독립적으로 개발하고, 최종적으로 통합합니다.

---

## 핵심 협업 규칙

### 1. 같은 Scene을 동시에 수정하지 않기
- 각자 자기 Scene 파일만 수정
- MainScene은 **통합 담당자만 수정**

### 2. 자기 폴더 안에서만 작업
```
Assets/_Project/
├── Scripts/
│   ├── Gear/         ← 이성수
│   ├── Shooting/     ← 최주원
│   ├── CQB/          ← 김선우
│   ├── Mine/         ← 길현우
│   └── Common/       ← 공동 (동의 후 수정)
├── Prefabs/
│   ├── Gear/
│   ├── Weapons/
│   ├── CQB/
│   └── Mine/
└── Models/
```

### 3. 매일 작업 시작 전에
```bash
git checkout feature/본인기능
git pull origin main
```

### 4. 작업 후 업로드
```bash
git add Assets/_Project/
git commit -m "[기능] 구현 내용"
git push origin feature/본인기능
```

### 5. Commit 메시지 규칙
```
좋은 예:
[Gear] 군장 착용 UI 구현
[Shooting] 표적 Raycast 시스템
[Mine] 지뢰 탐지 미니게임

나쁜 예:
수정
업데이트
asdf
```

---

## 브랜치 및 담당자

| 역할 | 담당자 | 브랜치 | Scene | 폴더 |
|------|--------|--------|-------|------|
| 군장 착용 | 이성수 | `feature/gear` | GearScene.unity | Scripts/Gear/ |
| 사격 & 수류탄 | 최주원 | `feature/shooting` | ShootingScene.unity | Scripts/Shooting/ |
| CQB | 김선우 | `feature/cqb` | CQBScene.unity | Scripts/CQB/ |
| 지뢰 탐지 | 길현우 (현재) | `feature/mine` | MineScene.unity | Scripts/Mine/ |
| 통합 담당 | TBD | `main` | MainScene.unity | - |

---

## 각 담당자의 구현 범위

### 이성수 — 군장 착용 (Gear)
**Scene**: `Assets/_Project/Scenes/GearScene.unity`

필수 구현:
- 장비 오브젝트 배치 (헬멧, 방탄조끼, 탄띠 등)
- Grab/Ray 선택 방식
- 장비 착용 처리
- 착용 체크리스트 UI
- 모든 장비 착용 시 다음 문 개방

최종 산출물:
```
GearTrainingArea.prefab
Scripts/Gear/GearManager.cs
Prefabs/Gear/Equipment/
```

### 최주원 — 사격 & 수류탄 (Shooting)
**Scene**: `Assets/_Project/Scenes/ShootingScene.unity`

필수 구현:
- 총 잡기/선택
- Trigger 입력 → Raycast 발사
- 표적 명중 판정 및 점수
- 이동 표적 시스템
- 수류탄 잡기/던지기/폭발
- 폭발 범위 내 표적 제거

최종 산출물:
```
ShootingTrainingArea.prefab
Scripts/Shooting/ShootingManager.cs
Prefabs/Weapons/
```

### 김선우 — CQB (Close Quarter Battle)
**Scene**: `Assets/_Project/Scenes/CQBScene.unity`

필수 구현:
- 어두운 공간 환경
- 손전등 켜기/끄기
- 문 열기/닫기
- 적/민간인/목표물 구분 표시
- 잘못된 선택 시 감점
- 적에게 노출되면 시간 비례 감점

최종 산출물:
```
CQBTrainingArea.prefab
Scripts/CQB/CQBManager.cs
Prefabs/CQB/
```

### 길현우 — 지뢰 탐지 (Mine) [현재 작업 중]
**Scene**: `Assets/_Project/Scenes/MineScene.unity`

필수 구현:
- 지뢰 탐지기 들기
- 지뢰 근처에서 UI/소리 반응
- 지뢰 선택 및 탐지
- 3버튼 순서 미니게임
- 성공 시 해제/실패 시 폭발

최종 산출물:
```
MineTrainingArea.prefab
Scripts/Mine/MineGrid.cs
Prefabs/Mine/
```

---

## 공통으로 맞춰야 하는 것

### 1. 점수 시스템 (공동 구현)
**Location**: `Assets/_Project/Scripts/Common/TrainingScoreManager.cs`

```csharp
public class TrainingScoreManager : MonoBehaviour
{
    public static TrainingScoreManager Instance;
    
    public int gearScore;
    public int shootingScore;
    public int cqbScore;
    public int mineScore;
    
    public void AddGearScore(int amount) { gearScore += amount; }
    public void AddShootingScore(int amount) { shootingScore += amount; }
    public void AddCQBScore(int amount) { cqbScore += amount; }
    public void AddMineScore(int amount) { mineScore += amount; }
    
    public int GetTotalScore() 
    { 
        return gearScore + shootingScore + cqbScore + mineScore; 
    }
}
```

각 기능 담당자는 자기 점수만 추가:
```csharp
TrainingScoreManager.Instance.AddShootingScore(10);
```

### 2. Unity 태그 (전체 통일)
```
Player
Target
Enemy
Civilian
Equipment
Mine
Door
Throwable
InteractableObject
```

### 3. Layer 설정
```
Default
TransparentFX
Ignore Raycast
Water
UI
Player
Equipment
Target
Mine
Interactive
```

### 4. Unity 버전
**모두 동일하게 유지**: Unity 2022 LTS 이상

### 5. VR 설정
**Edit > Project Settings > XR Plug-in Management**
- OpenXR 활성화
- HTC Vive Interaction Profile 확인

---

## GitHub 시작하기

### 첫 번째 클론 (모든 팀원)

```bash
git clone https://github.com/hoshinoresorts1/VR-Tactical-Training.git
cd VR-Tactical-Training
```

### 자신의 브랜치 생성

#### 이성수
```bash
git checkout -b feature/gear
git push -u origin feature/gear
```

#### 최주원
```bash
git checkout -b feature/shooting
git push -u origin feature/shooting
```

#### 김선우
```bash
git checkout -b feature/cqb
git push -u origin feature/cqb
```

#### 길현우 (이미 진행 중)
```bash
# 이미 mine 브랜치가 있으므로
git checkout mine
git pull origin main
```

---

## 매일 작업 흐름

### 1. 작업 시작
```bash
git checkout feature/본인기능
git pull origin main  # 다른 팀원의 변경사항 받기
```

### 2. Unity에서 자신의 Scene만 수정
```
Assets/_Project/Scenes/본인Scene.unity
Assets/_Project/Scripts/본인기능/
Assets/_Project/Prefabs/본인기능/
```

### 3. 변경사항 확인
```bash
git status
```

### 4. 커밋 및 푸시
```bash
git add Assets/_Project/
git commit -m "[본인기능] 구현 내용"
git push origin feature/본인기능
```

### 5. 기능 완성 후 Pull Request
GitHub에서 PR 생성:
- Title: `[Gear] 군장 착용 시스템 구현`
- Description:
  ```markdown
  ## 구현 내용
  - 헬멧, 방탄조끼, 탄띠 착용 기능
  - 착용 체크리스트 UI
  - 모든 장비 착용 시 다음 문 개방
  
  ## 테스트 방법
  1. GearScene 실행
  2. 장비 선택
  3. 체크 여부 확인
  ```

---

## Pull Request 프로세스

### PR 작성자
1. 기능 완성 후 `feature/브랜치`에서 PR 생성
2. PR 제목: `[기능명] 구현 내용`
3. 상세 설명 작성

### 통합 담당자
1. PR 검토
2. 코드 확인 및 테스트
3. Approve 및 Merge to main
4. 필요 시 MainScene에 Prefab 배치

---

## 충돌 방지 체크리스트

```markdown
- [ ] 자신의 Scene만 수정했는가?
- [ ] 자신의 폴더 안에서만 작업했는가?
- [ ] MainScene은 건드리지 않았는가?
- [ ] Common 폴더는 팀원에게 알리고 수정했는가?
- [ ] 작업 시작 전 `git pull origin main`을 했는가?
- [ ] .gitignore에 해당하는 파일은 커밋하지 않았는가?
- [ ] Commit 메시지는 명확한가?
```

---

## 문제 해결

### Git 충돌이 발생했을 때
```bash
git status  # 충돌 파일 확인
# 파일 편집 후
git add Assets/_Project/
git commit -m "Resolve merge conflict"
git push origin feature/본인기능
```

### 실수로 MainScene을 수정했을 때
```bash
git checkout main -- Assets/_Project/Scenes/MainScene.unity
git commit -m "Revert MainScene"
git push origin feature/본인기능
```

### 최신 main 내용을 받고 싶을 때
```bash
git pull origin main
# 혹은
git fetch origin
git rebase origin/main
git push -f origin feature/본인기능  # 주의: 로컬 커밋 덮어씀
```

---

## 최종 통합 (통합 담당자용)

모든 팀원의 PR이 Merge되면:

```bash
git checkout main
git pull origin main

# MainScene에 각 Prefab 배치
# GearTrainingArea
# ShootingTrainingArea
# CQBTrainingArea
# MineTrainingArea
# ResultUI

# MainScene 최종 테스트 후
git add Assets/_Project/Scenes/MainScene.unity
git commit -m "Integrate all training modules"
git push origin main
```

---

## 연락처 및 FAQ

### Unity 버전 맞추기
모두 같은 버전 사용 필수: **Unity 2022.3.x LTS**

### 에셋 추가 시
- 팀원에게 먼저 공유
- 큰 에셋은 용량 확인 후 추가
- Assets/_Project 폴더에만 배치

### 소통 채널
- Slack/카톡: 실시간 이슈 공유
- GitHub Issues: 버그 및 개선사항 추적

---

## 요약

| 항목 | 내용 |
|------|------|
| **Repository** | https://github.com/hoshinoresorts1/VR-Tactical-Training |
| **Main Branch** | 최종 통합용 (통합 담당자만 수정) |
| **Feature Branches** | feature/gear, feature/shooting, feature/cqb, feature/mine |
| **Project Structure** | Assets/_Project 하위에서만 작업 |
| **Scene Separation** | 각자 자신의 Scene만 수정 |
| **Commit Rule** | [기능명] 형식으로 명확하게 |
| **Integration** | PR → Review → Merge to main |

---

**마지막 주의**: Library, Temp, Build, Logs, UserSettings 폴더는 절대 Git에 올리지 않습니다. (.gitignore 확인)

행운을 빕니다! 🚀

# 브랜치별 빠른 시작 가이드

## 이성수 — 군장 착용 (Gear)

```bash
# 1. 프로젝트 클론 (처음 한 번만)
git clone https://github.com/hoshinoresorts1/VR-Tactical-Training.git
cd VR-Tactical-Training

# 2. feature/gear 브랜치 생성 (처음 한 번만)
git checkout -b feature/gear
git push -u origin feature/gear

# 3. 매일 작업 시작 전
git checkout feature/gear
git pull origin main

# 4. Unity에서 작업 위치
Assets/_Project/Scenes/GearScene.unity
Assets/_Project/Scripts/Gear/
Assets/_Project/Prefabs/Gear/

# 5. 작업 후 커밋
git add Assets/_Project/
git commit -m "[Gear] 군장 착용 시스템 구현"
git push origin feature/gear

# 6. GitHub에서 Pull Request 생성
# Title: [Gear] 군장 착용 시스템 구현
# Description: 헬멧, 방탄조끼, 탄띠 착용 기능 추가
```

---

## 최주원 — 사격 & 수류탄 (Shooting)

```bash
# 1. 프로젝트 클론 (처음 한 번만)
git clone https://github.com/hoshinoresorts1/VR-Tactical-Training.git
cd VR-Tactical-Training

# 2. feature/shooting 브랜치 생성 (처음 한 번만)
git checkout -b feature/shooting
git push -u origin feature/shooting

# 3. 매일 작업 시작 전
git checkout feature/shooting
git pull origin main

# 4. Unity에서 작업 위치
Assets/_Project/Scenes/ShootingScene.unity
Assets/_Project/Scripts/Shooting/
Assets/_Project/Prefabs/Weapons/

# 5. 작업 후 커밋
git add Assets/_Project/
git commit -m "[Shooting] 표적 Raycast 시스템 구현"
git push origin feature/shooting

# 6. GitHub에서 Pull Request 생성
# Title: [Shooting] 사격 및 수류탄 훈련 구현
# Description: Raycast 발사, 수류탄 투척 및 폭발 기능 추가
```

---

## 김선우 — CQB (Close Quarter Battle)

```bash
# 1. 프로젝트 클론 (처음 한 번만)
git clone https://github.com/hoshinoresorts1/VR-Tactical-Training.git
cd VR-Tactical-Training

# 2. feature/cqb 브랜치 생성 (처음 한 번만)
git checkout -b feature/cqb
git push -u origin feature/cqb

# 3. 매일 작업 시작 전
git checkout feature/cqb
git pull origin main

# 4. Unity에서 작업 위치
Assets/_Project/Scenes/CQBScene.unity
Assets/_Project/Scripts/CQB/
Assets/_Project/Prefabs/CQB/

# 5. 작업 후 커밋
git add Assets/_Project/
git commit -m "[CQB] 손전등 및 환경 상호작용"
git push origin feature/cqb

# 6. GitHub에서 Pull Request 생성
# Title: [CQB] CQB 훈련 모듈 구현
# Description: 어두운 공간, 손전등, 적/민간인 구분 기능
```

---

## 길현우 — 지뢰 탐지 (Mine) [현재 작업]

```bash
# 1. 프로젝트는 이미 클론됨
cd "c:\Users\user\My project (2)"

# 2. mine 브랜치 확인
git branch -a
# feature/mine 또는 mine이 보여야 함

# 3. 매일 작업 시작 전
git checkout mine
# 또는
git checkout feature/mine
git pull origin main

# 4. Unity에서 작업 위치
Assets/_Project/Scenes/MineScene.unity
Assets/_Project/Scripts/Mine/
Assets/_Project/Prefabs/Mine/

# 5. 작업 후 커밋
git add Assets/_Project/
git commit -m "[Mine] 지뢰 탐지 UI 업데이트"
git push origin mine
# 또는 feature/mine

# 6. 기능 완성 후 GitHub에서 Pull Request 생성
# Title: [Mine] 지뢰 탐지 훈련 시스템 구현
# Description: 지뢰 탐지, 해제 미니게임, 점수 시스템
```

---

## 통합 담당자 — MainScene 관리 (TBD)

```bash
# 1. 프로젝트 클론
git clone https://github.com/hoshinoresorts1/VR-Tactical-Training.git
cd VR-Tactical-Training

# 2. main 브랜치 유지
git checkout main
git pull origin main

# 3. 각 팀원의 PR 검토 및 Merge
# GitHub에서 Pull Request 탭에서
# - 코드 리뷰
# - 테스트 확인
# - Approve & Merge

# 4. MainScene에 각 Prefab 배치
Assets/_Project/Scenes/MainScene.unity 수정
# - GearTrainingArea 배치
# - ShootingTrainingArea 배치
# - CQBTrainingArea 배치
# - MineTrainingArea 배치
# - 레벨 전환 로직 구현

# 5. 최종 통합 커밋
git add Assets/_Project/Scenes/MainScene.unity
git commit -m "Integrate all training modules"
git push origin main

# 6. Release 태그 생성
git tag v1.0.0
git push origin v1.0.0
```

---

## 공통 명령어

### 상황별 Git 명령어

#### 내 변경사항 확인
```bash
git status
```

#### 최신 main 내용 받기
```bash
git pull origin main
```

#### 특정 파일 되돌리기
```bash
git checkout -- Assets/_Project/Scenes/MineScene.unity
```

#### 마지막 커밋 수정
```bash
git commit --amend
```

#### 커밋 히스토리 보기
```bash
git log --oneline -10
```

#### 다른 팀원의 작업 확인
```bash
git branch -a
# 다른 팀원의 브랜치를 로컬로 받기
git checkout -b feature/gear origin/feature/gear
```

---

## 주의사항

### ❌ 하지 말아야 할 것

1. **MainScene 수정하기** (통합 담당자만 가능)
2. **다른 팀원의 폴더 수정하기**
3. **Library, Temp, Build 폴더 커밋하기**
4. **크기 큰 파일 커밋하기** (1GB 이상)
5. **Personal 정보 커밋하기** (API 키, 비밀번호 등)

### ✅ 해야 할 것

1. **매일 작업 시작 전 `git pull origin main`**
2. **명확한 커밋 메시지 작성** (`[기능명]` 형식)
3. **정기적인 커밋** (하루에 1~3회)
4. **기능 완성 후 PR 생성**
5. **팀원의 PR 검토 및 Feedback**

---

## 트러블슈팅

### Q: "fatal: Could not read from remote repository"

```bash
# SSH 키 확인
ssh -T git@github.com

# 또는 HTTPS 사용
git config --global url."https://github.com/".insteadOf git://github.com/
```

### Q: Merge 충돌 발생

```bash
# 충돌 파일 확인
git status

# 텍스트 에디터에서 충돌 부분 수정 후
git add Assets/_Project/
git commit -m "Resolve merge conflict"
git push origin feature/본인기능
```

### Q: 실수로 파일을 커밋했어요

```bash
# 아직 push하지 않은 경우
git reset HEAD~1 Assets/_Project/실수파일.cs

# 이미 push한 경우
git revert HEAD
```

---

## 체크리스트

프로젝트 시작 전:

```markdown
- [ ] GitHub 저장소 Clone
- [ ] 자신의 Feature 브랜치 생성
- [ ] Unity 2022.3 LTS 설치
- [ ] VR 설정 확인
- [ ] 자신의 Scene 파일 확인
- [ ] COLLABORATION_GUIDE.md 읽기
- [ ] 팀원과 슬랙 연락
```

매일 작업 시작 전:

```markdown
- [ ] git pull origin main 실행
- [ ] 자신의 Scene만 열기
- [ ] 자신의 폴더 내에서만 수정
- [ ] MainScene 건드리지 않기
```

작업 후:

```markdown
- [ ] git status로 변경사항 확인
- [ ] 자신의 파일만 커밋했는지 확인
- [ ] 커밋 메시지 [기능명] 형식 확인
- [ ] git push로 업로드
```

---

**모두 행운을 빕니다! 팀 협업 성공! 🎮🚀**

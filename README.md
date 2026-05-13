# VR Tactical Training

## Project Info
- Unity Version: 2022.3.62f3 LTS
- Template: VR Basic
- Project Path: /Users/administrator/UnityProject/VR-Tactical-Training
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

## Unity Settings
- Asset Serialization: Force Text
- Visible Meta Files enabled

## Final Integration
Each feature owner should provide:
- Feature Scene
- Feature Prefab
- Required scripts
- Short test instructions

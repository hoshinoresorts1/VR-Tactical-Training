# Mine Training Integration Note

## Prefab
- `Assets/_Project/Prefabs/Mine/MineTrainingArea.prefab`
- Drag this prefab into `MainScene` at the desired mine training area location.
- The prefab is intentionally grouped under one root object: `MineTrainingArea`.

## Not Included In Prefab
- `TrainingScoreManager`
- Player / XR Origin
- Main Camera
- EventSystem
- Directional Light / scene lighting settings
- Build Settings / RenderSettings

## Runtime Dependencies
- `TrainingScoreManager` must exist in the shared scene if final score tracking is needed.
- Mine score is added only through:
  - `TrainingScoreManager.Instance.AddMineScore(...)`
- Player or XR Origin must be provided by `MainScene`.
- The player root or relevant collider object must use the `Player` tag.
- The `MAIN HALL` return button uses `TrainingSceneReturn`.
  - Default target scene name: `MainHall`
  - Add the real main hall scene to Build Settings later.

## Tags
- `Player`
  - Used to detect player stepping on mines and reaching the goal.
  - Built-in Unity tag, assign it to the XR Origin/player root or the collider used for movement.
- `Mine`
  - Used by mine objects.
  - Custom tag. The editor tools add it automatically when placing mines.

## Layers
- `Default` layer: `0`
- No custom layer is required by the mine feature.
- Mine objects, wires, detector, goal, and environment currently use `Default`.

## Input
- Keyboard test inputs:
  - `F`: start defusal when looking at a revealed mine
  - `1`, `2`, `3`: cut red/blue/green wires for desktop testing
  - `R`: retry after game over
- XR inputs:
  - XR Ray Select: start defusal, select/cut wires, press retry/main hall buttons
  - XR Grab: pick up and carry the mine detector
- The mine detector detects mines only while held when created as a pickup detector.

## Required Packages
- `com.unity.xr.interaction.toolkit` 3.1.2
- `com.unity.inputsystem` 1.14.0
- `com.unity.xr.openxr` 1.14.3
- `com.unity.cloud.gltfast` 6.14.1
- `com.unity.render-pipelines.universal` 14.0.12

## MainScene Placement
1. Open `MainScene`.
2. Drag `MineTrainingArea.prefab` into the scene.
3. Move the root object to the mine training zone location.
4. Ensure the shared XR Origin/player object has the `Player` tag.
5. Ensure `TrainingScoreManager` exists somewhere in the shared scene if scores are needed.
6. If the main hall scene exists, add it to Build Settings as `MainHall` or update `TrainingSceneReturn.mainHallSceneName`.

## Manual Test
1. Enter Play Mode in the scene containing `MineTrainingArea`.
2. Pick up the mine detector with XR Grab or use desktop test setup.
3. Move near hidden mines and confirm beep, haptic feedback, and `MINE DETECTED` UI.
4. Select a revealed mine and start defusal.
5. Cut wires in the displayed color order.
6. Confirm correct cuts visually split the wire.
7. Step on a mine and confirm life loss, haptic feedback, and game-over behavior.
8. Reach `MineGoal` and confirm `CLEAR` appears.
9. Press `MAIN HALL`.
   - If `MainHall` is not in Build Settings yet, a warning is expected and the scene will not change.

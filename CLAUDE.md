# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity mobile drift racing game (C#). The car drives forward automatically; the player steers via an on-screen steering wheel to drift past obstacles. Hitting a wall/container = game over, reaching the goal = win.

- **Engine**: Unity 2022.3.62f3
- **Render Pipeline**: Built-in (Forward)
- **Target**: Mobile (Android/iOS), portrait orientation
- **Color Space**: Linear

## Architecture

### Scene Flow
```
LoadingScene (index 0) → SelectScene (index 1) → LoadingScene → Map_X (index 2+)
```

### Singleton Managers (cross-scene)
- **ProgressManager** — level unlock state via PlayerPrefs. `DontDestroyOnLoad`.
- **AudioManager** — music/SFX sources with toggle on/off. `DontDestroyOnLoad`.

### Per-Scene Managers
- **GameManager** — state machine: Ready → Playing → Paused/GameOver/Win. Controls UI panel visibility.
- **ScoreManager** — drift scoring with combo multiplier.
- **GameUI** — wires all UI buttons to GameManager actions.

### Car Control (critical pattern)
`AutoDriveCarController` does NOT bypass `PrometeoCarController`. Instead it:
1. Creates invisible fake `PrometeoTouchInput` buttons at runtime
2. Sets `useTouchControls = true` on PrometeoCarController
3. Each frame sets `buttonPressed` on fake buttons based on `SteeringWheelUI` input
4. PrometeoCarController runs its full original physics/drift/effects pipeline

This preserves the exact same drift feel as the original Prometeo demo scenes. Never disable PrometeoCarController or call its methods directly — always go through the fake touch button pattern.

### Key Script Relationships
```
SteeringWheelUI (UI drag input, outputs SteeringAmount -1..1)
    ↓
AutoDriveCarController (sets fake button states)
    ↓
PrometeoCarController (original physics, drift, effects, sounds)
    ↓
ObstacleCollision (OnCollisionEnter → GameManager.TriggerGameOver)
LevelGoal (OnTriggerEnter → GameManager.TriggerWin)
```

### Third-Party Assets
- **PROMETEO Car Controller** — car physics, drift mechanics, wheel colliders. Scripts in `Assets/PROMETEO - Car Controller/Scripts/`. Do not modify these files.
- **Simple Scroll-Snap** (DanielLochner) — carousel UI for map selection. Namespace: `DanielLochner.Assets.SimpleScrollSnap`.
- **Layer Lab GUI** — UI prefabs (buttons, frames, popups). Two themes: SuperCasual and TheStone.
- **Shipping Container Environment** / **HQ Shipping Container** — 3D obstacle assets.

### Tags Required
`Player` (car), `Obstacle` (containers), `Wall` (boundaries).

## Custom Scripts Location

All game scripts are in `Assets/Scripts/`. See `Assets/Scripts/README.md` for detailed setup instructions per scene.

## Key Conventions

- Singletons use `Instance` (PascalCase) for MonoBehaviour managers, except `ProgressManager` and `AudioManager` which use `instance` (lowercase).
- Scene transitions go through `LoadingScene.LoadScene("sceneName")` static method.
- Level indexing: PlayerPrefs key `"selected_map"` stores 0-based panel index. Level number = panelIndex + 1. Scene names follow pattern `"Map_X"` where X = level number.
- `Time.timeScale` is managed by GameManager (0 when paused, 1 otherwise). Always reset to 1 before scene transitions.

# Design Patterns Games

Unity 6 (6000.3 LTS) project for a software architecture assignment focused on **Design Patterns** and **OOP**.

Reference: [unitydesignpatterns.com](https://www.unitydesignpatterns.com)

## How To Run (important)

1. Open this project in **Unity 6000.3 LTS**.
2. Open **`Assets/Scenes/Bootstrap.unity`** (do not start from game scenes alone).
3. Press **Play**.
4. Choose a mode in the dropdown → **Play** → pick a level.
5. **Esc** opens pause (Resume / Restart / Main Menu).

If UI/scenes look wrong: menu **DesignPatterns → Rebuild UI**, then **Reload** when Unity asks, then Play again from `Bootstrap`.

## What You Get (3 playable modes + architecture doc)

| Mode | Scene | What to demo |
|------|-------|--------------|
| Tic Tac Toe | `TicTacToe` | Local 2P, score, Undo/Redo, Restart |
| Adventure | `Adventure` | WASD, avoid red enemies, collect yellow pickups, score + minimap |
| Spatial Partition | `UnseenDemo` | WASD move, green = nearby, **T** toggles brute force vs grid timing |
| Escape Room | docs only | Class + sequence diagrams (not a playable scene) |

## Patterns Map

### Part 1 — Tic Tac Toe

| Feature | Pattern | Code |
|---------|---------|------|
| Score / turn / match | Singleton | `TicTacToeGameManager` |
| UI vs rules | MVP | `BoardModel`, `BoardView`, `GamePresenter` |
| Undo / Redo | Command | `PlaceMarkCommand`, `CommandHistory` |

**Why Command (not Memento):** each move is one cell place/clear. Memento would snapshot the whole board every turn for no gain here.

### Part 2 — Adventure

| Feature | Pattern | Code |
|---------|---------|------|
| Pickup notify | Observer | `PickupEventChannel` |
| Score (not Singleton) | ScriptableObject service | `ScoreService` |
| HUD + minimap | Observers | `ScoreView`, `MinimapController` |

Controls: **WASD**. Yellow cylinders = pickups. Red cubes chase you. Catch = Game Over.

### Part 3 — Escape Room (architecture only)

See [Docs/Part3_EscapeRoom/Architecture.md](Docs/Part3_EscapeRoom/Architecture.md):

- Class diagram
- Sequence: use item on item
- Sequence: solve puzzle / open door
- How to add a new item without breaking the core

Upload that document to Google Drive for submission.

### Part 4 — Unseen: Spatial Partition

| Feature | Pattern | Code |
|---------|---------|------|
| Fast proximity query | Spatial Partition | `SpatialGrid`, `SpatialPartitionDemo` |

Controls: **WASD** move query center; **T** toggle Spatial Partition vs Brute Force; compare **ms** on HUD. Nearby cubes turn green.

## Project Structure

```
Assets/
  _Shared/            Singleton, EventChannel, ServiceLocator
  Core/               GameFlow, SceneLoader, ScreenManager, catalog SO
  UI/                 Theme, Factory, menus
  Part1_TicTacToe/
  Part2_Adventure/
  Part4_UnseenPattern/
  Scenes/             Bootstrap + game scenes
Docs/Part3_EscapeRoom/
```

## Video Demo Checklist

1. Bootstrap → dropdown → each mode → level select
2. Tic Tac Toe: place marks, Undo, Redo, win/draw, Restart
3. Adventure: move, collect pickup (score up, yellow marker gone), get caught → Game Over
4. Spatial: move into a cluster, nearby green, press T and show query time change
5. Esc pause in any game → Resume / Main Menu

## Submission Checklist

- [ ] Public GitHub repo
- [ ] Screen recording of the checklist above
- [ ] Escape Room diagrams on Drive (from `Architecture.md`)

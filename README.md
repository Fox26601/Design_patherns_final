# Design Patterns Games

Unity 6 (6000.3 LTS) project for a software architecture course on **Design Patterns** and **OOP**.

Reference: [unitydesignpatterns.com](https://www.unitydesignpatterns.com)

## How To Run

1. Open the project in **Unity 6000.3 LTS**.
2. Open **`Assets/Scenes/Bootstrap.unity`**.
3. Press **Play**.
4. Choose a mode → **Play** → select a level.
5. **Esc** opens pause (Resume / Restart / Main Menu).

If the UI looks wrong: **DesignPatterns → Rebuild UI**, reload when prompted, then Play from `Bootstrap` again.

## Modes

| Mode | Scene | Demo |
|------|-------|------|
| Tic Tac Toe | `TicTacToe` | Local 2P, score, Undo/Redo, Restart |
| Adventure | `Adventure` | WASD, enemies, yellow pickups, score HUD, minimap |
| Escape Room | `EscapeRoom` / `EscapeRoomArchitecture` | Crimson Mini Room + architecture docs |

## Patterns

### Part 1 — Tic Tac Toe

| Feature | Pattern | Code |
|---------|---------|------|
| Score / turn / match | Singleton | `TicTacToeGameManager` |
| UI vs rules | MVP | `BoardModel`, `BoardView`, `GamePresenter` |
| Undo / Redo | Command | `PlaceMarkCommand`, `CommandHistory` |

Command fits better than Memento here: each undo step is one cell place/clear.

### Part 2 — Adventure

| Feature | Pattern | Code |
|---------|---------|------|
| Pickup notify | Observer | `PickupEventChannel` |
| Score | ScriptableObject service | `ScoreService` |
| HUD + minimap | Observer | `ScoreView`, `MinimapController` |

Controls: **WASD**. Yellow pickups raise score. Red enemies chase; contact ends the run.

### Part 3 — Escape Room / Part 4 — Visitor

Playable room and docs: [Architecture.md](Docs/Part3_EscapeRoom/Architecture.md), [Walkthrough.md](Docs/Part3_EscapeRoom/Walkthrough.md).

| Feature | Pattern | Code |
|---------|---------|------|
| Room state / views | State + Observer | `RoomStateController`, `InteractableView` |
| Item-on-item rules | Strategy | `InteractionResolver`, `InteractionRuleSO` |
| Puzzle prerequisites | Composite | `PuzzleDefinition`, conditions |
| Spawns / config | Factory + ScriptableObject | `RoomObjectFactory`, `EscapeRoomSetupSO` |
| Inspect / report / use on items | Visitor | `IRoomItem`, `InspectVisitor`, `InventoryReportVisitor`, `UseOnTargetVisitor` |

In Crimson Mini Room: **I** inspect all, **R** room report, inventory slot + click target applies use through Visitor.

Upload the architecture diagrams / `Architecture.md` to Google Drive for submission.

## Project Structure

```
Assets/
  _Shared/
  Core/
  UI/
  Part1_TicTacToe/
  Part2_Adventure/
  Part3_EscapeRoom/
  Scenes/
Docs/Part3_EscapeRoom/
```

## Video Checklist

1. Bootstrap → each mode → level select
2. Tic Tac Toe: marks, Undo, Redo, win/draw, Restart
3. Adventure: move, collect pickup, Game Over on catch
4. Escape Room → Architecture Docs (Visitor on the diagrams)
5. Crimson Mini Room: **I** / **R**, then key on drawer/door
6. Esc → Resume / Main Menu

## Submission

- [ ] Public GitHub repo
- [ ] Screen recording of the checklist
- [ ] Escape Room diagrams on Drive

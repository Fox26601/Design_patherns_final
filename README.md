# Design Patterns Games

Unity 6 (6000.3 LTS) project for a software architecture assignment focused on **Design Patterns** and **OOP**.

Reference: [unitydesignpatterns.com](https://www.unitydesignpatterns.com)

## Project Structure

```
Assets/
  _Shared/              Shared utilities (Singleton, EventChannel, ServiceLocator)
  Core/                 UI shell, scene flow, ScriptableObject catalog
  UI/                   Menu screens (MainMenu, LevelSelect, Pause, GameOver, Loading)
  Part1_TicTacToe/      Tic Tac Toe local multiplayer
  Part2_Adventure/      Top-down adventure with minimap
  Part3_EscapeRoom/     Escape room (playable + architecture)
  Scenes/               Bootstrap, MainMenu, game scenes
Docs/
  Part3_EscapeRoom/     Escape room diagrams + walkthrough
```

## How To Run

1. Open the project in **Unity 6000.3 LTS** (or newer 6000.x).
2. If scenes are missing, run menu: **DesignPatterns → Setup Project**.
3. Open `Assets/Scenes/Bootstrap.unity` and press Play.
4. Use the dropdown to select a game mode, choose a level, and play.
5. Press **Esc** during gameplay to open the pause menu.

## Submission Checklist

- [ ] Public GitHub repository (without `Library/`)
- [ ] Screen recording (~5–8 min): Main menu → Part 1 Tic Tac Toe → Part 2 Adventure → Part 3 Escape Room
- [ ] Upload Part 3 diagrams to Google Drive: PNG files in [`Docs/Part3_EscapeRoom/`](Docs/Part3_EscapeRoom/) (`class_diagram.png`, `sequence_item_on_item.png`, `sequence_puzzle_door.png`)
- [ ] README describes patterns used in each part (see below)

## Part 1 — Tic Tac Toe

| Feature | Pattern | Why |
|---------|---------|-----|
| Score / turn / match state | **Singleton** (`TicTacToeGameManager`) | One authoritative match coordinator |
| UI vs logic separation | **MVP** (`BoardModel`, `BoardView`, `GamePresenter`) | View has no rules; model has no Unity API |
| Undo / Redo | **Command** (`PlaceMarkCommand`, `CommandHistory`) | Each move is a reversible action |

### Command vs Memento

We chose **Command** because a tic-tac-toe move is a small discrete action. `Undo()` only clears one cell, and `Redo()` replays the same command. **Memento** would snapshot the entire 3×3 board each turn — more memory and unnecessary for this problem size.

## Part 2 — Adventure

| Feature | Pattern | Why |
|---------|---------|-----|
| Pickup collection notifications | **Observer** (`PickupEventChannel`) | Pickups do not know about UI or score |
| Score update on collect | Observer subscriber (`PickupScoreHandler`) | Score rises when channel event is raised |
| Score storage | **ScriptableObject service** (`ScoreService`) | Shared state without Singleton (per assignment requirement) |
| HUD + minimap updates | Observer subscribers (`ScoreView`, `MinimapController`) | Multiple views react to one event |

Controls: **WASD** to move, avoid red cubes, collect yellow cylinders.

## Part 3 — Escape Room

**Playable mini escape room** + architecture diagrams.

| Deliverable | Location |
|-------------|----------|
| Playable scene | Main menu → **Part 3 — Escape Room** → **Crimson Mini Room** |
| Architecture docs viewer | Same mode → **Architecture Docs** |
| Class + sequence diagrams | `Docs/Part3_EscapeRoom/*.png` |
| Walkthrough | `Docs/Part3_EscapeRoom/Walkthrough.md` |
| C# systems | `Assets/Part3_EscapeRoom/Scripts/` |

### Patterns

| Concern | Pattern |
|---------|---------|
| Room object states | **State** + **Observer** (`RoomStateController`) |
| Item used on object | **Strategy** (`IItemUsageHandler` / `InteractionRuleSO`) |
| Safe prerequisites | **Composite** (`ICondition`) |
| Content tuning | **ScriptableObject** (`EscapeRoomSetupSO`) |

### Requirements checklist

- 6+ object types (key, drawer, note, safe, gold key, door) + decoy
- 4 ordered steps to escape
- Collectibles in inventory; used keys are consumed
- Item-on-item (key→drawer, gold key→door)
- Room state changes (drawer/safe/door)

Controls: **click** to interact · select inventory item then click target · **Esc** pause  
Code for safe: **7391**

---

## Кратко по заданию (RU)

| Часть | Паттерн | Что сделано |
|-------|---------|-------------|
| 1. Tic Tac Toe | Singleton, MVP, Command | Локальный мультиплеер, undo/redo, счёт |
| 2. Adventure | Observer, ScriptableObject | Pickups, миникарта, счёт без Singleton |
| 3. Escape Room | State, Observer, Strategy, Composite | Playable room + diagrams in `Docs/Part3_EscapeRoom/` |

**Запуск:** `Bootstrap.unity` → Play → выбрать режим в dropdown.

## Shared UI Shell

| Component | Pattern | Role |
|-----------|---------|------|
| `GameFlowManager` | Singleton | Mode/level selection, pause state |
| `SceneLoaderService` | Singleton | Async scene loading |
| `ScreenManager` | State stack | Menu navigation, pause overlay |
| `GameCatalog` | ScriptableObject | Data-driven game mode list for dropdown |

Adding a new game mode: create `GameModeDefinition` + `LevelDefinition` assets and add them to `GameCatalog` — no UI code changes required.

## Video Demo Suggestions

1. Main menu → dropdown → level select → launch each mode
2. Tic Tac Toe: place marks, undo/redo, win round, restart
3. Adventure: move, collect pickup (score + minimap marker disappear), get caught by enemy
4. Escape Room: pick rusty key → use on drawer → read note → open safe (7391) → gold key on door
5. Pause menu from any gameplay scene

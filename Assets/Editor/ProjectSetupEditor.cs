#if UNITY_EDITOR
using System.IO;
using Core;
using Part1_TicTacToe;
using Part2_Adventure;
using Part3_EscapeRoom;
using TMPro;
using UI.Screens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EditorTools
{
    public static class ProjectSetupEditor
    {
        private const string ScenesPath = "Assets/Scenes";
        private const string DataPath = "Assets/Core/Data";

        [MenuItem("DesignPatterns/Setup Project")]
        public static void SetupProject()
        {
            EnsureFolders();
            var catalog = CreateDataAssets();
            CreateBootstrapScene();
            CreateMainMenuScene();
            CreateTicTacToeScene();
            CreateAdventureScene();
            CreateEscapeRoomScene();
            CreateEscapeRoomArchitectureScene();
            ConfigureBuildSettings();
            ConfigureEscapeRoomDiagramSprites();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Design Patterns project setup complete.");
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(ScenesPath);
            Directory.CreateDirectory(DataPath);
            Directory.CreateDirectory("Assets/Part2_Adventure/Data");
            Directory.CreateDirectory("Assets/Part3_EscapeRoom/Data");
            Directory.CreateDirectory("Assets/Part3_EscapeRoom/Resources/Diagrams");
            CopyEscapeRoomDiagrams();
        }

        private static GameCatalog CreateDataAssets()
        {
            var pickupChannel = GetOrCreateAsset<PickupEventChannel>("Assets/Part2_Adventure/Data/PickupEventChannel.asset");
            var scoreService = GetOrCreateAsset<ScoreService>("Assets/Part2_Adventure/Data/ScoreService.asset");

            var tttLevel = GetOrCreateAsset<LevelDefinition>($"{DataPath}/TTT_SingleMatch.asset");
            SetLevel(tttLevel, "Single Match", 0, 2f, 0, 0, 0);

            var advEasy = GetOrCreateAsset<LevelDefinition>($"{DataPath}/Adventure_Easy.asset");
            SetLevel(advEasy, "Easy", 0, 1.5f, 2, 5, 0);
            var advNormal = GetOrCreateAsset<LevelDefinition>($"{DataPath}/Adventure_Normal.asset");
            SetLevel(advNormal, "Normal", 1, 2.5f, 4, 7, 0);
            var advHard = GetOrCreateAsset<LevelDefinition>($"{DataPath}/Adventure_Hard.asset");
            SetLevel(advHard, "Hard", 2, 4f, 6, 10, 0);

            var escapePlayLevel = GetOrCreateAsset<LevelDefinition>($"{DataPath}/EscapeRoom_Play.asset");
            SetLevel(escapePlayLevel, "Crimson Mini Room", 0, 0f, 0, 0, 0);
            var escapeDocsLevel = GetOrCreateAsset<LevelDefinition>($"{DataPath}/EscapeRoom_Architecture.asset");
            SetLevel(escapeDocsLevel, "Architecture Docs", 1, 0f, 0, 0, 0, "EscapeRoomArchitecture");

            CreateEscapeRoomDataAssets();

            var tttMode = GetOrCreateAsset<GameModeDefinition>($"{DataPath}/Mode_TicTacToe.asset");
            SetMode(tttMode, "Part 1 — Tic Tac Toe", "TicTacToe", new[] { tttLevel });
            var advMode = GetOrCreateAsset<GameModeDefinition>($"{DataPath}/Mode_Adventure.asset");
            SetMode(advMode, "Part 2 — Adventure", "Adventure", new[] { advEasy, advNormal, advHard });
            var escapeMode = GetOrCreateAsset<GameModeDefinition>($"{DataPath}/Mode_EscapeRoom.asset");
            SetMode(escapeMode, "Part 3 — Escape Room", "EscapeRoom", new[] { escapePlayLevel, escapeDocsLevel });

            var catalog = GetOrCreateAsset<GameCatalog>($"{DataPath}/GameCatalog.asset");
            SetCatalog(catalog, new[] { tttMode, advMode, escapeMode });

            return catalog;
        }

        private static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var managers = new GameObject("PersistentManagers");
            var flow = managers.AddComponent<GameFlowManager>();
            managers.AddComponent<SceneLoaderService>();

            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>($"{DataPath}/GameCatalog.asset");
            SetPrivateField(flow, "catalog", catalog);
            SetPrivateField(flow, "mainMenuSceneName", "MainMenu");

            CreatePersistentUiShell();

            var bootstrap = new GameObject("BootstrapLoader");
            bootstrap.AddComponent<BootstrapLoader>();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Bootstrap.unity");
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
                mainCamera.orthographic = true;
            }

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/MainMenu.unity");
        }

        private static ScreenManager CreatePersistentUiShell()
        {
            var eventSystem = CreateEventSystem();

            var canvas = CreateCanvas("PersistentUI");
            canvas.AddComponent<PersistentUIInitializer>();
            canvas.AddComponent<UI.UiLayoutFixer>();
            if (eventSystem != null)
            {
                eventSystem.transform.SetParent(canvas.transform, false);
            }

            var screenManagerObject = new GameObject("ScreenManager");
            screenManagerObject.transform.SetParent(canvas.transform, false);
            var screenManager = screenManagerObject.AddComponent<ScreenManager>();

            var mainMenu = CreatePanel(canvas.transform, "MainMenuScreen", new Vector2(0, 0), new Vector2(500, 400));
            var mainMenuScreen = mainMenu.AddComponent<MainMenuScreen>();
            var title = CreateText(mainMenu.transform, "Title", "Design Patterns Games", 36, new Vector2(0, 140));
            var dropdown = CreateDropdown(mainMenu.transform, "ModeDropdown", new Vector2(0, 40));
            var playButton = CreateButton(mainMenu.transform, "PlayButton", "Play", new Vector2(-80, -80));
            var quitButton = CreateButton(mainMenu.transform, "QuitButton", "Quit", new Vector2(80, -80));
            SetPrivateField(mainMenuScreen, "modeDropdown", dropdown);
            SetPrivateField(mainMenuScreen, "playButton", playButton);
            SetPrivateField(mainMenuScreen, "quitButton", quitButton);

            var levelSelect = CreatePanel(canvas.transform, "LevelSelectScreen", Vector2.zero, new Vector2(500, 450));
            var levelSelectScreen = levelSelect.AddComponent<LevelSelectScreen>();
            levelSelect.SetActive(false);
            var levelTitle = CreateText(levelSelect.transform, "Title", "Select Level", 28, new Vector2(0, 170));
            var levelContainer = new GameObject("LevelButtonContainer");
            levelContainer.transform.SetParent(levelSelect.transform, false);
            var containerRect = levelContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(400, 250);
            containerRect.anchoredPosition = new Vector2(0, 20);
            var layout = levelContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var levelButtonPrefab = CreateButton(levelContainer.transform, "LevelButtonPrefab", "Level", Vector2.zero);
            levelButtonPrefab.gameObject.SetActive(false);
            var backButton = CreateButton(levelSelect.transform, "BackButton", "Back", new Vector2(0, -180));
            SetPrivateField(levelSelectScreen, "levelButtonContainer", levelContainer.transform);
            SetPrivateField(levelSelectScreen, "levelButtonPrefab", levelButtonPrefab);
            SetPrivateField(levelSelectScreen, "backButton", backButton);
            SetPrivateField(levelSelectScreen, "titleText", levelTitle);

            var gameOver = CreatePanel(canvas.transform, "GameOverScreen", Vector2.zero, new Vector2(560, 340));
            var gameOverScreen = gameOver.AddComponent<GameOverScreen>();
            gameOver.SetActive(false);
            var goMessage = CreateText(gameOver.transform, "Message", "Game Over", 24, new Vector2(0, 80));
            goMessage.textWrappingMode = TextWrappingModes.Normal;
            goMessage.overflowMode = TextOverflowModes.Truncate;
            goMessage.enableAutoSizing = true;
            goMessage.fontSizeMin = 18;
            goMessage.fontSizeMax = 28;
            goMessage.rectTransform.sizeDelta = new Vector2(500, 120);
            var retryBtn = CreateButton(gameOver.transform, "RetryButton", "Retry", new Vector2(-90, -100));
            var goMenuBtn = CreateButton(gameOver.transform, "MainMenuButton", "Main Menu", new Vector2(90, -100));
            SetPrivateField(gameOverScreen, "messageText", goMessage);
            SetPrivateField(gameOverScreen, "retryButton", retryBtn);
            SetPrivateField(gameOverScreen, "mainMenuButton", goMenuBtn);

            var pause = CreatePanel(canvas.transform, "PauseScreen", Vector2.zero, new Vector2(560, 360));
            var pauseScreen = pause.AddComponent<PauseScreen>();
            pause.SetActive(false);
            var pauseTitle = CreateText(pause.transform, "Title", "Paused", 32, new Vector2(0, 120));
            var resumeBtn = CreateButton(pause.transform, "ResumeButton", "Resume", new Vector2(0, 40));
            var restartBtn = CreateButton(pause.transform, "RestartButton", "Restart", new Vector2(0, -20));
            var levelBtn = CreateButton(pause.transform, "LevelSelectButton", "Main Menu", new Vector2(0, -80));
            var menuBtn = CreateButton(pause.transform, "MainMenuButton", "Quit", new Vector2(0, -140));
            SetPrivateField(pauseScreen, "resumeButton", resumeBtn);
            SetPrivateField(pauseScreen, "restartButton", restartBtn);
            SetPrivateField(pauseScreen, "levelSelectButton", levelBtn);
            SetPrivateField(pauseScreen, "mainMenuButton", menuBtn);

            var loading = CreatePanel(canvas.transform, "LoadingScreen", Vector2.zero, new Vector2(400, 120));
            var loadingScreen = loading.AddComponent<LoadingScreen>();
            loading.SetActive(false);
            var loadingText = CreateText(loading.transform, "Progress", "Loading...", 24, Vector2.zero);
            SetPrivateField(loadingScreen, "progressText", loadingText);

            SetPrivateField(screenManager, "mainMenuScreen", mainMenuScreen);
            SetPrivateField(screenManager, "levelSelectScreen", levelSelectScreen);
            SetPrivateField(screenManager, "pauseScreen", pauseScreen);
            SetPrivateField(screenManager, "gameOverScreen", gameOverScreen);
            SetPrivateField(screenManager, "loadingScreen", loadingScreen);

            return screenManager;
        }

        private static void CreateTicTacToeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var managerObject = new GameObject("TicTacToeGameManager");
            managerObject.AddComponent<TicTacToeGameManager>();

            var canvas = CreateCanvas("TicTacToeCanvas");
            var boardRoot = CreatePanel(canvas.transform, "BoardView", Vector2.zero, new Vector2(480, 540));
            var boardRect = boardRoot.GetComponent<RectTransform>();
            boardRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardRect.pivot = new Vector2(0.5f, 0.5f);

            var vertical = boardRoot.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(20, 20, 20, 20);
            vertical.spacing = 10f;
            vertical.childAlignment = TextAnchor.MiddleCenter;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;

            var boardView = boardRoot.AddComponent<BoardView>();

            var status = CreateText(boardRoot.transform, "Status", "Turn: X", 22, new Vector2(0, 220));
            AddLayoutElement(status.gameObject, 34f);
            var score = CreateText(boardRoot.transform, "Score", "X: 0   O: 0", 18, new Vector2(0, 180));
            AddLayoutElement(score.gameObject, 30f);

            var grid = new GameObject("Grid");
            grid.transform.SetParent(boardRoot.transform, false);
            var gridRect = grid.AddComponent<RectTransform>();
            gridRect.sizeDelta = new Vector2(280, 280);
            var gridLayout = grid.AddComponent<GridLayoutGroup>();
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            gridLayout.cellSize = new Vector2(84, 84);
            gridLayout.spacing = new Vector2(8, 8);
            AddLayoutElement(grid, 280f);

            var buttons = new Button[9];
            for (var i = 0; i < 9; i++)
            {
                buttons[i] = CreateButton(grid.transform, $"Cell_{i}", string.Empty, Vector2.zero);
            }

            var actions = new GameObject("Actions");
            actions.transform.SetParent(boardRoot.transform, false);
            var actionsRect = actions.AddComponent<RectTransform>();
            actionsRect.sizeDelta = new Vector2(420f, 44f);
            var actionsLayout = actions.AddComponent<HorizontalLayoutGroup>();
            actionsLayout.spacing = 12f;
            actionsLayout.childAlignment = TextAnchor.MiddleCenter;
            actionsLayout.childForceExpandWidth = true;
            actionsLayout.childForceExpandHeight = true;
            AddLayoutElement(actions, 44f);

            var undo = CreateButton(actions.transform, "UndoButton", "Undo", Vector2.zero);
            var redo = CreateButton(actions.transform, "RedoButton", "Redo", Vector2.zero);
            var restart = CreateButton(actions.transform, "RestartButton", "Restart", Vector2.zero);

            SetPrivateField(boardView, "cellButtons", buttons);
            SetPrivateField(boardView, "statusText", status);
            SetPrivateField(boardView, "scoreText", score);
            SetPrivateField(boardView, "undoButton", undo);
            SetPrivateField(boardView, "redoButton", redo);
            SetPrivateField(boardView, "restartButton", restart);

            var controller = new GameObject("TicTacToeController");
            var gameController = controller.AddComponent<TicTacToeGameController>();
            SetPrivateField(gameController, "boardView", boardView);

            new GameObject("GamePauseHandler").AddComponent<GamePauseHandler>();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/TicTacToe.unity");
        }

        private static void CreateEscapeRoomScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0.08f, 0.07f, 0.09f, 1f);
                mainCamera.fieldOfView = 50f;
                mainCamera.transform.position = new Vector3(0f, 6.8f, -5.8f);
                mainCamera.transform.rotation = Quaternion.Euler(48f, 0f, 0f);
            }

            var setup = AssetDatabase.LoadAssetAtPath<EscapeRoomSetupSO>(
                "Assets/Part3_EscapeRoom/Resources/EscapeRoomSetup.asset");
            var controllerObject = new GameObject("EscapeRoomGameController");
            var controller = controllerObject.AddComponent<EscapeRoomGameController>();
            SetPrivateField(controller, "setup", setup);
            SetPrivateField(controller, "raycastCamera", mainCamera);

            new GameObject("GamePauseHandler").AddComponent<GamePauseHandler>();
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/EscapeRoom.unity");
        }

        private static void CreateEscapeRoomDataAssets()
        {
            const string folder = "Assets/Part3_EscapeRoom/Data";
            Directory.CreateDirectory(folder);

            var rustyKey = GetOrCreateAsset<ItemDefinition>($"{folder}/Item_RustyKey.asset");
            SetItemDefinition(rustyKey, "rustyKey", "Rusty Key", ItemType.Collectible);
            var goldKey = GetOrCreateAsset<ItemDefinition>($"{folder}/Item_GoldKey.asset");
            SetItemDefinition(goldKey, "goldKey", "Gold Key", ItemType.Collectible);
            var note = GetOrCreateAsset<ItemDefinition>($"{folder}/Item_Note.asset");
            SetItemDefinition(note, "note", "Note", ItemType.Collectible);

            var keyOnDrawer = GetOrCreateAsset<InteractionRuleSO>($"{folder}/Rule_KeyOnDrawer.asset");
            SetInteractionRule(
                keyOnDrawer,
                "rustyKey",
                "drawer",
                "Drawer opened.",
                new[]
                {
                    new InteractionAction { ActionType = InteractionActionType.ConsumeItem, TargetId = "rustyKey" },
                    new InteractionAction { ActionType = InteractionActionType.SetState, TargetId = "drawer", State = RoomObjectState.Open },
                    new InteractionAction { ActionType = InteractionActionType.SetState, TargetId = "note", State = RoomObjectState.Revealed }
                });

            var keyOnDoor = GetOrCreateAsset<InteractionRuleSO>($"{folder}/Rule_KeyOnDoor.asset");
            SetInteractionRule(
                keyOnDoor,
                "goldKey",
                "door",
                "The door unlocks. You escaped!",
                new[]
                {
                    new InteractionAction { ActionType = InteractionActionType.ConsumeItem, TargetId = "goldKey" },
                    new InteractionAction { ActionType = InteractionActionType.SetState, TargetId = "door", State = RoomObjectState.Unlocked },
                    new InteractionAction { ActionType = InteractionActionType.Win }
                });

            var drawerOpenCondition = GetOrCreateAsset<StateConditionSO>($"{folder}/Condition_DrawerOpen.asset");
            SetPrivateField(drawerOpenCondition, "ObjectId", "drawer");
            SetPrivateField(drawerOpenCondition, "RequiredState", RoomObjectState.Open);
            EditorUtility.SetDirty(drawerOpenCondition);

            var readNoteCondition = GetOrCreateAsset<FlagConditionSO>($"{folder}/Condition_HasReadNote.asset");
            SetPrivateField(readNoteCondition, "FlagName", "hasReadNote");
            SetPrivateField(readNoteCondition, "RequiredValue", true);
            EditorUtility.SetDirty(readNoteCondition);

            var safePuzzle = GetOrCreateAsset<PuzzleDefinition>($"{folder}/Puzzle_Safe.asset");
            SetPrivateField(safePuzzle, "PuzzleId", "safe_code");
            SetPrivateField(safePuzzle, "StatePrerequisites", new[] { drawerOpenCondition });
            SetPrivateField(safePuzzle, "FlagPrerequisites", new[] { readNoteCondition });
            SetPrivateField(safePuzzle, "RequiredCode", "7391");
            SetPrivateField(safePuzzle, "RewardItemId", "goldKey");
            SetPrivateField(safePuzzle, "TargetObjectId", "safe");
            EditorUtility.SetDirty(safePuzzle);

            var setup = GetOrCreateAsset<EscapeRoomSetupSO>("Assets/Part3_EscapeRoom/Resources/EscapeRoomSetup.asset");
            SetPrivateField(setup, "RoomId", "crimson_mini");
            SetPrivateField(setup, "SafeCode", "7391");
            SetPrivateField(setup, "HintText",
                "Click objects to examine or pick them up.\nSelect an inventory item, then click a target to use it.\nEsc pauses.");
            SetPrivateField(setup, "Items", new[] { rustyKey, goldKey, note });
            SetPrivateField(setup, "InteractionRules", new[] { keyOnDrawer, keyOnDoor });
            SetPrivateField(setup, "SafePuzzle", safePuzzle);
            SetPrivateField(setup, "InitialStates", new[]
            {
                new InitialRoomState { ObjectId = "drawer", State = RoomObjectState.Closed },
                new InitialRoomState { ObjectId = "note", State = RoomObjectState.Locked },
                new InitialRoomState { ObjectId = "safe", State = RoomObjectState.Locked },
                new InitialRoomState { ObjectId = "door", State = RoomObjectState.Locked }
            });
            EditorUtility.SetDirty(setup);
        }

        private static void SetItemDefinition(ItemDefinition item, string id, string displayName, ItemType type)
        {
            SetPrivateField(item, "ItemId", id);
            SetPrivateField(item, "DisplayName", displayName);
            SetPrivateField(item, "Type", type);
            EditorUtility.SetDirty(item);
        }

        private static void SetInteractionRule(
            InteractionRuleSO rule,
            string sourceId,
            string targetId,
            string successMessage,
            InteractionAction[] actions)
        {
            SetPrivateField(rule, "SourceItemId", sourceId);
            SetPrivateField(rule, "TargetInteractableId", targetId);
            SetPrivateField(rule, "SuccessMessage", successMessage);
            SetPrivateField(rule, "Actions", actions);
            EditorUtility.SetDirty(rule);
        }

        private static void CreateEscapeRoomArchitectureScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0.1f, 0.11f, 0.14f, 1f);
            }

            var viewer = new GameObject("EscapeRoomArchitectureViewer");
            viewer.AddComponent<EscapeRoomArchitectureViewer>();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/EscapeRoomArchitecture.unity");
        }

        private static void CopyEscapeRoomDiagrams()
        {
            const string sourceFolder = "Docs/Part3_EscapeRoom";
            const string targetFolder = "Assets/Part3_EscapeRoom/Resources/Diagrams";
            if (!Directory.Exists(sourceFolder))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(sourceFolder, "*.png"))
            {
                var targetPath = Path.Combine(targetFolder, Path.GetFileName(file));
                File.Copy(file, targetPath, true);
            }
        }

        private static void ConfigureEscapeRoomDiagramSprites()
        {
            const string folder = "Assets/Part3_EscapeRoom/Resources/Diagrams";
            if (!Directory.Exists(folder))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(folder, "*.png"))
            {
                var assetPath = file.Replace('\\', '/');
                if (!assetPath.StartsWith("Assets/"))
                {
                    continue;
                }

                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        private static void CreateAdventureScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Ground";
            plane.transform.localScale = new Vector3(4f, 1f, 4f);
            plane.GetComponent<Renderer>().sharedMaterial.color = Color.white;

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.GetComponent<Renderer>().sharedMaterial.color = new Color(0.25f, 0.55f, 1f, 1f);
            var playerBody = player.AddComponent<Rigidbody>();
            playerBody.useGravity = false;
            var playerController = player.AddComponent<PlayerController>();
            var playerCollider = player.GetComponent<CapsuleCollider>();
            playerCollider.height = 2f;

            var cameraFollow = Camera.main != null ? Camera.main.gameObject.AddComponent<AdventureCameraFollow>() : null;
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3(0f, 14f, -8f);
                Camera.main.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            }

            var enemyPrefab = CreateEnemyPrefab();
            var pickupPrefab = CreatePickupPrefab();

            var spawnRoot = new GameObject("SpawnRoot").transform;

            var canvas = CreateCanvas("AdventureHUD");
            var scoreText = CreateHudText(
                canvas.transform,
                "ScoreText",
                "Score: 0",
                22,
                new Vector2(24f, -24f),
                new Vector2(420f, 64f));
            var scoreViewObject = scoreText.gameObject.AddComponent<ScoreView>();
            var scoreService = AssetDatabase.LoadAssetAtPath<ScoreService>("Assets/Part2_Adventure/Data/ScoreService.asset");
            SetPrivateField(scoreViewObject, "scoreService", scoreService);
            SetPrivateField(scoreViewObject, "scoreText", scoreText);

            CreateHudText(
                canvas.transform,
                "InstructionsText",
                "Controls: WASD move · Esc pause\nWin: collect all pickups\nLose: enemy touches you",
                13,
                new Vector2(16f, -64f),
                new Vector2(360f, 120f));

            var mapPanel = CreatePanel(canvas.transform, "Minimap", new Vector2(-32f, -32f), new Vector2(180, 180));
            var mapRoot = mapPanel.GetComponent<RectTransform>();
            mapRoot.anchorMin = new Vector2(1f, 1f);
            mapRoot.anchorMax = new Vector2(1f, 1f);
            mapRoot.pivot = new Vector2(1f, 1f);
            mapRoot.anchoredPosition = new Vector2(-32f, -32f);
            var markerPrefab = CreateImageMarker(mapPanel.transform);
            var minimap = mapPanel.AddComponent<MinimapController>();
            var pickupChannel = AssetDatabase.LoadAssetAtPath<PickupEventChannel>("Assets/Part2_Adventure/Data/PickupEventChannel.asset");
            SetPrivateField(minimap, "mapRoot", mapRoot);
            SetPrivateField(minimap, "markerPrefab", markerPrefab);
            SetPrivateField(minimap, "pickupEventChannel", pickupChannel);
            SetPrivateField(minimap, "worldCenter", player.transform);
            SetPrivateField(minimap, "worldSize", 40f);
            SetPrivateField(minimap, "mapSize", 80f);

            var scoreHandlerObject = new GameObject("PickupScoreHandler");
            var scoreHandler = scoreHandlerObject.AddComponent<PickupScoreHandler>();
            SetPrivateField(scoreHandler, "pickupEventChannel", pickupChannel);
            SetPrivateField(scoreHandler, "scoreService", scoreService);

            var controllerObject = new GameObject("AdventureController");
            var controller = controllerObject.AddComponent<AdventureGameController>();
            SetPrivateField(controller, "player", player.transform);
            SetPrivateField(controller, "cameraFollow", cameraFollow);
            SetPrivateField(controller, "enemyPrefab", enemyPrefab);
            SetPrivateField(controller, "pickupPrefab", pickupPrefab);
            SetPrivateField(controller, "pickupEventChannel", pickupChannel);
            SetPrivateField(controller, "scoreService", scoreService);
            SetPrivateField(controller, "minimap", minimap);
            SetPrivateField(controller, "spawnRoot", spawnRoot);
            SetPrivateField(controller, "spawnRadius", 15f);

            new GameObject("GamePauseHandler").AddComponent<GamePauseHandler>();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Adventure.unity");
        }

        private static GameObject CreateEnemyPrefab()
        {
            const string path = "Assets/Part2_Adventure/Enemy.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            var enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = "Enemy";
            enemy.GetComponent<Renderer>().sharedMaterial.color = Color.red;
            var collider = enemy.GetComponent<BoxCollider>();
            collider.isTrigger = true;
            var body = enemy.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            enemy.AddComponent<EnemyController>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
            Object.DestroyImmediate(enemy);
            return prefab;
        }

        private static GameObject CreatePickupPrefab()
        {
            const string path = "Assets/Part2_Adventure/Pickup.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing;
            }

            var pickup = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pickup.name = "Pickup";
            pickup.transform.localScale = new Vector3(0.6f, 0.3f, 0.6f);
            pickup.GetComponent<Renderer>().sharedMaterial.color = Color.yellow;
            var collider = pickup.GetComponent<CapsuleCollider>();
            collider.isTrigger = true;
            pickup.AddComponent<Pickup>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(pickup, path);
            Object.DestroyImmediate(pickup);
            return prefab;
        }

        private static void ConfigureBuildSettings()
        {
            var scenes = new[]
            {
                $"{ScenesPath}/Bootstrap.unity",
                $"{ScenesPath}/MainMenu.unity",
                $"{ScenesPath}/TicTacToe.unity",
                $"{ScenesPath}/Adventure.unity",
                $"{ScenesPath}/EscapeRoom.unity",
                $"{ScenesPath}/EscapeRoomArchitecture.unity"
            };

            var buildScenes = new EditorBuildSettingsScene[scenes.Length];
            for (var i = 0; i < scenes.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene(scenes[i], true);
            }

            EditorBuildSettings.scenes = buildScenes;
        }

        private static GameObject CreateEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return Object.FindFirstObjectByType<EventSystem>().gameObject;
            }

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            return eventSystem;
        }

        private static GameObject CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.AddComponent<UI.AdaptiveCanvasGuard>();
            return canvasObject;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var image = panel.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.15f, 0.92f);
            var rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            return panel;
        }

        private static void AddLayoutElement(GameObject target, float preferredHeight)
        {
            var layoutElement = target.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = target.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = preferredHeight;
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.flexibleHeight = 0f;
        }

        private static TMP_Text CreateHudText(
            Transform parent,
            string name,
            string text,
            int fontSize,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = new Color(0.94f, 0.96f, 1f, 1f);
            tmp.outlineWidth = 0.18f;
            tmp.outlineColor = new Color(0f, 0f, 0f, 0.85f);
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return tmp;
        }

        private static TMP_Text CreateText(Transform parent, string name, string text, int fontSize, Vector2 position)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            var rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 60);
            rect.anchoredPosition = position;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 position)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.2f, 0.45f, 0.85f, 1f);
            var button = buttonObject.AddComponent<Button>();
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(140, 40);
            rect.anchoredPosition = position;

            var text = CreateText(buttonObject.transform, "Label", label, 18, Vector2.zero);
            text.rectTransform.sizeDelta = new Vector2(140, 40);
            return button;
        }

        private static TMP_Dropdown CreateDropdown(Transform parent, string name, Vector2 position)
        {
            var dropdownObject = new GameObject(name);
            dropdownObject.transform.SetParent(parent, false);
            var image = dropdownObject.AddComponent<Image>();
            image.color = Color.white;
            var dropdown = dropdownObject.AddComponent<TMP_Dropdown>();
            var rect = dropdownObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 40);
            rect.anchoredPosition = position;

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(dropdownObject.transform, false);
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.fontSize = 18;
            label.color = Color.black;
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10, 0);
            labelRect.offsetMax = new Vector2(-30, 0);

            var template = new GameObject("Template");
            template.transform.SetParent(dropdownObject.transform, false);
            template.SetActive(false);
            var templateImage = template.AddComponent<Image>();
            templateImage.color = Color.white;
            var templateRect = template.GetComponent<RectTransform>();
            templateRect.sizeDelta = new Vector2(300, 150);

            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            viewport.AddComponent<Image>().color = Color.white;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 28);

            var item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            var itemToggle = item.AddComponent<Toggle>();
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(300, 28);
            var itemLabelObject = new GameObject("Item Label");
            itemLabelObject.transform.SetParent(item.transform, false);
            var itemLabel = itemLabelObject.AddComponent<TextMeshProUGUI>();
            itemLabel.fontSize = 16;
            itemLabel.color = Color.black;
            var itemLabelRect = itemLabelObject.GetComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(10, 0);
            itemLabelRect.offsetMax = Vector2.zero;

            dropdown.captionText = label;
            dropdown.template = templateRect;
            dropdown.itemText = itemLabel;

            return dropdown;
        }

        private static RectTransform CreateImageMarker(Transform parent)
        {
            var marker = new GameObject("MarkerPrefab");
            marker.transform.SetParent(parent, false);
            var image = marker.AddComponent<Image>();
            image.color = Color.yellow;
            var rect = marker.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(8, 8);
            marker.SetActive(false);
            return rect;
        }

        private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void SetMode(GameModeDefinition mode, string displayName, string sceneName, LevelDefinition[] levels)
        {
            SetPrivateField(mode, "displayName", displayName);
            SetPrivateField(mode, "sceneName", sceneName);
            SetPrivateField(mode, "levels", levels);
            EditorUtility.SetDirty(mode);
        }

        private static void SetLevel(
            LevelDefinition level,
            string displayName,
            int difficulty,
            float enemySpeed,
            int enemyCount,
            int pickupCount,
            int spatialCount,
            string sceneOverride = null)
        {
            SetPrivateField(level, "displayName", displayName);
            SetPrivateField(level, "difficultyIndex", difficulty);
            SetPrivateField(level, "enemySpeed", enemySpeed);
            SetPrivateField(level, "enemyCount", enemyCount);
            SetPrivateField(level, "pickupCount", pickupCount);
            SetPrivateField(level, "spatialEntityCount", spatialCount);
            SetPrivateField(level, "sceneOverride", sceneOverride ?? string.Empty);
            EditorUtility.SetDirty(level);
        }

        private static void SetCatalog(GameCatalog catalog, GameModeDefinition[] modes)
        {
            SetPrivateField(catalog, "modes", modes);
            EditorUtility.SetDirty(catalog);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var type = target.GetType();
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field == null)
            {
                field = type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            }

            field?.SetValue(target, value);
        }
    }
}
#endif

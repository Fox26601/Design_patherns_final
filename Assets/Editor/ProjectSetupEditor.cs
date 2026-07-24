#if UNITY_EDITOR
using System.IO;
using Core;
using Part1_TicTacToe;
using Part2_Adventure;
using Part4_UnseenPattern;
using TMPro;
using UI;
using UI.Screens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace EditorTools
{
    public static class ProjectSetupEditor
    {
        private const string ScenesPath = "Assets/Scenes";
        private const string DataPath = "Assets/Core/Data";
        private const string ThemePath = "Assets/UI/Data/UiTheme.asset";
        private const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        [MenuItem("DesignPatterns/Setup Project")]
        public static void SetupProject()
        {
            EnsureFolders();
            CreateOrUpdateTheme();
            CreateDataAssets();
            CreateBootstrapScene();
            CreateMainMenuScene();
            CreateTicTacToeScene();
            CreateAdventureScene();
            CreateUnseenDemoScene();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Design Patterns project setup complete.");
        }

        [MenuItem("DesignPatterns/Rebuild UI")]
        public static void RebuildUi()
        {
            EnsureFolders();
            CreateOrUpdateTheme();
            CreateBootstrapScene();
            CreateMainMenuScene();
            CreateTicTacToeScene();
            CreateAdventureScene();
            CreateUnseenDemoScene();
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("UI rebuilt with UiTheme + UiFactory. Open Bootstrap and press Play.");
        }

        [MenuItem("DesignPatterns/Fix Missing Cameras")]
        public static void FixMissingCameras()
        {
            EnsureCameraInScene($"{ScenesPath}/MainMenu.unity", solidColor: true);
            EnsureCameraInScene($"{ScenesPath}/Bootstrap.unity", solidColor: true);
            EnsureCameraInScene($"{ScenesPath}/TicTacToe.unity", solidColor: true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Cameras ensured in MainMenu, Bootstrap, TicTacToe.");
        }

        private static void EnsureCameraInScene(string scenePath, bool solidColor)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (Object.FindFirstObjectByType<Camera>() == null)
            {
                CreateSceneCamera(solidColor);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static void CreateSceneCamera(bool solidColor)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.depth = -1f;
            if (solidColor)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
            }
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(ScenesPath);
            Directory.CreateDirectory(DataPath);
            Directory.CreateDirectory("Assets/Part2_Adventure/Data");
            Directory.CreateDirectory("Assets/UI/Data");
        }

        private static UiTheme CreateOrUpdateTheme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UiTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<UiTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
            if (font == null && TMP_Settings.defaultFontAsset != null)
            {
                font = TMP_Settings.defaultFontAsset;
            }

            theme.ApplyReadableDefaults(font);
            EditorUtility.SetDirty(theme);
            return theme;
        }

        private static void NormalizeCanvasTransforms()
        {
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var rect = canvas.GetComponent<RectTransform>();
                if (rect == null)
                {
                    continue;
                }

                // Batchmode saves can bake scale (0,0,0) into overlay canvases.
                if (rect.localScale.sqrMagnitude < 0.01f)
                {
                    rect.localScale = Vector3.one;
                }

                if (!canvas.TryGetComponent<AdaptiveCanvasGuard>(out _))
                {
                    canvas.gameObject.AddComponent<AdaptiveCanvasGuard>();
                }
            }
        }

        private static UiTheme GetTheme()
        {
            return CreateOrUpdateTheme();
        }

        private static GameCatalog CreateDataAssets()
        {
            GetOrCreateAsset<PickupEventChannel>("Assets/Part2_Adventure/Data/PickupEventChannel.asset");
            GetOrCreateAsset<ScoreService>("Assets/Part2_Adventure/Data/ScoreService.asset");

            var tttLevel = GetOrCreateAsset<LevelDefinition>($"{DataPath}/TTT_SingleMatch.asset");
            SetLevel(tttLevel, "Single Match", 0, 2f, 0, 0, 0);

            var advEasy = GetOrCreateAsset<LevelDefinition>($"{DataPath}/Adventure_Easy.asset");
            SetLevel(advEasy, "Easy", 0, 1.5f, 2, 5, 0);
            var advNormal = GetOrCreateAsset<LevelDefinition>($"{DataPath}/Adventure_Normal.asset");
            SetLevel(advNormal, "Normal", 1, 2.5f, 4, 7, 0);
            var advHard = GetOrCreateAsset<LevelDefinition>($"{DataPath}/Adventure_Hard.asset");
            SetLevel(advHard, "Hard", 2, 4f, 6, 10, 0);

            var unseenSmall = GetOrCreateAsset<LevelDefinition>($"{DataPath}/Unseen_Small.asset");
            SetLevel(unseenSmall, "50 Entities", 0, 0f, 0, 0, 50);
            var unseenLarge = GetOrCreateAsset<LevelDefinition>($"{DataPath}/Unseen_Large.asset");
            SetLevel(unseenLarge, "200 Entities", 1, 0f, 0, 0, 200);

            var tttMode = GetOrCreateAsset<GameModeDefinition>($"{DataPath}/Mode_TicTacToe.asset");
            SetMode(tttMode, "Tic Tac Toe", "TicTacToe", new[] { tttLevel });
            var advMode = GetOrCreateAsset<GameModeDefinition>($"{DataPath}/Mode_Adventure.asset");
            SetMode(advMode, "Adventure", "Adventure", new[] { advEasy, advNormal, advHard });
            var unseenMode = GetOrCreateAsset<GameModeDefinition>($"{DataPath}/Mode_Unseen.asset");
            SetMode(unseenMode, "Spatial Partition Demo", "UnseenDemo", new[] { unseenSmall, unseenLarge });

            var catalog = GetOrCreateAsset<GameCatalog>($"{DataPath}/GameCatalog.asset");
            SetCatalog(catalog, new[] { tttMode, advMode, unseenMode });
            return catalog;
        }

        private static void CreateBootstrapScene()
        {
            var theme = GetTheme();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateSceneCamera(solidColor: true);

            var managers = new GameObject("PersistentManagers");
            var flow = managers.AddComponent<GameFlowManager>();
            managers.AddComponent<SceneLoaderService>();

            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>($"{DataPath}/GameCatalog.asset");
            SetPrivateField(flow, "catalog", catalog);
            SetPrivateField(flow, "mainMenuSceneName", "MainMenu");

            CreatePersistentUiShell(theme);

            var bootstrap = new GameObject("BootstrapLoader");
            bootstrap.AddComponent<BootstrapLoader>();

            NormalizeCanvasTransforms();
            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Bootstrap.unity");
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateSceneCamera(solidColor: true);
            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/MainMenu.unity");
        }

        private static ScreenManager CreatePersistentUiShell(UiTheme theme)
        {
            var eventSystem = CreateEventSystem();

            var canvas = UiFactory.CreateScaledCanvas("PersistentUI", theme);
            canvas.AddComponent<PersistentUIInitializer>();
            if (eventSystem != null)
            {
                eventSystem.transform.SetParent(canvas.transform, false);
            }

            var screenManagerObject = new GameObject("ScreenManager");
            screenManagerObject.transform.SetParent(canvas.transform, false);
            var screenManager = screenManagerObject.AddComponent<ScreenManager>();

            var mainMenuRoot = CreateScreenRoot(canvas.transform, "MainMenuScreen", theme);
            var mainMenuScreen = mainMenuRoot.AddComponent<MainMenuScreen>();
            var mainCard = UiFactory.CreateMenuCard(mainMenuRoot.transform, "Card", theme);
            UiFactory.CreateFlexibleSpacer(mainCard.transform, 0.8f);
            UiFactory.CreateText(mainCard.transform, "Title", "Design Patterns Games", theme.TitleSize, theme);
            var dropdown = UiFactory.CreateDropdown(mainCard.transform, "ModeDropdown", theme);
            var mainButtons = UiFactory.CreateHorizontalStack(mainCard.transform, "Buttons", theme.Spacing, theme.ButtonHeight);
            var playButton = UiFactory.CreateButton(mainButtons.transform, "PlayButton", "Play", theme, primary: true);
            var quitButton = UiFactory.CreateButton(mainButtons.transform, "QuitButton", "Quit", theme, primary: false);
            UiFactory.CreateFlexibleSpacer(mainCard.transform, 0.8f);
            SetPrivateField(mainMenuScreen, "modeDropdown", dropdown);
            SetPrivateField(mainMenuScreen, "playButton", playButton);
            SetPrivateField(mainMenuScreen, "quitButton", quitButton);

            var levelRoot = CreateScreenRoot(canvas.transform, "LevelSelectScreen", theme);
            levelRoot.SetActive(false);
            var levelSelectScreen = levelRoot.AddComponent<LevelSelectScreen>();
            var levelCard = UiFactory.CreateMenuCard(levelRoot.transform, "Card", theme);
            var levelTitle = UiFactory.CreateText(levelCard.transform, "Title", "Select Level", theme.TitleSize, theme);
            var levelContainer = UiFactory.CreateVerticalStack(levelCard.transform, "LevelButtonContainer", theme.Spacing);
            var levelLayout = levelContainer.GetComponent<LayoutElement>();
            levelLayout.minHeight = theme.ButtonHeight * 3.5f;
            levelLayout.preferredHeight = theme.ButtonHeight * 3.5f;
            levelLayout.flexibleHeight = 1f;
            var levelButtonPrefab = UiFactory.CreateButton(levelContainer.transform, "LevelButtonPrefab", "Level", theme);
            levelButtonPrefab.gameObject.SetActive(false);
            var backButton = UiFactory.CreateButton(levelCard.transform, "BackButton", "Back", theme, primary: false);
            SetPrivateField(levelSelectScreen, "levelButtonContainer", levelContainer.transform);
            SetPrivateField(levelSelectScreen, "levelButtonPrefab", levelButtonPrefab);
            SetPrivateField(levelSelectScreen, "backButton", backButton);
            SetPrivateField(levelSelectScreen, "titleText", levelTitle);

            var pauseRoot = CreateScreenRoot(canvas.transform, "PauseScreen", theme);
            pauseRoot.SetActive(false);
            var pauseScreen = pauseRoot.AddComponent<PauseScreen>();
            var pauseCard = UiFactory.CreateMenuCard(pauseRoot.transform, "Card", theme);
            UiFactory.CreateFlexibleSpacer(pauseCard.transform, 1f);
            UiFactory.CreateText(pauseCard.transform, "Title", "Paused", theme.TitleSize, theme);
            var resumeBtn = UiFactory.CreateButton(pauseCard.transform, "ResumeButton", "Resume", theme);
            var restartBtn = UiFactory.CreateButton(pauseCard.transform, "RestartButton", "Restart", theme, primary: false);
            var levelBtn = UiFactory.CreateButton(pauseCard.transform, "LevelSelectButton", "Main Menu", theme, primary: false);
            var menuBtn = UiFactory.CreateButton(pauseCard.transform, "MainMenuButton", "Quit To Menu", theme, primary: false);
            UiFactory.CreateFlexibleSpacer(pauseCard.transform, 1f);
            SetPrivateField(pauseScreen, "resumeButton", resumeBtn);
            SetPrivateField(pauseScreen, "restartButton", restartBtn);
            SetPrivateField(pauseScreen, "levelSelectButton", levelBtn);
            SetPrivateField(pauseScreen, "mainMenuButton", menuBtn);

            var gameOverRoot = CreateScreenRoot(canvas.transform, "GameOverScreen", theme);
            gameOverRoot.SetActive(false);
            var gameOverScreen = gameOverRoot.AddComponent<GameOverScreen>();
            var gameOverCard = UiFactory.CreateMenuCard(gameOverRoot.transform, "Card", theme);
            UiFactory.CreateFlexibleSpacer(gameOverCard.transform, 1f);
            var goMessage = UiFactory.CreateText(gameOverCard.transform, "Message", "Game Over", theme.TitleSize, theme);
            var goButtons = UiFactory.CreateHorizontalStack(gameOverCard.transform, "Buttons", theme.Spacing, theme.ButtonHeight);
            var retryBtn = UiFactory.CreateButton(goButtons.transform, "RetryButton", "Retry", theme);
            var goMenuBtn = UiFactory.CreateButton(goButtons.transform, "MainMenuButton", "Main Menu", theme, primary: false);
            UiFactory.CreateFlexibleSpacer(gameOverCard.transform, 1f);
            SetPrivateField(gameOverScreen, "messageText", goMessage);
            SetPrivateField(gameOverScreen, "retryButton", retryBtn);
            SetPrivateField(gameOverScreen, "mainMenuButton", goMenuBtn);

            var loadingRoot = CreateScreenRoot(canvas.transform, "LoadingScreen", theme);
            loadingRoot.SetActive(false);
            var loadingScreen = loadingRoot.AddComponent<LoadingScreen>();
            var loadingCard = UiFactory.CreateMenuCard(loadingRoot.transform, "Card", theme);
            UiFactory.CreateFlexibleSpacer(loadingCard.transform, 1f);
            var loadingText = UiFactory.CreateText(loadingCard.transform, "Progress", "Loading...", theme.BodySize, theme);
            UiFactory.CreateFlexibleSpacer(loadingCard.transform, 1f);
            SetPrivateField(loadingScreen, "progressText", loadingText);

            SetPrivateField(screenManager, "mainMenuScreen", mainMenuScreen);
            SetPrivateField(screenManager, "levelSelectScreen", levelSelectScreen);
            SetPrivateField(screenManager, "pauseScreen", pauseScreen);
            SetPrivateField(screenManager, "gameOverScreen", gameOverScreen);
            SetPrivateField(screenManager, "loadingScreen", loadingScreen);

            return screenManager;
        }

        private static GameObject CreateScreenRoot(Transform parent, string name, UiTheme theme)
        {
            var root = UiFactory.CreateFullScreenOverlay(parent, name, theme.OverlayColor);
            return root;
        }

        private static void CreateTicTacToeScene()
        {
            var theme = GetTheme();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateSceneCamera(solidColor: true);

            var managerObject = new GameObject("TicTacToeGameManager");
            managerObject.AddComponent<TicTacToeGameManager>();

            var canvas = UiFactory.CreateScaledCanvas("TicTacToeCanvas", theme);
            var boardRoot = UiFactory.CreateMenuCard(canvas.transform, "BoardView", theme);
            var boardView = boardRoot.AddComponent<BoardView>();

            var status = UiFactory.CreateText(boardRoot.transform, "Status", "Turn: X", theme.BodySize, theme);
            var score = UiFactory.CreateText(boardRoot.transform, "Score", "X: 0   O: 0", theme.BodySize, theme, theme.MutedTextColor);

            var grid = new GameObject("Grid");
            grid.transform.SetParent(boardRoot.transform, false);
            var gridLayoutElement = grid.AddComponent<LayoutElement>();
            gridLayoutElement.minHeight = 420f;
            gridLayoutElement.preferredHeight = 420f;
            gridLayoutElement.flexibleHeight = 1f;
            var gridLayout = grid.AddComponent<GridLayoutGroup>();
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 3;
            gridLayout.cellSize = new Vector2(130f, 130f);
            gridLayout.spacing = new Vector2(16f, 16f);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            var buttons = new Button[9];
            for (var i = 0; i < 9; i++)
            {
                buttons[i] = UiFactory.CreateButton(grid.transform, $"Cell_{i}", string.Empty, theme, primary: false);
                var cellLayout = buttons[i].GetComponent<LayoutElement>();
                if (cellLayout != null)
                {
                    Object.DestroyImmediate(cellLayout);
                }
            }

            var actions = UiFactory.CreateHorizontalStack(boardRoot.transform, "Actions", theme.Spacing, theme.ButtonHeight);
            var undo = UiFactory.CreateButton(actions.transform, "UndoButton", "Undo", theme, primary: false);
            var redo = UiFactory.CreateButton(actions.transform, "RedoButton", "Redo", theme, primary: false);
            var restart = UiFactory.CreateButton(actions.transform, "RestartButton", "Restart", theme);

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

            NormalizeCanvasTransforms();
            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/TicTacToe.unity");
        }

        private static void CreateAdventureScene()
        {
            var theme = GetTheme();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Ground";
            plane.transform.localScale = new Vector3(4f, 1f, 4f);
            plane.AddComponent<CheckerboardGround>();

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.AddComponent<Rigidbody>();
            player.AddComponent<PlayerController>();
            player.GetComponent<CapsuleCollider>().height = 2f;

            var enemyPrefab = CreateEnemyPrefab();
            var pickupPrefab = CreatePickupPrefab();
            var spawnRoot = new GameObject("SpawnRoot").transform;

            var canvas = UiFactory.CreateScaledCanvas("AdventureHUD", theme);
            var scoreText = UiFactory.CreateHudText(
                canvas.transform,
                "ScoreText",
                "Score: 0",
                theme,
                TextAnchor.UpperLeft,
                new Vector2(520f, 80f));
            var scoreViewObject = scoreText.gameObject.AddComponent<ScoreView>();
            var scoreService = AssetDatabase.LoadAssetAtPath<ScoreService>("Assets/Part2_Adventure/Data/ScoreService.asset");
            SetPrivateField(scoreViewObject, "scoreService", scoreService);
            SetPrivateField(scoreViewObject, "scoreText", scoreText);

            var mapPanel = new GameObject("Minimap");
            mapPanel.transform.SetParent(canvas.transform, false);
            var mapImage = mapPanel.AddComponent<Image>();
            mapImage.color = new Color(0.08f, 0.1f, 0.14f, 0.85f);
            var mapRoot = mapPanel.GetComponent<RectTransform>();
            UiFactory.ApplyCorner(mapRoot, TextAnchor.UpperRight, new Vector2(280f, 280f), new Vector2(32f, 32f));
            var markerPrefab = CreateImageMarker(mapPanel.transform);
            var minimap = mapPanel.AddComponent<MinimapController>();
            var pickupChannel = AssetDatabase.LoadAssetAtPath<PickupEventChannel>("Assets/Part2_Adventure/Data/PickupEventChannel.asset");
            SetPrivateField(minimap, "mapRoot", mapRoot);
            SetPrivateField(minimap, "markerPrefab", markerPrefab);
            SetPrivateField(minimap, "pickupEventChannel", pickupChannel);
            SetPrivateField(minimap, "worldCenter", player.transform);
            SetPrivateField(minimap, "worldSize", 40f);
            SetPrivateField(minimap, "mapSize", 90f);

            var controllerObject = new GameObject("AdventureController");
            var controller = controllerObject.AddComponent<AdventureGameController>();
            SetPrivateField(controller, "player", player.transform);
            SetPrivateField(controller, "enemyPrefab", enemyPrefab);
            SetPrivateField(controller, "pickupPrefab", pickupPrefab);
            SetPrivateField(controller, "pickupEventChannel", pickupChannel);
            SetPrivateField(controller, "scoreService", scoreService);
            SetPrivateField(controller, "minimap", minimap);
            SetPrivateField(controller, "spawnRoot", spawnRoot);
            SetPrivateField(controller, "spawnRadius", 15f);

            new GameObject("GamePauseHandler").AddComponent<GamePauseHandler>();

            NormalizeCanvasTransforms();
            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/Adventure.unity");
        }

        private static void CreateUnseenDemoScene()
        {
            var theme = GetTheme();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.localScale = new Vector3(3f, 1f, 3f);

            var canvas = UiFactory.CreateScaledCanvas("UnseenHUD", theme);
            var status = UiFactory.CreateHudText(
                canvas.transform,
                "Status",
                "Spatial Partition Demo",
                theme,
                TextAnchor.UpperLeft,
                new Vector2(720f, 280f));
            status.fontSize = theme.BodySize;

            var demoObject = new GameObject("SpatialPartitionDemo");
            var demo = demoObject.AddComponent<SpatialPartitionDemo>();
            SetPrivateField(demo, "statusText", status);
            SetPrivateField(demo, "queryRadius", 8f);
            SetPrivateField(demo, "cellSize", 4f);

            new GameObject("GamePauseHandler").AddComponent<GamePauseHandler>();

            NormalizeCanvasTransforms();
            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/UnseenDemo.unity");
        }

        private static GameObject CreateEnemyPrefab()
        {
            const string path = "Assets/Part2_Adventure/Enemy.prefab";
            var existed = AssetDatabase.LoadAssetAtPath<GameObject>(path) != null;
            GameObject enemyRoot;
            if (existed)
            {
                enemyRoot = PrefabUtility.LoadPrefabContents(path);
            }
            else
            {
                enemyRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                enemyRoot.name = "Enemy";
            }

            if (enemyRoot.GetComponent<EnemyController>() == null)
            {
                enemyRoot.AddComponent<EnemyController>();
            }

            var body = enemyRoot.GetComponent<Rigidbody>();
            if (body == null)
            {
                body = enemyRoot.AddComponent<Rigidbody>();
            }

            body.isKinematic = true;
            body.useGravity = false;

            var collider = enemyRoot.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            PrefabUtility.SaveAsPrefabAsset(enemyRoot, path);
            if (existed)
            {
                PrefabUtility.UnloadPrefabContents(enemyRoot);
            }
            else
            {
                Object.DestroyImmediate(enemyRoot);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
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
            pickup.GetComponent<CapsuleCollider>().isTrigger = true;
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
                $"{ScenesPath}/UnseenDemo.unity"
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

        private static RectTransform CreateImageMarker(Transform parent)
        {
            var marker = new GameObject("MarkerPrefab");
            marker.transform.SetParent(parent, false);
            var image = marker.AddComponent<Image>();
            image.color = Color.yellow;
            var rect = marker.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(12f, 12f);
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
            SetPrivateField(mode, "<DisplayName>k__BackingField", displayName);
            SetPrivateField(mode, "<SceneName>k__BackingField", sceneName);
            SetPrivateField(mode, "<Levels>k__BackingField", levels);
            EditorUtility.SetDirty(mode);
        }

        private static void SetLevel(
            LevelDefinition level,
            string displayName,
            int difficulty,
            float enemySpeed,
            int enemyCount,
            int pickupCount,
            int spatialCount)
        {
            SetPrivateField(level, "<DisplayName>k__BackingField", displayName);
            SetPrivateField(level, "<DifficultyIndex>k__BackingField", difficulty);
            SetPrivateField(level, "<EnemySpeed>k__BackingField", enemySpeed);
            SetPrivateField(level, "<EnemyCount>k__BackingField", enemyCount);
            SetPrivateField(level, "<PickupCount>k__BackingField", pickupCount);
            SetPrivateField(level, "<SpatialEntityCount>k__BackingField", spatialCount);
            EditorUtility.SetDirty(level);
        }

        private static void SetCatalog(GameCatalog catalog, GameModeDefinition[] modes)
        {
            SetPrivateField(catalog, "<Modes>k__BackingField", modes);
            EditorUtility.SetDirty(catalog);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var type = target.GetType();
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        ?? type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            field?.SetValue(target, value);
        }
    }
}
#endif

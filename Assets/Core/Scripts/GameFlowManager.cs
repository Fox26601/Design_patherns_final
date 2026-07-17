using System;
using System.Collections;
using Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Orchestrates game mode/level selection and scene transitions.
    /// Pattern: Singleton (https://www.unitydesignpatterns.com/patterns/singleton)
    /// </summary>
    public class GameFlowManager : Singleton<GameFlowManager>
    {
        [SerializeField] private GameCatalog catalog;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        public GameCatalog Catalog => catalog;
        public GameModeDefinition SelectedMode { get; private set; }
        public LevelDefinition SelectedLevel { get; private set; }
        public bool IsPaused { get; private set; }

        public event Action<GameModeDefinition> OnModeSelected;
        public event Action<LevelDefinition> OnLevelSelected;
        public event Action<bool> OnPauseChanged;

        protected override void OnSingletonAwake()
        {
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        public void SelectMode(int index)
        {
            if (catalog == null || catalog.Modes == null || index < 0 || index >= catalog.Modes.Length)
            {
                return;
            }

            SelectedMode = catalog.Modes[index];
            OnModeSelected?.Invoke(SelectedMode);
        }

        public void SelectLevel(int index)
        {
            if (SelectedMode == null || SelectedMode.Levels == null || index < 0 || index >= SelectedMode.Levels.Length)
            {
                return;
            }

            SelectedLevel = SelectedMode.Levels[index];
            OnLevelSelected?.Invoke(SelectedLevel);
        }

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
            {
                return;
            }

            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            OnPauseChanged?.Invoke(paused);
        }

        public void LoadSelectedLevel()
        {
            if (SelectedMode == null || string.IsNullOrEmpty(SelectedMode.SceneName))
            {
                Debug.LogError("Cannot load level: mode or scene name is missing.");
                return;
            }

            if (SelectedLevel == null && SelectedMode.Levels != null && SelectedMode.Levels.Length > 0)
            {
                SelectLevel(0);
            }

            var sceneName = SelectedMode.SceneName;
            if (SelectedLevel != null && !string.IsNullOrEmpty(SelectedLevel.SceneOverride))
            {
                sceneName = SelectedLevel.SceneOverride;
            }

            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.EnterGameplay();
            }

            SceneLoaderService.Instance.LoadScene(sceneName);
        }

        public void RestartCurrentLevel()
        {
            SetPaused(false);
            var active = SceneManager.GetActiveScene().name;
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.EnterGameplay();
            }

            // Restart must be reliable even if async loader state is stale.
            SceneManager.LoadScene(active, LoadSceneMode.Single);
        }

        public void ReturnToMainMenu()
        {
            SetPaused(false);
            SelectedMode = null;
            SelectedLevel = null;
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.ShowMainMenu();
            }

            SceneLoaderService.Instance.LoadScene(mainMenuSceneName);
        }
    }
}

using System.Collections.Generic;
using Shared;
using UI.Screens;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Manages UI screen stack for menu navigation.
    /// Pattern: State (https://www.unitydesignpatterns.com/patterns/state)
    /// </summary>
    public class ScreenManager : Singleton<ScreenManager>
    {
        private static readonly string[] PauseEnabledScenes =
        {
            "TicTacToe",
            "Adventure",
            "EscapeRoom"
        };

        [SerializeField] private UIScreen mainMenuScreen;
        [SerializeField] private UIScreen levelSelectScreen;
        [SerializeField] private UIScreen pauseScreen;
        [SerializeField] private UIScreen gameOverScreen;
        [SerializeField] private UIScreen loadingScreen;

        private readonly Stack<UIScreen> _screenStack = new();

        protected override void OnSingletonAwake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            HideAll();
            if (mainMenuScreen != null)
            {
                Push(mainMenuScreen);
            }
        }

        public void EnterGameplay()
        {
            while (_screenStack.Count > 0)
            {
                _screenStack.Pop().Hide();
            }
        }

        public void ShowMainMenu()
        {
            ReplaceStack(mainMenuScreen);
        }

        public void ShowLevelSelect()
        {
            Push(levelSelectScreen);
        }

        public bool CanShowPause
        {
            get
            {
                if (pauseScreen == null)
                {
                    return false;
                }

                if (!IsPauseEnabledScene(SceneManager.GetActiveScene().name))
                {
                    return false;
                }

                if (gameOverScreen != null && gameOverScreen.gameObject.activeInHierarchy)
                {
                    return false;
                }

                if (loadingScreen != null && loadingScreen.gameObject.activeInHierarchy)
                {
                    return false;
                }

                if (_screenStack.Count > 0)
                {
                    var top = _screenStack.Peek();
                    if (top == mainMenuScreen || top == levelSelectScreen || top == gameOverScreen)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static bool IsPauseEnabledScene(string sceneName)
        {
            for (var i = 0; i < PauseEnabledScenes.Length; i++)
            {
                if (PauseEnabledScenes[i] == sceneName)
                {
                    return true;
                }
            }

            return false;
        }

        public void ShowPause()
        {
            if (!CanShowPause)
            {
                return;
            }

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.SetPaused(true);
            }

            Push(pauseScreen);
        }

        public void HidePause()
        {
            if (_screenStack.Count > 0 && _screenStack.Peek() == pauseScreen)
            {
                Pop();
            }

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.SetPaused(false);
            }
        }

        public void ShowGameOver(string message)
        {
            if (gameOverScreen is GameOverScreen screen)
            {
                screen.SetMessage(message);
            }

            Push(gameOverScreen);
        }

        public void ShowLoading(bool visible)
        {
            if (loadingScreen == null)
            {
                return;
            }

            loadingScreen.gameObject.SetActive(visible);
            if (visible)
            {
                loadingScreen.Show();
            }
            else
            {
                loadingScreen.Hide();
            }
        }

        public void Push(UIScreen screen)
        {
            if (screen == null)
            {
                return;
            }

            if (_screenStack.Count > 0)
            {
                _screenStack.Peek().Hide();
            }

            _screenStack.Push(screen);
            screen.Show();
        }

        public void Pop()
        {
            if (_screenStack.Count == 0)
            {
                return;
            }

            var top = _screenStack.Pop();
            top.Hide();

            if (_screenStack.Count > 0)
            {
                _screenStack.Peek().Show();
            }
        }

        private void ReplaceStack(UIScreen screen)
        {
            while (_screenStack.Count > 0)
            {
                _screenStack.Pop().Hide();
            }

            if (screen != null)
            {
                Push(screen);
            }
        }

        private void HideAll()
        {
            if (mainMenuScreen != null) mainMenuScreen.Hide();
            if (levelSelectScreen != null) levelSelectScreen.Hide();
            if (pauseScreen != null) pauseScreen.Hide();
            if (gameOverScreen != null) gameOverScreen.Hide();
            if (loadingScreen != null) loadingScreen.Hide();
        }
    }
}

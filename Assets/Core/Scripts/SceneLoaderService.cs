using System;
using System.Collections;
using Shared;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Handles async scene loading with progress events.
    /// </summary>
    public class SceneLoaderService : Singleton<SceneLoaderService>
    {
        public event Action<float> OnLoadProgress;
        public event Action<string> OnLoadStarted;
        public event Action<string> OnLoadCompleted;

        private bool _isLoading;

        protected override void OnSingletonAwake()
        {
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
        }

        public void LoadScene(string sceneName)
        {
            if (_isLoading || string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _isLoading = true;
            OnLoadStarted?.Invoke(sceneName);

            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                OnLoadProgress?.Invoke(operation.progress);
                yield return null;
            }

            OnLoadProgress?.Invoke(1f);
            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            _isLoading = false;
            OnLoadCompleted?.Invoke(sceneName);
        }
    }
}

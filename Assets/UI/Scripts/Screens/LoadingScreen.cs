using Core;
using TMPro;
using UnityEngine;

namespace UI.Screens
{
    /// <summary>
    /// Loading overlay shown during async scene transitions.
    /// </summary>
    public class LoadingScreen : UIScreen
    {
        [SerializeField] private TMP_Text progressText;

        private void OnEnable()
        {
            if (SceneLoaderService.Instance != null)
            {
                SceneLoaderService.Instance.OnLoadProgress += UpdateProgress;
                SceneLoaderService.Instance.OnLoadStarted += OnLoadStarted;
                SceneLoaderService.Instance.OnLoadCompleted += OnLoadCompleted;
            }
        }

        private void OnDisable()
        {
            if (SceneLoaderService.Instance != null)
            {
                SceneLoaderService.Instance.OnLoadProgress -= UpdateProgress;
                SceneLoaderService.Instance.OnLoadStarted -= OnLoadStarted;
                SceneLoaderService.Instance.OnLoadCompleted -= OnLoadCompleted;
            }
        }

        private void OnLoadStarted(string sceneName)
        {
            Show();
            UpdateProgress(0f);
        }

        private void OnLoadCompleted(string sceneName)
        {
            Hide();
        }

        private void UpdateProgress(float progress)
        {
            if (progressText != null)
            {
                progressText.text = $"Loading... {Mathf.RoundToInt(progress * 100f)}%";
            }
        }
    }
}

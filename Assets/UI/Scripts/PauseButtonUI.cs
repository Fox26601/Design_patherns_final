using Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Persistent Pause button shown during gameplay scenes only.
    /// </summary>
    public class PauseButtonUI : MonoBehaviour
    {
        private static readonly string[] GameplayScenes =
        {
            "TicTacToe",
            "Adventure",
            "UnseenDemo",
            "EscapeRoom"
        };

        private GameObject _canvasRoot;
        private Button _button;

        private void Awake()
        {
            EnsureButton();
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnPauseChanged += OnPauseChanged;
            }

            RefreshVisibility(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnPauseChanged -= OnPauseChanged;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnPauseChanged -= OnPauseChanged;
                GameFlowManager.Instance.OnPauseChanged += OnPauseChanged;
            }

            RefreshVisibility(scene.name);
        }

        private void OnPauseChanged(bool paused)
        {
            RefreshVisibility(SceneManager.GetActiveScene().name);
        }

        private void EnsureButton()
        {
            if (_canvasRoot != null)
            {
                return;
            }

            _canvasRoot = new GameObject("PauseButtonCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvasRoot.transform.SetParent(transform, false);

            var canvasRect = _canvasRoot.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.pivot = new Vector2(0.5f, 0.5f);
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
            canvasRect.localScale = Vector3.one;

            var canvas = _canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;

            var scaler = _canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            var buttonObject = new GameObject("PauseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(_canvasRoot.transform, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-16f, -16f);
            rect.sizeDelta = new Vector2(110f, 40f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.14f, 0.2f, 0.92f);

            _button = buttonObject.GetComponent<Button>();
            _button.onClick.AddListener(OnPauseClicked);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = "Pause";
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            var labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            _canvasRoot.SetActive(false);
        }

        private void OnPauseClicked()
        {
            GamePauseHandler.EnsureExists().TogglePause();
        }

        private void RefreshVisibility(string sceneName)
        {
            EnsureButton();

            var isGameplay = IsGameplayScene(sceneName);
            var paused = GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused;
            _canvasRoot.SetActive(isGameplay && !paused);
        }

        private static bool IsGameplayScene(string sceneName)
        {
            for (var i = 0; i < GameplayScenes.Length; i++)
            {
                if (GameplayScenes[i] == sceneName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

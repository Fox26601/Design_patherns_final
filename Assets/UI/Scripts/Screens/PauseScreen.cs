using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    /// <summary>
    /// Pause overlay available in all gameplay scenes.
    /// </summary>
    public class PauseScreen : UIScreen
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button mainMenuButton;

        protected override void OnShow()
        {
            EnsureCompactCard();

            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResume);
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestart);
            levelSelectButton.onClick.RemoveAllListeners();
            levelSelectButton.onClick.AddListener(OnLevelSelect);
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnQuit);
            SetButtonLabel(mainMenuButton, "Quit");
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            var tmp = button.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (tmp != null)
            {
                tmp.text = label;
            }
        }

        private void EnsureCompactCard()
        {
            var root = transform as RectTransform;
            if (root != null)
            {
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;
            }

            var card = transform.Find("Card") as RectTransform;
            if (card != null)
            {
                card.anchorMin = new Vector2(0.5f, 0.5f);
                card.anchorMax = new Vector2(0.5f, 0.5f);
                card.pivot = new Vector2(0.5f, 0.5f);
                card.anchoredPosition = Vector2.zero;
                card.sizeDelta = new Vector2(360f, 320f);

                foreach (Transform child in card)
                {
                    if (child.name == "Spacer")
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }

            foreach (var button in new[] { resumeButton, restartButton, levelSelectButton, mainMenuButton })
            {
                if (button == null)
                {
                    continue;
                }

                var layout = button.GetComponent<LayoutElement>();
                if (layout != null)
                {
                    layout.preferredHeight = 40f;
                    layout.minHeight = 40f;
                    layout.flexibleHeight = 0f;
                    layout.flexibleWidth = 1f;
                }
            }

            if (card != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(card);
            }
        }

        private void OnResume()
        {
            ScreenManager.Instance.HidePause();
        }

        private void OnRestart()
        {
            ScreenManager.Instance.HidePause();
            GameFlowManager.Instance.RestartCurrentLevel();
        }

        private void OnLevelSelect()
        {
            GameFlowManager.Instance.SetPaused(false);
            ScreenManager.Instance.Pop();
            GameFlowManager.Instance.ReturnToMainMenu();
        }

        private void OnQuit()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

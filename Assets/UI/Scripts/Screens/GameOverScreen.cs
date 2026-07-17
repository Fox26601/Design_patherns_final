using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    /// <summary>
    /// End-of-round screen for games that report results.
    /// </summary>
    public class GameOverScreen : UIScreen
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        public void SetMessage(string message)
        {
            EnsureAdaptiveLayout();
            if (messageText != null)
            {
                messageText.text = message;
            }
        }

        protected override void OnShow()
        {
            EnsureAdaptiveLayout();

            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetry);
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void EnsureAdaptiveLayout()
        {
            // Keep root as full-screen dim overlay (stretch, sizeDelta 0).
            var root = transform as RectTransform;
            if (root != null)
            {
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;
                root.pivot = new Vector2(0.5f, 0.5f);
            }

            var card = transform.Find("Card") as RectTransform;
            if (card != null)
            {
                card.anchorMin = new Vector2(0.5f, 0.5f);
                card.anchorMax = new Vector2(0.5f, 0.5f);
                card.pivot = new Vector2(0.5f, 0.5f);
                card.anchoredPosition = Vector2.zero;
                card.sizeDelta = new Vector2(380f, 240f);

                var vertical = card.GetComponent<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    vertical.padding = new RectOffset(24, 24, 24, 24);
                    vertical.spacing = 16f;
                    vertical.childAlignment = TextAnchor.MiddleCenter;
                    vertical.childControlWidth = true;
                    vertical.childControlHeight = true;
                    vertical.childForceExpandWidth = true;
                    vertical.childForceExpandHeight = false;
                }

                foreach (Transform child in card)
                {
                    if (child.name == "Spacer")
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }

            if (messageText != null)
            {
                messageText.textWrappingMode = TextWrappingModes.Normal;
                messageText.overflowMode = TextOverflowModes.Truncate;
                messageText.enableAutoSizing = true;
                messageText.fontSizeMin = 18f;
                messageText.fontSizeMax = 26f;
                messageText.alignment = TextAlignmentOptions.Center;

                var messageLayout = messageText.GetComponent<LayoutElement>();
                if (messageLayout == null)
                {
                    messageLayout = messageText.gameObject.AddComponent<LayoutElement>();
                }

                messageLayout.minHeight = 64f;
                messageLayout.preferredHeight = 80f;
                messageLayout.flexibleWidth = 1f;
                messageLayout.flexibleHeight = 0f;

                // Stay under Card layout control — do not stretch to fullscreen.
                var messageRect = messageText.rectTransform;
                messageRect.anchorMin = new Vector2(0.5f, 0.5f);
                messageRect.anchorMax = new Vector2(0.5f, 0.5f);
                messageRect.pivot = new Vector2(0.5f, 0.5f);
            }

            NormalizeButton(retryButton);
            NormalizeButton(mainMenuButton);

            var buttons = transform.Find("Card/Buttons");
            if (buttons != null)
            {
                var horizontal = buttons.GetComponent<HorizontalLayoutGroup>();
                if (horizontal != null)
                {
                    horizontal.spacing = 16f;
                    horizontal.childAlignment = TextAnchor.MiddleCenter;
                    horizontal.childForceExpandWidth = false;
                    horizontal.childForceExpandHeight = false;
                    horizontal.childControlWidth = true;
                    horizontal.childControlHeight = true;
                }

                var rowLayout = buttons.GetComponent<LayoutElement>();
                if (rowLayout != null)
                {
                    rowLayout.minHeight = 44f;
                    rowLayout.preferredHeight = 44f;
                    rowLayout.preferredWidth = -1f;
                    rowLayout.flexibleWidth = 1f;
                    rowLayout.flexibleHeight = 0f;
                }
            }

            Canvas.ForceUpdateCanvases();
            if (card != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(card);
            }
        }

        private static void NormalizeButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            var layout = button.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = button.gameObject.AddComponent<LayoutElement>();
            }

            layout.minWidth = 130f;
            layout.preferredWidth = 140f;
            layout.minHeight = 40f;
            layout.preferredHeight = 40f;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;

            var label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.enableAutoSizing = false;
                label.fontSize = 18f;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.overflowMode = TextOverflowModes.Truncate;
            }
        }

        private void OnRetry()
        {
            ScreenManager.Instance.Pop();
            GameFlowManager.Instance.RestartCurrentLevel();
        }

        private void OnMainMenu()
        {
            GameFlowManager.Instance.ReturnToMainMenu();
        }
    }
}

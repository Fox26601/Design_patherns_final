using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    /// <summary>
    /// Level selection screen built from selected game mode data.
    /// </summary>
    public class LevelSelectScreen : UIScreen
    {
        [SerializeField] private Transform levelButtonContainer;
        [SerializeField] private Button levelButtonPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text titleText;

        private readonly List<GameObject> _spawnedButtons = new();

        protected override void OnShow()
        {
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
            }

            RebuildLevelButtons();
        }

        private void RebuildLevelButtons()
        {
            foreach (var buttonObject in _spawnedButtons)
            {
                if (buttonObject != null)
                {
                    Destroy(buttonObject);
                }
            }

            _spawnedButtons.Clear();

            var mode = GameFlowManager.Instance.SelectedMode;
            if (mode == null)
            {
                if (titleText != null)
                {
                    titleText.text = "Select Level";
                }

                return;
            }

            if (titleText != null)
            {
                titleText.text = $"{mode.DisplayName} - Select Level";
            }

            var levels = mode.Levels;
            if (levels == null || levels.Length == 0)
            {
                CreateLevelButton("Play", 0);
                return;
            }

            for (var i = 0; i < levels.Length; i++)
            {
                var level = levels[i];
                var label = level != null && !string.IsNullOrEmpty(level.DisplayName)
                    ? level.DisplayName
                    : $"Level {i + 1}";
                CreateLevelButton(label, i);
            }
        }

        private void CreateLevelButton(string label, int index)
        {
            Button button;
            if (levelButtonPrefab != null)
            {
                button = Instantiate(levelButtonPrefab, levelButtonContainer);
                button.gameObject.SetActive(true);
            }
            else
            {
                button = BuildFallbackButton(label);
            }

            button.name = $"LevelButton_{label}";
            EnsureButtonLayout(button);

            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.gameObject.SetActive(true);
                text.text = label;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnLevelClicked(index));
            _spawnedButtons.Add(button.gameObject);
        }

        private Button BuildFallbackButton(string label)
        {
            var buttonObject = new GameObject($"LevelButton_{label}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(levelButtonContainer, false);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.22f, 0.5f, 0.95f, 1f);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 28f;
            text.color = Color.white;
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonObject.GetComponent<Button>();
        }

        private static void EnsureButtonLayout(Button button)
        {
            var rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(360f, 72f);
            }

            var layoutElement = button.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = button.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = 72f;
            layoutElement.preferredHeight = 72f;
            layoutElement.minWidth = 280f;
            layoutElement.preferredWidth = 360f;
            layoutElement.flexibleWidth = 1f;
        }

        private void OnLevelClicked(int index)
        {
            var mode = GameFlowManager.Instance.SelectedMode;
            if (mode != null && mode.Levels != null && mode.Levels.Length > 0)
            {
                GameFlowManager.Instance.SelectLevel(index);
            }

            GameFlowManager.Instance.LoadSelectedLevel();
        }

        private void OnBackClicked()
        {
            ScreenManager.Instance.Pop();
        }
    }
}

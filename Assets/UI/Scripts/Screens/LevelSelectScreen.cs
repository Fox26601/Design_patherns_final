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

        private readonly List<Button> _spawnedButtons = new();

        protected override void OnShow()
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
            RebuildLevelButtons();
        }

        private void RebuildLevelButtons()
        {
            foreach (var button in _spawnedButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            _spawnedButtons.Clear();

            var mode = GameFlowManager.Instance.SelectedMode;
            if (mode == null)
            {
                titleText.text = "Select Level";
                return;
            }

            titleText.text = $"{mode.DisplayName} - Select Level";

            if (mode.Levels == null)
            {
                return;
            }

            for (var i = 0; i < mode.Levels.Length; i++)
            {
                var level = mode.Levels[i];
                var index = i;
                var button = Instantiate(levelButtonPrefab, levelButtonContainer);
                button.gameObject.SetActive(true);
                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    label.text = level.DisplayName;
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnLevelClicked(index));
                _spawnedButtons.Add(button);
            }
        }

        private void OnLevelClicked(int index)
        {
            GameFlowManager.Instance.SelectLevel(index);
            GameFlowManager.Instance.LoadSelectedLevel();
        }

        private void OnBackClicked()
        {
            ScreenManager.Instance.Pop();
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens
{
    /// <summary>
    /// Main menu with game mode dropdown selection.
    /// </summary>
    public class MainMenuScreen : Core.UIScreen
    {
        [SerializeField] private TMP_Dropdown modeDropdown;
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        protected override void OnShow()
        {
            PopulateDropdown();
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void PopulateDropdown()
        {
            var catalog = Core.GameFlowManager.Instance.Catalog;
            modeDropdown.ClearOptions();

            if (catalog == null || catalog.Modes == null)
            {
                return;
            }

            var options = new System.Collections.Generic.List<string>();
            foreach (var mode in catalog.Modes)
            {
                if (mode == null)
                {
                    continue;
                }

                options.Add(string.IsNullOrEmpty(mode.DisplayName) ? mode.name : mode.DisplayName);
            }

            modeDropdown.AddOptions(options);
            modeDropdown.value = 0;
            Core.GameFlowManager.Instance.SelectMode(0);
            modeDropdown.onValueChanged.RemoveAllListeners();
            modeDropdown.onValueChanged.AddListener(Core.GameFlowManager.Instance.SelectMode);
        }

        private void OnPlayClicked()
        {
            var flow = Core.GameFlowManager.Instance;
            var mode = flow.SelectedMode;
            if (mode == null)
            {
                flow.SelectMode(modeDropdown != null ? modeDropdown.value : 0);
                mode = flow.SelectedMode;
            }

            if (mode == null || string.IsNullOrEmpty(mode.SceneName))
            {
                Debug.LogError("No game mode selected or scene name is empty.");
                return;
            }

            var levelCount = mode.Levels != null ? mode.Levels.Length : 0;
            if (levelCount <= 1)
            {
                if (levelCount == 1)
                {
                    flow.SelectLevel(0);
                }

                flow.LoadSelectedLevel();
                return;
            }

            Core.ScreenManager.Instance.ShowLevelSelect();
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

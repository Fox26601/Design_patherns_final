using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    /// <summary>
    /// Shared pause input handler for gameplay scenes.
    /// </summary>
    public class GamePauseHandler : MonoBehaviour
    {
        /// <summary>
        /// Ensures a pause handler exists in the active scene (or creates one).
        /// </summary>
        public static GamePauseHandler EnsureExists()
        {
            var existing = FindFirstObjectByType<GamePauseHandler>();
            if (existing != null)
            {
                return existing;
            }

            var pauseObject = new GameObject("GamePauseHandler");
            return pauseObject.AddComponent<GamePauseHandler>();
        }

        /// <summary>
        /// Toggles pause menu visibility.
        /// </summary>
        public void TogglePause()
        {
            if (GameFlowManager.Instance == null || ScreenManager.Instance == null)
            {
                return;
            }

            if (GameFlowManager.Instance.IsPaused)
            {
                ScreenManager.Instance.HidePause();
            }
            else
            {
                ScreenManager.Instance.ShowPause();
            }
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            TogglePause();
        }
    }
}

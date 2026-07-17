using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{
    /// <summary>
    /// Shared pause input handler for gameplay scenes.
    /// </summary>
    public class GamePauseHandler : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            var flow = GameFlowManager.Instance;
            var screens = ScreenManager.Instance;
            if (flow == null || screens == null)
            {
                return;
            }

            if (flow.IsPaused)
            {
                screens.HidePause();
                return;
            }

            if (!screens.CanShowPause)
            {
                return;
            }

            screens.ShowPause();
        }

        public static GamePauseHandler EnsureExists()
        {
            var handler = FindFirstObjectByType<GamePauseHandler>();
            if (handler != null)
            {
                return handler;
            }

            var pauseObject = new GameObject("GamePauseHandler");
            return pauseObject.AddComponent<GamePauseHandler>();
        }
    }
}

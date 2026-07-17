using UnityEngine;

namespace Core
{
    /// <summary>
    /// Loads persistent managers then transitions to main menu.
    /// </summary>
    public class BootstrapLoader : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private void Start()
        {
            SceneLoaderService.Instance.LoadScene(mainMenuSceneName);
        }
    }
}

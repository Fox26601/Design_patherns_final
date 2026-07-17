using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GameMode", menuName = "DesignPatterns/Game Mode")]
    public class GameModeDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField] private string sceneName;
        [SerializeField] private LevelDefinition[] levels;

        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public LevelDefinition[] Levels => levels;
    }
}

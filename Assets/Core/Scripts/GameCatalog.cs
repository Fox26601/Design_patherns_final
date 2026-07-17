using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "GameCatalog", menuName = "DesignPatterns/Game Catalog")]
    public class GameCatalog : ScriptableObject
    {
        [SerializeField] private GameModeDefinition[] modes;

        public GameModeDefinition[] Modes => modes;
    }
}

using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "Level", menuName = "DesignPatterns/Level")]
    public class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Level";
        [SerializeField] private int difficultyIndex;
        [SerializeField] private float enemySpeed = 2f;
        [SerializeField] private int enemyCount = 3;
        [SerializeField] private int pickupCount = 5;
        [SerializeField] private int spatialEntityCount = 50;
        [SerializeField] private string sceneOverride;

        public string DisplayName => displayName;
        public int DifficultyIndex => difficultyIndex;
        public float EnemySpeed => enemySpeed;
        public int EnemyCount => enemyCount;
        public int PickupCount => pickupCount;
        public int SpatialEntityCount => spatialEntityCount;
        public string SceneOverride => sceneOverride;
    }
}

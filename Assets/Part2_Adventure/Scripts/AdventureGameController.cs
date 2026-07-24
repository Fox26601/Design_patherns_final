using Core;
using UnityEngine;

namespace Part2_Adventure
{
    /// <summary>
    /// Spawns adventure level content based on selected level definition.
    /// </summary>
    public class AdventureGameController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject pickupPrefab;
        [SerializeField] private PickupEventChannel pickupEventChannel;
        [SerializeField] private ScoreService scoreService;
        [SerializeField] private MinimapController minimap;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private float spawnRadius = 12f;
        [SerializeField] private float worldBound = 18f;

        private void Start()
        {
            if (FindFirstObjectByType<GamePauseHandler>() == null)
            {
                var pauseObject = new GameObject("GamePauseHandler");
                pauseObject.AddComponent<GamePauseHandler>();
            }

            SetupCamera();
            ApplyPlayerVisual();
            EnsurePlayerBounds();
            ApplyCheckerboardGround();

            if (scoreService != null)
            {
                scoreService.ResetScore();
            }

            if (minimap != null && player != null)
            {
                minimap.RegisterPlayer(player);
            }

            SpawnFromSelectedLevel();
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            var follow = cam.GetComponent<CameraFollow>();
            if (follow == null)
            {
                follow = cam.gameObject.AddComponent<CameraFollow>();
            }

            follow.Configure(player);
        }

        private void ApplyPlayerVisual()
        {
            if (player == null)
            {
                return;
            }

            var renderer = player.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = CreateColorMaterial(new Color(0.2f, 0.55f, 1f, 1f));
            }
        }

        private void EnsurePlayerBounds()
        {
            if (player == null)
            {
                return;
            }

            var bounds = player.GetComponent<WorldBoundsClamp>();
            if (bounds == null)
            {
                bounds = player.gameObject.AddComponent<WorldBoundsClamp>();
            }

            bounds.Configure(worldBound);
        }

        private static void ApplyCheckerboardGround()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null)
            {
                var plane = GameObject.Find("Plane");
                ground = plane;
            }

            CheckerboardGround.EnsureOn(ground);
        }

        private void SpawnFromSelectedLevel()
        {
            var enemyCount = 3;
            var pickupCount = 5;
            var enemySpeed = 2f;

            if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedLevel != null)
            {
                var level = GameFlowManager.Instance.SelectedLevel;
                enemyCount = level.EnemyCount;
                pickupCount = level.PickupCount;
                enemySpeed = level.EnemySpeed;
            }

            for (var i = 0; i < enemyCount; i++)
            {
                var position = RandomPointOnPlane();
                var enemy = Instantiate(enemyPrefab, position, Quaternion.identity, spawnRoot);
                ApplyRendererColor(enemy, Color.red);
                EnsureEnemyRigidbody(enemy);
                var controller = enemy.GetComponent<EnemyController>();
                controller.Configure(player, enemySpeed);
                minimap.RegisterEnemy(enemy.transform);
            }

            for (var i = 0; i < pickupCount; i++)
            {
                var position = RandomPointOnPlane();
                var pickupObject = Instantiate(pickupPrefab, position, Quaternion.identity, spawnRoot);
                ApplyRendererColor(pickupObject, Pickup.BrightYellow);
                var pickup = pickupObject.GetComponent<Pickup>();
                pickup.Configure(i, pickupEventChannel);
                minimap.RegisterPickup(i, pickupObject.transform);
            }
        }

        private Vector3 RandomPointOnPlane()
        {
            var point = Random.insideUnitCircle * spawnRadius;
            return new Vector3(point.x, 0.5f, point.y);
        }

        private static void EnsureEnemyRigidbody(GameObject enemy)
        {
            if (enemy.GetComponent<Rigidbody>() == null)
            {
                var body = enemy.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
            }

            var collider = enemy.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        private static void ApplyRendererColor(GameObject obj, Color color)
        {
            var meshRenderer = obj.GetComponent<Renderer>();
            if (meshRenderer == null)
            {
                return;
            }

            meshRenderer.material = CreateColorMaterial(color);
        }

        private static Material CreateColorMaterial(Color color)
        {
            var shader = Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Standard");
            var material = new Material(shader) { color = color };
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            return material;
        }
    }
}

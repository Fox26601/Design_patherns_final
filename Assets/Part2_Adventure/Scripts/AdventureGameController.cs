using Core;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Part2_Adventure
{
    /// <summary>
    /// Spawns adventure level content based on selected level definition.
    /// </summary>
    public class AdventureGameController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private AdventureCameraFollow cameraFollow;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private GameObject pickupPrefab;
        [SerializeField] private PickupEventChannel pickupEventChannel;
        [SerializeField] private ScoreService scoreService;
        [SerializeField] private MinimapController minimap;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private float spawnRadius = 15f;
        [SerializeField] private float minEnemyDistanceFromPlayer = 6f;
        [SerializeField] private float minPickupDistanceFromPlayer = 2.5f;
        [SerializeField] private int floorTileCount = 8;
        [SerializeField] private Color tileDarkColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        [SerializeField] private Color tileLightColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        [SerializeField] private Color enemyColor = new Color(0.82f, 0.18f, 0.18f, 1f);

        private int _pickupsRemaining;
        private bool _isWon;

        private void Start()
        {
            if (FindFirstObjectByType<GamePauseHandler>() == null)
            {
                var pauseObject = new GameObject("GamePauseHandler");
                pauseObject.AddComponent<GamePauseHandler>();
            }

            if (scoreService != null)
            {
                scoreService.ResetScore();
            }

            if (cameraFollow != null)
            {
                cameraFollow.Configure(player);
            }

            var level = GameFlowManager.Instance.SelectedLevel;
            _pickupsRemaining = level != null ? level.PickupCount : 5;
            _isWon = false;
            EnsureInstructionsUI(_pickupsRemaining);

            ApplyGroundVisuals();
            SpawnFromSelectedLevel();
        }

        private void OnEnable()
        {
            if (pickupEventChannel != null)
            {
                pickupEventChannel.Subscribe(OnPickupCollected);
            }
        }

        private void OnDisable()
        {
            if (pickupEventChannel != null)
            {
                pickupEventChannel.Unsubscribe(OnPickupCollected);
            }
        }

        private void SpawnFromSelectedLevel()
        {
            var level = GameFlowManager.Instance.SelectedLevel;
            var enemyCount = level != null ? level.EnemyCount : 3;
            var pickupCount = level != null ? level.PickupCount : 5;
            _pickupsRemaining = pickupCount;
            var enemySpeed = level != null ? level.EnemySpeed : 2f;

            for (var i = 0; i < enemyCount; i++)
            {
                var position = GetSpawnPosition(minEnemyDistanceFromPlayer);
                var enemy = Instantiate(enemyPrefab, position, Quaternion.identity, spawnRoot);
                StyleEnemy(enemy);
                var controller = enemy.GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.Configure(player, enemySpeed);
                }

                if (minimap != null)
                {
                    minimap.RegisterEnemy(enemy.transform);
                }
            }

            for (var i = 0; i < pickupCount; i++)
            {
                var position = GetSpawnPosition(minPickupDistanceFromPlayer);
                var pickupObject = Instantiate(pickupPrefab, position, Quaternion.identity, spawnRoot);
                var pickup = pickupObject.GetComponent<Pickup>();
                if (pickup != null)
                {
                    pickup.Configure(i, pickupEventChannel);
                }

                if (minimap != null)
                {
                    minimap.RegisterPickup(i, pickupObject.transform);
                }
            }
        }

        private void OnPickupCollected(PickupCollectedData _)
        {
            if (_isWon || GameFlowManager.Instance.IsPaused)
            {
                return;
            }

            if (_pickupsRemaining <= 0)
            {
                return;
            }

            _pickupsRemaining--;
            if (_pickupsRemaining == 0)
            {
                _isWon = true;
                GameFlowManager.Instance.SetPaused(true);
                ScreenManager.Instance.ShowGameOver("Victory! All pickups collected.");
            }
        }

        private void EnsureInstructionsUI(int pickupCount)
        {
            var hudRoot = GameObject.Find("AdventureHUD");
            if (hudRoot == null)
            {
                return;
            }

            // Center-top HUD stays visible even when Game view Scale zooms the middle.
            var scoreObj = GameObject.Find("ScoreText");
            if (scoreObj != null)
            {
                var scoreTmp = scoreObj.GetComponent<TMP_Text>();
                if (scoreTmp != null)
                {
                    scoreTmp.enableAutoSizing = false;
                    scoreTmp.fontSize = 20f;
                    scoreTmp.alignment = TextAlignmentOptions.Center;
                    scoreTmp.color = new Color(0.94f, 0.96f, 1f, 1f);
                    scoreTmp.outlineWidth = 0.18f;
                    scoreTmp.outlineColor = new Color(0f, 0f, 0f, 0.85f);
                    scoreTmp.overflowMode = TextOverflowModes.Overflow;

                    var scoreRect = scoreTmp.rectTransform;
                    scoreRect.anchorMin = new Vector2(0.5f, 1f);
                    scoreRect.anchorMax = new Vector2(0.5f, 1f);
                    scoreRect.pivot = new Vector2(0.5f, 1f);
                    scoreRect.anchoredPosition = new Vector2(0f, -16f);
                    scoreRect.sizeDelta = new Vector2(420f, 36f);
                }
            }

            // Minimap layout is owned by MinimapController (responsive to canvas size).

            var instructions =
                "WASD move · Esc pause\n" +
                $"Win: collect all pickups ({pickupCount}) · Lose: enemy touch";

            var existing = hudRoot.transform.Find("InstructionsText");
            TMP_Text tmp;
            if (existing != null)
            {
                tmp = existing.GetComponent<TMP_Text>();
            }
            else
            {
                var sourceTmp = scoreObj != null ? scoreObj.GetComponent<TMP_Text>() : null;
                var textObject = new GameObject("InstructionsText");
                textObject.transform.SetParent(hudRoot.transform, false);
                tmp = textObject.AddComponent<TextMeshProUGUI>();
                if (sourceTmp != null)
                {
                    tmp.font = sourceTmp.font;
                    tmp.fontSharedMaterial = sourceTmp.fontSharedMaterial;
                }
            }

            if (tmp == null)
            {
                return;
            }

            tmp.text = instructions;
            tmp.fontSize = 14f;
            tmp.enableAutoSizing = false;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.94f, 0.96f, 1f, 1f);
            tmp.outlineWidth = 0.18f;
            tmp.outlineColor = new Color(0f, 0f, 0f, 0.85f);
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Overflow;

            var rect = tmp.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -56f);
            rect.sizeDelta = new Vector2(460f, 56f);

            EnsureInstructionBackground(tmp.transform);
        }

        private static void EnsureInstructionBackground(Transform textTransform)
        {
            var parent = textTransform.parent;
            if (parent == null)
            {
                return;
            }

            var existing = parent.Find("InstructionsPanel");
            RectTransform panelRect;
            if (existing != null)
            {
                panelRect = existing as RectTransform;
            }
            else
            {
                var panel = new GameObject("InstructionsPanel");
                panel.transform.SetParent(parent, false);
                panel.transform.SetSiblingIndex(0);
                var image = panel.AddComponent<UnityEngine.UI.Image>();
                image.color = new Color(0.04f, 0.05f, 0.08f, 0.9f);
                image.raycastTarget = false;
                panelRect = panel.GetComponent<RectTransform>();
            }

            if (panelRect == null)
            {
                return;
            }

            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.anchoredPosition = new Vector2(0f, -8f);
            panelRect.sizeDelta = new Vector2(480f, 110f);
        }

        // Picks a point on the ground plane that keeps a minimum distance from the player.
        private Vector3 GetSpawnPosition(float minDistance)
        {
            var playerPosition = player != null ? player.position : Vector3.zero;

            for (var attempt = 0; attempt < 24; attempt++)
            {
                var circle = Random.insideUnitCircle * spawnRadius;
                var candidate = new Vector3(
                    playerPosition.x + circle.x,
                    0.5f,
                    playerPosition.z + circle.y);

                var flatDistance = Vector2.Distance(
                    new Vector2(candidate.x, candidate.z),
                    new Vector2(playerPosition.x, playerPosition.z));

                if (flatDistance >= minDistance)
                {
                    return candidate;
                }
            }

            var angle = Random.value * Mathf.PI * 2f;
            return new Vector3(
                playerPosition.x + Mathf.Cos(angle) * minDistance,
                0.5f,
                playerPosition.z + Mathf.Sin(angle) * minDistance);
        }

        private void ApplyGroundVisuals()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null)
            {
                return;
            }

            // Remove old overlapping tile cubes that caused z-fighting.
            var oldTiles = ground.transform.Find("TilesRoot");
            if (oldTiles != null)
            {
                Destroy(oldTiles.gameObject);
            }

            var meshRenderer = ground.GetComponent<Renderer>();
            if (meshRenderer == null)
            {
                return;
            }

            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            // Hard shadow edges on a flat plane look like floor artifacts in this demo.
            meshRenderer.receiveShadows = false;

            var light = FindFirstObjectByType<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                light.shadows = LightShadows.Soft;
                light.shadowBias = 0.05f;
                light.shadowNormalBias = 1f;
                light.shadowStrength = 0.4f;
            }

            var tiles = Mathf.Max(2, floorTileCount);
            var texture = CreateCheckerTexture(tiles, tileDarkColor, tileLightColor);
            var material = CreateFloorMaterial(texture);
            meshRenderer.material = material;
        }

        private static Texture2D CreateCheckerTexture(int tiles, Color dark, Color light)
        {
            var texture = new Texture2D(tiles, tiles, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "AdventureFloorChecker"
            };

            for (var x = 0; x < tiles; x++)
            {
                for (var y = 0; y < tiles; y++)
                {
                    texture.SetPixel(x, y, (x + y) % 2 == 0 ? dark : light);
                }
            }

            texture.Apply(false, true);
            return texture;
        }

        private static Material CreateFloorMaterial(Texture2D texture)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader)
            {
                name = "AdventureFloorMaterial",
                mainTexture = texture,
                color = Color.white
            };
            material.mainTextureScale = Vector2.one;

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0f);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", 0f);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            return material;
        }

        private void StyleEnemy(GameObject enemy)
        {
            if (enemy == null)
            {
                return;
            }

            var meshRenderer = enemy.GetComponent<Renderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material.color = enemyColor;
            }
        }
    }
}

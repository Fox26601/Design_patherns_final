using System.Collections.Generic;
using System.Diagnostics;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Part4_UnseenPattern
{
    /// <summary>
    /// Demonstrates O(n^2) vs spatial partition query performance.
    /// Pattern: Spatial Partition (https://www.unitydesignpatterns.com/patterns/spatialpartition)
    /// </summary>
    public class SpatialPartitionDemo : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private float queryRadius = 8f;
        [SerializeField] private float cellSize = 4f;
        [SerializeField] private float playerSpeed = 8f;
        [SerializeField] private float worldRadius = 22f;
        [SerializeField] private Color farColor = new(0.35f, 0.35f, 0.4f, 1f);
        [SerializeField] private Color nearColor = new(0.2f, 0.95f, 0.35f, 1f);

        private readonly List<Transform> _entities = new();
        private readonly List<Renderer> _renderers = new();
        private SpatialGrid _grid;
        private bool _useSpatialPartition = true;
        private float _rebuildTimer;
        private Material _farMaterial;
        private Material _nearMaterial;

        private void Start()
        {
            if (FindFirstObjectByType<GamePauseHandler>() == null)
            {
                var pauseObject = new GameObject("GamePauseHandler");
                pauseObject.AddComponent<GamePauseHandler>();
            }

            _entities.Clear();
            _renderers.Clear();
            _grid = new SpatialGrid(cellSize);

            var count = 50;
            if (GameFlowManager.Instance != null && GameFlowManager.Instance.SelectedLevel != null)
            {
                count = GameFlowManager.Instance.SelectedLevel.SpatialEntityCount;
            }

            var farMaterial = CreateColorMaterial(farColor);
            _farMaterial = farMaterial;
            _nearMaterial = CreateColorMaterial(nearColor);

            for (var i = 0; i < count; i++)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var pos = Random.insideUnitSphere * worldRadius;
                cube.transform.position = new Vector3(pos.x, 0.5f, pos.z);
                cube.name = $"Entity_{i}";
                var renderer = cube.GetComponent<Renderer>();
                renderer.sharedMaterial = _farMaterial;
                _entities.Add(cube.transform);
                _renderers.Add(renderer);
                _grid.Insert(cube.transform);
            }

            if (player == null)
            {
                var playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                playerObject.name = "Player";
                playerObject.transform.position = Vector3.zero;
                var playerRenderer = playerObject.GetComponent<Renderer>();
                playerRenderer.sharedMaterial = CreateColorMaterial(new Color(0.2f, 0.55f, 1f, 1f));
                player = playerObject.transform;
            }

            SetupTopDownCamera();
        }

        private void Update()
        {
            if (player == null || _grid == null)
            {
                return;
            }

            MovePlayer();

            _rebuildTimer += Time.deltaTime;
            if (_rebuildTimer >= 0.25f)
            {
                _rebuildTimer = 0f;
                RebuildGrid();
            }

            var playerPos = player.position;
            var sw = Stopwatch.StartNew();
            List<Transform> nearby;

            if (_useSpatialPartition)
            {
                nearby = _grid.QueryNearby(playerPos, queryRadius);
            }
            else
            {
                nearby = new List<Transform>();
                foreach (var entity in _entities)
                {
                    if (entity == null)
                    {
                        continue;
                    }

                    if (Vector3.Distance(entity.position, playerPos) <= queryRadius)
                    {
                        nearby.Add(entity);
                    }
                }
            }

            sw.Stop();
            HighlightNearby(nearby);

            if (statusText != null)
            {
                statusText.text =
                    $"Mode: {(_useSpatialPartition ? "Spatial Partition" : "Brute Force")}\n" +
                    $"Entities: {_entities.Count}\n" +
                    $"Nearby: {nearby.Count}\n" +
                    $"Query: {sw.Elapsed.TotalMilliseconds:F3} ms\n" +
                    "WASD move | T toggle method";
            }

            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                _useSpatialPartition = !_useSpatialPartition;
            }
        }

        private void MovePlayer()
        {
            var input = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) input.y += 1f;
                if (Keyboard.current.sKey.isPressed) input.y -= 1f;
                if (Keyboard.current.aKey.isPressed) input.x -= 1f;
                if (Keyboard.current.dKey.isPressed) input.x += 1f;
            }

            if (input.sqrMagnitude < 0.01f)
            {
                return;
            }

            var direction = new Vector3(input.x, 0f, input.y).normalized;
            var next = player.position + direction * (playerSpeed * Time.deltaTime);
            next.x = Mathf.Clamp(next.x, -worldRadius, worldRadius);
            next.z = Mathf.Clamp(next.z, -worldRadius, worldRadius);
            next.y = 0f;
            player.position = next;
        }

        private void RebuildGrid()
        {
            _grid.Clear();
            foreach (var entity in _entities)
            {
                if (entity != null)
                {
                    _grid.Insert(entity);
                }
            }
        }

        private void HighlightNearby(List<Transform> nearby)
        {
            var nearSet = new HashSet<Transform>(nearby);

            for (var i = 0; i < _entities.Count; i++)
            {
                if (_entities[i] == null || _renderers[i] == null)
                {
                    continue;
                }

                _renderers[i].sharedMaterial = nearSet.Contains(_entities[i]) ? _nearMaterial : _farMaterial;
            }
        }

        private void SetupTopDownCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            cam.orthographic = true;
            cam.orthographicSize = 18f;
            cam.transform.position = new Vector3(0f, 30f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static Material CreateColorMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard")
                         ?? Shader.Find("Unlit/Color");
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            material.color = color;
            return material;
        }
    }
}

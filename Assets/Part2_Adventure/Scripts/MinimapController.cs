using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Part2_Adventure
{
    /// <summary>
    /// Minimap that tracks enemies and pickups via observer events.
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] private RectTransform mapRoot;
        [SerializeField] private RectTransform markerPrefab;
        [SerializeField] private PickupEventChannel pickupEventChannel;
        [SerializeField] private Transform worldCenter;
        [SerializeField] private float worldSize = 40f;
        [SerializeField] private float mapSize = 180f;

        private readonly Dictionary<int, RectTransform> _pickupMarkers = new();
        private RectTransform _playerMarker;

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

        private void Update()
        {
            if (worldCenter != null && _playerMarker != null)
            {
                UpdateMarker(_playerMarker, worldCenter.position);
            }
        }

        public void RegisterPlayer(Transform playerTransform)
        {
            worldCenter = playerTransform;
            _playerMarker = CreateMarker(Color.cyan);
            UpdateMarker(_playerMarker, playerTransform.position);
        }

        public void RegisterPickup(int pickupId, Transform pickupTransform)
        {
            var marker = CreateMarker(Color.yellow);
            _pickupMarkers[pickupId] = marker;
            UpdateMarker(marker, pickupTransform.position);
        }

        public void RegisterEnemy(Transform enemyTransform)
        {
            var marker = CreateMarker(Color.red);
            StartCoroutine(TrackEnemy(marker, enemyTransform));
        }

        private RectTransform CreateMarker(Color color)
        {
            var marker = Instantiate(markerPrefab, mapRoot);
            marker.gameObject.SetActive(true);
            var image = marker.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }

            return marker;
        }

        private System.Collections.IEnumerator TrackEnemy(RectTransform marker, Transform enemy)
        {
            while (enemy != null && marker != null)
            {
                UpdateMarker(marker, enemy.position);
                yield return null;
            }

            if (marker != null)
            {
                Destroy(marker.gameObject);
            }
        }

        private void OnPickupCollected(PickupCollectedData data)
        {
            if (_pickupMarkers.TryGetValue(data.PickupId, out var marker))
            {
                Destroy(marker.gameObject);
                _pickupMarkers.Remove(data.PickupId);
            }
        }

        private void UpdateMarker(RectTransform marker, Vector3 worldPosition)
        {
            if (worldCenter == null || marker == null)
            {
                return;
            }

            var offset = worldPosition - worldCenter.position;
            var normalized = new Vector2(offset.x / worldSize, offset.z / worldSize);
            marker.anchoredPosition = normalized * mapSize;
        }
    }
}

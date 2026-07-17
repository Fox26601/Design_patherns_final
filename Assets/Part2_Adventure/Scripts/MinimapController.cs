using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Part2_Adventure
{
    /// <summary>
    /// Responsive minimap that tracks enemies and pickups via observer events.
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] private RectTransform mapRoot;
        [SerializeField] private RectTransform markerPrefab;
        [SerializeField] private PickupEventChannel pickupEventChannel;
        [SerializeField] private Transform worldCenter;
        [SerializeField] private float worldSize = 40f;

        private readonly Dictionary<int, RectTransform> _pickupMarkers = new();
        private readonly Dictionary<int, Transform> _pickupTargets = new();
        private readonly List<RectTransform> _enemyMarkers = new();
        private RectTransform _canvasRect;
        private RectTransform _playerMarker;
        private RectTransform _titleRect;
        private TMP_Text _titleText;
        private RectTransform _legendRoot;
        private RectTransform _gridHorizontal;
        private RectTransform _gridVertical;
        private float _currentSize = -1f;
        private Sprite _markerSprite;

        private void Awake()
        {
            if (mapRoot == null)
            {
                mapRoot = transform as RectTransform;
            }

            var canvas = GetComponentInParent<Canvas>();
            _canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
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

        private void Start()
        {
            EnsureVisuals();
            EnsurePlayerMarker();
            RefreshLayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            RefreshLayout();
        }

        private void LateUpdate()
        {
            if (_playerMarker != null)
            {
                _playerMarker.anchoredPosition = Vector2.zero;
            }

            foreach (var pair in _pickupMarkers)
            {
                if (_pickupTargets.TryGetValue(pair.Key, out var target) && target != null)
                {
                    UpdateMarker(pair.Value, target.position);
                }
            }
        }

        public void RegisterPickup(int pickupId, Transform pickupTransform)
        {
            var marker = CreateMarker(new Color(1f, 0.88f, 0.29f, 1f), false);
            if (marker == null)
            {
                return;
            }

            marker.name = $"Pickup_{pickupId}";
            _pickupMarkers[pickupId] = marker;
            _pickupTargets[pickupId] = pickupTransform;
            UpdateMarker(marker, pickupTransform.position);
            ApplyMarkerSizes();
        }

        public void RegisterEnemy(Transform enemyTransform)
        {
            var marker = CreateMarker(new Color(1f, 0.27f, 0.27f, 1f), false);
            if (marker == null)
            {
                return;
            }

            marker.name = "EnemyMarker";
            _enemyMarkers.Add(marker);
            StartCoroutine(TrackEnemy(marker, enemyTransform));
            ApplyMarkerSizes();
        }

        private void RefreshLayout()
        {
            if (mapRoot == null)
            {
                return;
            }

            if (_canvasRect == null)
            {
                var canvas = GetComponentInParent<Canvas>();
                _canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
            }

            var canvasSize = _canvasRect != null ? _canvasRect.rect.size : new Vector2(1280f, 720f);
            if (canvasSize.x < 1f || canvasSize.y < 1f)
            {
                return;
            }

            var shortSide = Mathf.Min(canvasSize.x, canvasSize.y);
            var size = Mathf.Clamp(shortSide * 0.22f, 105f, 180f);
            var margin = Mathf.Clamp(size * 0.12f, 10f, 24f);

            mapRoot.anchorMin = Vector2.zero;
            mapRoot.anchorMax = Vector2.zero;
            mapRoot.pivot = Vector2.zero;
            mapRoot.anchoredPosition = new Vector2(margin, margin);
            mapRoot.sizeDelta = new Vector2(size, size);

            if (Mathf.Abs(size - _currentSize) < 0.5f && _titleRect != null)
            {
                return;
            }

            _currentSize = size;
            EnsureVisuals();
            ApplyChromeSizes();
            ApplyMarkerSizes();
        }

        private void EnsureVisuals()
        {
            if (mapRoot == null)
            {
                return;
            }

            var background = mapRoot.GetComponent<Image>();
            if (background != null)
            {
                background.color = new Color(0.025f, 0.045f, 0.075f, 0.94f);
                background.raycastTarget = false;
            }

            if (mapRoot.GetComponent<RectMask2D>() == null)
            {
                mapRoot.gameObject.AddComponent<RectMask2D>();
            }

            var outline = mapRoot.GetComponent<Outline>();
            if (outline == null)
            {
                outline = mapRoot.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new Color(0.25f, 0.7f, 0.9f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);

            if (_gridHorizontal == null)
            {
                _gridHorizontal = CreateOrFindLine("GridHorizontal");
            }

            if (_gridVertical == null)
            {
                _gridVertical = CreateOrFindLine("GridVertical");
            }

            if (_titleRect == null)
            {
                var existingTitle = mapRoot.Find("MinimapTitle");
                if (existingTitle != null)
                {
                    _titleRect = existingTitle as RectTransform;
                    _titleText = existingTitle.GetComponent<TMP_Text>();
                }
                else
                {
                    var titleObject = new GameObject("MinimapTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
                    titleObject.transform.SetParent(mapRoot, false);
                    _titleRect = titleObject.GetComponent<RectTransform>();
                    _titleText = titleObject.GetComponent<TextMeshProUGUI>();
                    _titleText.text = "MINIMAP";
                    _titleText.fontStyle = FontStyles.Bold;
                    _titleText.alignment = TextAlignmentOptions.Top;
                    _titleText.color = new Color(0.75f, 0.9f, 1f, 0.95f);
                    _titleText.raycastTarget = false;
                }
            }

            EnsureAsciiLegend();
            ApplyChromeSizes();
        }

        private void EnsureAsciiLegend()
        {
            var oldLegend = mapRoot.Find("Legend");
            if (oldLegend != null && oldLegend.GetComponent<HorizontalLayoutGroup>() == null)
            {
                Destroy(oldLegend.gameObject);
            }

            if (_legendRoot != null)
            {
                return;
            }

            var existing = mapRoot.Find("Legend");
            if (existing != null)
            {
                _legendRoot = existing as RectTransform;
                return;
            }

            var legendObject = new GameObject("Legend", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            legendObject.transform.SetParent(mapRoot, false);
            _legendRoot = legendObject.GetComponent<RectTransform>();
            _legendRoot.anchorMin = Vector2.zero;
            _legendRoot.anchorMax = new Vector2(1f, 0f);
            _legendRoot.pivot = new Vector2(0.5f, 0f);

            var layout = legendObject.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 6f;
            layout.padding = new RectOffset(6, 6, 2, 2);

            CreateLegendItem(_legendRoot, "YOU", new Color(0.2f, 0.85f, 1f, 1f));
            CreateLegendItem(_legendRoot, "ITEM", new Color(1f, 0.88f, 0.29f, 1f));
            CreateLegendItem(_legendRoot, "ENEMY", new Color(1f, 0.27f, 0.27f, 1f));
        }

        private void CreateLegendItem(Transform parent, string label, Color color)
        {
            var item = new GameObject(label, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            item.transform.SetParent(parent, false);
            var itemLayout = item.GetComponent<HorizontalLayoutGroup>();
            itemLayout.childAlignment = TextAnchor.MiddleCenter;
            itemLayout.spacing = 3f;
            itemLayout.childControlWidth = false;
            itemLayout.childControlHeight = false;
            itemLayout.childForceExpandWidth = false;
            itemLayout.childForceExpandHeight = false;

            var swatchObject = new GameObject("Swatch", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            swatchObject.transform.SetParent(item.transform, false);
            var swatch = swatchObject.GetComponent<Image>();
            swatch.sprite = GetMarkerSprite();
            swatch.color = color;
            swatch.raycastTarget = false;
            var swatchLayout = swatchObject.GetComponent<LayoutElement>();
            swatchLayout.preferredWidth = 8f;
            swatchLayout.preferredHeight = 8f;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            labelObject.transform.SetParent(item.transform, false);
            var tmp = labelObject.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 9f;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = new Color(0.9f, 0.94f, 1f, 0.95f);
            tmp.raycastTarget = false;
            tmp.overflowMode = TextOverflowModes.Truncate;
            var labelLayout = labelObject.GetComponent<LayoutElement>();
            labelLayout.preferredWidth = 36f;
            labelLayout.preferredHeight = 12f;
        }

        private RectTransform CreateOrFindLine(string name)
        {
            var existing = mapRoot.Find(name) as RectTransform;
            if (existing != null)
            {
                return existing;
            }

            var lineObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            lineObject.transform.SetParent(mapRoot, false);
            lineObject.transform.SetAsFirstSibling();
            var line = lineObject.GetComponent<Image>();
            line.sprite = GetMarkerSprite();
            line.color = new Color(0.45f, 0.7f, 0.85f, 0.16f);
            line.raycastTarget = false;
            var rect = line.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        private void ApplyChromeSizes()
        {
            if (mapRoot == null || _currentSize <= 0f)
            {
                return;
            }

            var titleHeight = Mathf.Clamp(_currentSize * 0.12f, 14f, 22f);
            var legendHeight = Mathf.Clamp(_currentSize * 0.11f, 13f, 20f);
            var titleFont = Mathf.Clamp(_currentSize * 0.07f, 9f, 13f);
            var legendFont = Mathf.Clamp(_currentSize * 0.055f, 7f, 10f);
            var swatchSize = Mathf.Clamp(_currentSize * 0.05f, 6f, 9f);

            if (_titleRect != null)
            {
                _titleRect.anchorMin = new Vector2(0f, 1f);
                _titleRect.anchorMax = new Vector2(1f, 1f);
                _titleRect.pivot = new Vector2(0.5f, 1f);
                _titleRect.anchoredPosition = new Vector2(0f, -3f);
                _titleRect.sizeDelta = new Vector2(0f, titleHeight);
            }

            if (_titleText != null)
            {
                _titleText.fontSize = titleFont;
            }

            if (_legendRoot != null)
            {
                _legendRoot.anchoredPosition = new Vector2(0f, 2f);
                _legendRoot.sizeDelta = new Vector2(0f, legendHeight);

                foreach (Transform item in _legendRoot)
                {
                    foreach (Transform child in item)
                    {
                        if (child.name == "Swatch")
                        {
                            var le = child.GetComponent<LayoutElement>();
                            if (le != null)
                            {
                                le.preferredWidth = swatchSize;
                                le.preferredHeight = swatchSize;
                            }

                            child.GetComponent<RectTransform>().sizeDelta = new Vector2(swatchSize, swatchSize);
                        }
                        else if (child.name == "Label")
                        {
                            var tmp = child.GetComponent<TMP_Text>();
                            if (tmp != null)
                            {
                                tmp.fontSize = legendFont;
                            }
                        }
                    }
                }
            }

            if (_gridHorizontal != null)
            {
                _gridHorizontal.sizeDelta = new Vector2(_currentSize, 1f);
            }

            if (_gridVertical != null)
            {
                _gridVertical.sizeDelta = new Vector2(1f, _currentSize);
            }
        }

        private void ApplyMarkerSizes()
        {
            if (_currentSize <= 0f)
            {
                return;
            }

            var pickupEnemySize = Mathf.Clamp(_currentSize * 0.055f, 6f, 10f);
            var playerSize = Mathf.Clamp(_currentSize * 0.07f, 8f, 13f);

            foreach (var marker in _pickupMarkers.Values)
            {
                if (marker != null)
                {
                    marker.sizeDelta = new Vector2(pickupEnemySize, pickupEnemySize);
                }
            }

            foreach (var marker in _enemyMarkers)
            {
                if (marker != null)
                {
                    marker.sizeDelta = new Vector2(pickupEnemySize, pickupEnemySize);
                }
            }

            if (_playerMarker != null)
            {
                _playerMarker.sizeDelta = new Vector2(playerSize, playerSize);
                _playerMarker.SetAsLastSibling();
            }
        }

        private RectTransform CreateMarker(Color color, bool diamond)
        {
            if (markerPrefab == null || mapRoot == null)
            {
                return null;
            }

            var marker = Instantiate(markerPrefab, mapRoot);
            marker.gameObject.SetActive(true);
            marker.localScale = Vector3.one;
            marker.anchorMin = new Vector2(0.5f, 0.5f);
            marker.anchorMax = new Vector2(0.5f, 0.5f);
            marker.pivot = new Vector2(0.5f, 0.5f);

            var image = marker.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = GetMarkerSprite();
                image.color = color;
                image.raycastTarget = false;
            }

            if (diamond)
            {
                marker.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }

            return marker;
        }

        private Sprite GetMarkerSprite()
        {
            if (_markerSprite != null)
            {
                return _markerSprite;
            }

            _markerSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            return _markerSprite;
        }

        private void EnsurePlayerMarker()
        {
            if (_playerMarker != null || mapRoot == null)
            {
                return;
            }

            _playerMarker = CreateMarker(new Color(0.2f, 0.85f, 1f, 1f), true);
            if (_playerMarker != null)
            {
                _playerMarker.name = "PlayerMarker";
                _playerMarker.anchoredPosition = Vector2.zero;
                _playerMarker.SetAsLastSibling();
            }
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
                _enemyMarkers.Remove(marker);
                Destroy(marker.gameObject);
            }
        }

        private void OnPickupCollected(PickupCollectedData data)
        {
            if (_pickupMarkers.TryGetValue(data.PickupId, out var marker))
            {
                Destroy(marker.gameObject);
                _pickupMarkers.Remove(data.PickupId);
                _pickupTargets.Remove(data.PickupId);
            }
        }

        private void UpdateMarker(RectTransform marker, Vector3 worldPosition)
        {
            if (marker == null || mapRoot == null || worldCenter == null)
            {
                return;
            }

            // worldSize is full map width; normalize by half so edges map to +/-1.
            var halfWorld = Mathf.Max(0.01f, worldSize * 0.5f);
            var offset = worldPosition - worldCenter.position;
            var normalized = new Vector2(offset.x / halfWorld, offset.z / halfWorld);
            normalized.x = Mathf.Clamp(normalized.x, -1f, 1f);
            normalized.y = Mathf.Clamp(normalized.y, -1f, 1f);

            var half = mapRoot.rect.size * 0.42f;
            marker.anchoredPosition = new Vector2(normalized.x * half.x, normalized.y * half.y);
        }
    }
}

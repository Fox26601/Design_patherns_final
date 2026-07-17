using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Part3_EscapeRoom
{
    public class InventoryUI : MonoBehaviour
    {
        private const int SlotCount = 6;

        [SerializeField] private Transform slotRoot;
        [SerializeField] private TMP_Text selectedLabel;
        [SerializeField] private TMP_Text usedLabel;

        private EscapeRoomController _controller;
        private readonly List<Button> _slotButtons = new();
        private readonly List<TMP_Text> _slotLabels = new();
        private string _selectedItemId;

        public string SelectedItemId => _selectedItemId;
        public event Action<string> OnSelectionChanged;

        public void Bind(EscapeRoomController controller)
        {
            if (_controller != null)
            {
                _controller.Inventory.OnInventoryChanged -= Refresh;
            }

            _controller = controller;
            if (_controller != null)
            {
                _controller.Inventory.OnInventoryChanged += Refresh;
            }

            EnsureSlots();
            Refresh();
        }

        public void Configure(Transform slots, TMP_Text label, TMP_Text used = null)
        {
            slotRoot = slots;
            selectedLabel = label;
            usedLabel = used;
        }

        private void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.Inventory.OnInventoryChanged -= Refresh;
            }
        }

        public void ClearSelection()
        {
            _selectedItemId = null;
            UpdateSelectedLabel();
            OnSelectionChanged?.Invoke(null);
            HighlightSelection();
        }

        public void HandleHotkeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            for (var i = 0; i < SlotCount; i++)
            {
                if (WasSlotKeyPressed(keyboard, i))
                {
                    OnSlotClicked(i);
                    return;
                }
            }
        }

        private static bool WasSlotKeyPressed(Keyboard keyboard, int index)
        {
            return index switch
            {
                0 => keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame,
                1 => keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame,
                2 => keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame,
                3 => keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame,
                4 => keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame,
                5 => keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame,
                _ => false
            };
        }

        private void EnsureSlots()
        {
            if (slotRoot == null || _slotButtons.Count > 0)
            {
                return;
            }

            for (var i = 0; i < SlotCount; i++)
            {
                var buttonObject = new GameObject($"Slot_{i}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
                buttonObject.transform.SetParent(slotRoot, false);

                var layout = buttonObject.GetComponent<LayoutElement>();
                layout.minWidth = 88f;
                layout.preferredWidth = 88f;
                layout.minHeight = 56f;
                layout.preferredHeight = 56f;
                layout.flexibleWidth = 0f;
                layout.flexibleHeight = 0f;

                var image = buttonObject.GetComponent<Image>();
                image.color = new Color(0.16f, 0.18f, 0.24f, 0.98f);

                var button = buttonObject.GetComponent<Button>();
                var colors = button.colors;
                colors.highlightedColor = new Color(0.28f, 0.34f, 0.46f, 1f);
                colors.pressedColor = new Color(0.2f, 0.28f, 0.4f, 1f);
                colors.disabledColor = new Color(0.12f, 0.13f, 0.16f, 0.7f);
                button.colors = colors;

                var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(buttonObject.transform, false);
                var tmp = labelObject.GetComponent<TextMeshProUGUI>();
                tmp.fontSize = 12;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = new Color(0.94f, 0.96f, 1f, 1f);
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Truncate;
                tmp.raycastTarget = false;
                var labelRect = tmp.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(4f, 14f);
                labelRect.offsetMax = new Vector2(-4f, -2f);

                var hotkeyObject = new GameObject("Hotkey", typeof(RectTransform), typeof(TextMeshProUGUI));
                hotkeyObject.transform.SetParent(buttonObject.transform, false);
                var hotkey = hotkeyObject.GetComponent<TextMeshProUGUI>();
                hotkey.text = (i + 1).ToString();
                hotkey.fontSize = 11;
                hotkey.fontStyle = FontStyles.Bold;
                hotkey.alignment = TextAlignmentOptions.Bottom;
                hotkey.color = new Color(0.65f, 0.7f, 0.8f, 0.95f);
                hotkey.textWrappingMode = TextWrappingModes.NoWrap;
                hotkey.overflowMode = TextOverflowModes.Truncate;
                hotkey.raycastTarget = false;
                var hotkeyRect = hotkey.rectTransform;
                hotkeyRect.anchorMin = new Vector2(0f, 0f);
                hotkeyRect.anchorMax = new Vector2(1f, 0f);
                hotkeyRect.pivot = new Vector2(0.5f, 0f);
                hotkeyRect.anchoredPosition = Vector2.zero;
                hotkeyRect.sizeDelta = new Vector2(0f, 14f);
                hotkeyRect.offsetMin = new Vector2(2f, 1f);
                hotkeyRect.offsetMax = new Vector2(-2f, 15f);

                var index = i;
                button.onClick.AddListener(() => OnSlotClicked(index));
                _slotButtons.Add(button);
                _slotLabels.Add(tmp);
            }
        }

        private void OnSlotClicked(int index)
        {
            if (_controller == null)
            {
                return;
            }

            var items = _controller.Inventory.CollectedItems;
            if (index < 0 || index >= items.Count)
            {
                ClearSelection();
                return;
            }

            var itemId = items[index];
            if (_selectedItemId == itemId)
            {
                ClearSelection();
                return;
            }

            _selectedItemId = itemId;
            UpdateSelectedLabel();
            HighlightSelection();
            OnSelectionChanged?.Invoke(_selectedItemId);
        }

        public void Refresh()
        {
            if (_controller == null)
            {
                return;
            }

            EnsureSlots();
            var items = _controller.Inventory.CollectedItems;
            if (!string.IsNullOrEmpty(_selectedItemId) && !_controller.Inventory.HasItem(_selectedItemId))
            {
                _selectedItemId = null;
                OnSelectionChanged?.Invoke(null);
            }

            for (var i = 0; i < _slotButtons.Count; i++)
            {
                var button = _slotButtons[i];
                var label = i < _slotLabels.Count ? _slotLabels[i] : null;
                if (i < items.Count)
                {
                    button.interactable = true;
                    if (label != null)
                    {
                        label.text = _controller.GetDisplayName(items[i]);
                    }
                }
                else
                {
                    button.interactable = false;
                    if (label != null)
                    {
                        label.text = string.Empty;
                    }
                }
            }

            UpdateSelectedLabel();
            UpdateUsedLabel();
            HighlightSelection();
        }

        private void UpdateSelectedLabel()
        {
            if (selectedLabel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_selectedItemId))
            {
                selectedLabel.text = "Inventory — 1–6 or click, then click a target";
            }
            else
            {
                selectedLabel.text = $"Selected: {_controller.GetDisplayName(_selectedItemId)} — click a target";
            }
        }

        private void UpdateUsedLabel()
        {
            if (usedLabel == null || _controller == null)
            {
                return;
            }

            var used = _controller.Inventory.ConsumedItems;
            if (used.Count == 0)
            {
                usedLabel.text = "Used: —";
                return;
            }

            var builder = new StringBuilder("Used: ");
            for (var i = 0; i < used.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(_controller.GetDisplayName(used[i]));
            }

            usedLabel.text = builder.ToString();
        }

        private void HighlightSelection()
        {
            if (_controller == null)
            {
                return;
            }

            var items = _controller.Inventory.CollectedItems;
            for (var i = 0; i < _slotButtons.Count; i++)
            {
                var image = _slotButtons[i].GetComponent<Image>();
                if (image == null)
                {
                    continue;
                }

                var selected = i < items.Count && items[i] == _selectedItemId;
                image.color = selected
                    ? new Color(0.28f, 0.48f, 0.82f, 0.98f)
                    : new Color(0.16f, 0.18f, 0.24f, 0.98f);
            }
        }
    }
}

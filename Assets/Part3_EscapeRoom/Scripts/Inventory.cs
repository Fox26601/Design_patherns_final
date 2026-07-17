using System;
using System.Collections.Generic;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Tracks collected items separately from consumed items.
    /// </summary>
    public class Inventory
    {
        private readonly List<string> _collectedItems = new();
        private readonly List<string> _consumedItems = new();
        private readonly HashSet<string> _consumedSet = new();

        public event Action OnInventoryChanged;

        public IReadOnlyList<string> CollectedItems => _collectedItems;
        public IReadOnlyList<string> ConsumedItems => _consumedItems;

        public void AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || _consumedSet.Contains(itemId))
            {
                return;
            }

            if (!_collectedItems.Contains(itemId))
            {
                _collectedItems.Add(itemId);
                OnInventoryChanged?.Invoke();
            }
        }

        public void ConsumeItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            var removed = _collectedItems.Remove(itemId);
            if (_consumedSet.Add(itemId))
            {
                _consumedItems.Add(itemId);
            }

            if (removed)
            {
                OnInventoryChanged?.Invoke();
            }
        }

        public bool HasItem(string itemId) =>
            !string.IsNullOrEmpty(itemId) &&
            _collectedItems.Contains(itemId) &&
            !_consumedSet.Contains(itemId);

        public bool WasConsumed(string itemId) =>
            !string.IsNullOrEmpty(itemId) && _consumedSet.Contains(itemId);
    }
}

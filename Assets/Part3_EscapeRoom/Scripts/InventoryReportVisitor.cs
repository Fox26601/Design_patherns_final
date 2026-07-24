using System.Text;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Summarizes collectibles vs interactable lock state across the room.
    /// </summary>
    public sealed class InventoryReportVisitor : IRoomItemVisitor
    {
        private readonly EscapeRoomController _controller;
        private readonly StringBuilder _collectibles = new();
        private readonly StringBuilder _interactables = new();
        private int _pickupCount;
        private int _openCount;
        private int _lockedCount;

        public InventoryReportVisitor(EscapeRoomController controller)
        {
            _controller = controller;
        }

        public string Result
        {
            get
            {
                var report = new StringBuilder();
                report.AppendLine("=== Room Report ===");
                if (_controller != null)
                {
                    report.AppendLine(
                        $"Inventory slots: {_controller.Inventory.CollectedItems.Count} | Used: {_controller.Inventory.ConsumedItems.Count}");
                }

                report.AppendLine($"Pickups seen: {_pickupCount} | Open: {_openCount} | Locked/closed: {_lockedCount}");
                report.AppendLine("-- Collectibles --");
                report.Append(_collectibles.Length > 0 ? _collectibles : "• (none)\n");
                report.AppendLine("-- Interactables --");
                report.Append(_interactables.Length > 0 ? _interactables : "• (none)\n");
                return report.ToString().TrimEnd();
            }
        }

        public void Clear()
        {
            _collectibles.Clear();
            _interactables.Clear();
            _pickupCount = 0;
            _openCount = 0;
            _lockedCount = 0;
        }

        public void VisitPickup(PickupInteractable pickup)
        {
            _pickupCount++;
            var status = "in world";
            if (_controller != null)
            {
                if (_controller.Inventory.WasConsumed(pickup.InteractableId))
                {
                    status = "used";
                }
                else if (_controller.Inventory.HasItem(pickup.InteractableId))
                {
                    status = "in inventory";
                }
                else if (!pickup.gameObject.activeInHierarchy)
                {
                    status = "collected";
                }
            }

            _collectibles.AppendLine($"• {pickup.DisplayLabel} ({status})");
        }

        public void VisitNote(NoteInteractable note)
        {
            var read = _controller != null && _controller.HasReadNote;
            _collectibles.AppendLine($"• Note {note.DisplayLabel} ({(read ? "read" : "unread")})");
        }

        public void VisitDrawer(DrawerInteractable drawer) => TrackInteractable("Drawer", drawer.InteractableId, drawer.DisplayLabel);

        public void VisitDoor(DoorInteractable door) => TrackInteractable("Door", door.InteractableId, door.DisplayLabel);

        public void VisitSafe(SafeInteractable safe) => TrackInteractable("Safe", safe.InteractableId, safe.DisplayLabel);

        public void VisitDecoy(DecoyInteractable decoy)
        {
            _interactables.AppendLine($"• Decoy {decoy.DisplayLabel}");
        }

        private void TrackInteractable(string kind, string id, string label)
        {
            var state = _controller != null ? _controller.RoomState.GetState(id) : RoomObjectState.Locked;
            var open = state is RoomObjectState.Open or RoomObjectState.Unlocked or RoomObjectState.Solved or RoomObjectState.Revealed;
            if (open)
            {
                _openCount++;
            }
            else
            {
                _lockedCount++;
            }

            _interactables.AppendLine($"• {kind} {label}: {state}");
        }
    }
}

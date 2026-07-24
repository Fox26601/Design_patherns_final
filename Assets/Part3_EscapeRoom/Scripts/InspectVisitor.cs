using System.Text;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Builds examine-style descriptions for each room item type.
    /// </summary>
    public sealed class InspectVisitor : IRoomItemVisitor
    {
        private readonly EscapeRoomController _controller;
        private readonly StringBuilder _builder = new();

        public InspectVisitor(EscapeRoomController controller)
        {
            _controller = controller;
        }

        public string Result => _builder.ToString().TrimEnd();

        public void Clear() => _builder.Clear();

        public void VisitPickup(PickupInteractable pickup)
        {
            var inInventory = _controller != null &&
                              (_controller.Inventory.HasItem(pickup.InteractableId) ||
                               _controller.Inventory.WasConsumed(pickup.InteractableId));
            _builder.AppendLine(inInventory
                ? $"[Key/Pickup] {pickup.DisplayLabel} — already collected"
                : $"[Key/Pickup] {pickup.DisplayLabel} — {pickup.ExamineText}");
        }

        public void VisitNote(NoteInteractable note)
        {
            var drawerOpen = _controller != null &&
                             _controller.RoomState.GetState("drawer") == RoomObjectState.Open;
            _builder.AppendLine(drawerOpen
                ? $"[Note] {note.DisplayLabel} — {note.ExamineText}"
                : $"[Note] {note.DisplayLabel} — hidden until the drawer opens");
        }

        public void VisitDrawer(DrawerInteractable drawer)
        {
            var state = StateOf(drawer.InteractableId);
            _builder.AppendLine($"[Drawer] {drawer.DisplayLabel} — {state}. {drawer.ExamineText}");
        }

        public void VisitDoor(DoorInteractable door)
        {
            var state = StateOf(door.InteractableId);
            _builder.AppendLine($"[Door] {door.DisplayLabel} — {state}. {door.ExamineText}");
        }

        public void VisitSafe(SafeInteractable safe)
        {
            var state = StateOf(safe.InteractableId);
            _builder.AppendLine($"[Safe] {safe.DisplayLabel} — {state}. {safe.ExamineText}");
        }

        public void VisitDecoy(DecoyInteractable decoy)
        {
            _builder.AppendLine($"[Decoy] {decoy.DisplayLabel} — {decoy.ExamineText}");
        }

        private string StateOf(string id)
        {
            if (_controller == null)
            {
                return "unknown";
            }

            return _controller.RoomState.GetState(id).ToString().ToLowerInvariant();
        }
    }
}

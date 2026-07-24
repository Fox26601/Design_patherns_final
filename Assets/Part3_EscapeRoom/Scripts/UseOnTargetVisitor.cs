namespace Part3_EscapeRoom
{
    /// <summary>
    /// Applies a selected inventory item to a typed room target.
    /// Puzzle rules still resolve through InteractionResolver.
    /// </summary>
    public sealed class UseOnTargetVisitor : IRoomItemVisitor
    {
        private readonly EscapeRoomController _controller;
        private readonly string _selectedItemId;

        public UseOnTargetVisitor(EscapeRoomController controller, string selectedItemId)
        {
            _controller = controller;
            _selectedItemId = selectedItemId;
        }

        public UsageResult Result { get; private set; } = new(false, "Nothing happens.");

        public void VisitPickup(PickupInteractable pickup)
        {
            Result = new UsageResult(false, "You cannot use an item on a pickup.");
        }

        public void VisitNote(NoteInteractable note)
        {
            Result = new UsageResult(false, "The note does not accept items.");
        }

        public void VisitDrawer(DrawerInteractable drawer) => Apply(drawer);

        public void VisitDoor(DoorInteractable door) => Apply(door);

        public void VisitSafe(SafeInteractable safe)
        {
            Result = new UsageResult(false, "The safe needs a code, not an inventory item.");
        }

        public void VisitDecoy(DecoyInteractable decoy)
        {
            Result = new UsageResult(false, "Nothing happens.");
        }

        private void Apply(IInteractable target)
        {
            if (_controller == null || string.IsNullOrEmpty(_selectedItemId))
            {
                Result = new UsageResult(false, "No item selected.");
                return;
            }

            Result = _controller.TryUseItemOn(_selectedItemId, target);
        }
    }
}

namespace Part3_EscapeRoom
{
    public class DrawerInteractable : InteractableView
    {
        public override InteractionResult Interact(InteractionContext context)
        {
            var state = context.RoomState.GetState(InteractableId);
            if (state == RoomObjectState.Open)
            {
                return new InteractionResult(true, "The drawer is open. Click the Note.");
            }

            return new InteractionResult(true, "The drawer is locked. It needs a key.");
        }

        public override void Accept(IRoomItemVisitor visitor) => visitor.VisitDrawer(this);
    }
}

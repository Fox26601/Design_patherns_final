namespace Part3_EscapeRoom
{
    public class DoorInteractable : InteractableView
    {
        public override InteractionResult Interact(InteractionContext context)
        {
            var state = context.RoomState.GetState(InteractableId);
            if (state == RoomObjectState.Unlocked)
            {
                context.Controller?.RequestWin();
                return new InteractionResult(true, "You escaped the room!");
            }

            return new InteractionResult(true, "The door is locked. It needs a special key.");
        }

        public override void Accept(IRoomItemVisitor visitor) => visitor.VisitDoor(this);
    }
}

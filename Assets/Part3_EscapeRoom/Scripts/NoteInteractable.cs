namespace Part3_EscapeRoom
{
    public class NoteInteractable : InteractableView
    {
        public override bool CanInteract(InteractionContext context)
        {
            return context.RoomState.GetState("drawer") == RoomObjectState.Open ||
                   context.RoomState.GetState(InteractableId) == RoomObjectState.Revealed;
        }

        public override InteractionResult Interact(InteractionContext context)
        {
            if (!CanInteract(context))
            {
                return new InteractionResult(false, "There is nothing here.");
            }

            return context.Controller != null
                ? context.Controller.ReadNote()
                : new InteractionResult(true, "The note is blank.");
        }

        public override void Accept(IRoomItemVisitor visitor) => visitor.VisitNote(this);
    }
}

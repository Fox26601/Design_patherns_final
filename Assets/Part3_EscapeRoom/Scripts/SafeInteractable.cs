using System;

namespace Part3_EscapeRoom
{
    public class SafeInteractable : InteractableView
    {
        public event Action OnCodeInputRequested;

        public override InteractionResult Interact(InteractionContext context)
        {
            var state = context.RoomState.GetState(InteractableId);
            if (state == RoomObjectState.Open)
            {
                return new InteractionResult(true, "The safe is open.");
            }

            if (context.RoomState.GetState("drawer") != RoomObjectState.Open)
            {
                return new InteractionResult(true, "A locked safe. You need more clues.");
            }

            if (context.Controller != null && !context.Controller.HasReadNote)
            {
                return new InteractionResult(true, "A locked safe. Maybe the note has the code.");
            }

            OnCodeInputRequested?.Invoke();
            return new InteractionResult(true, "Enter the code.");
        }
    }
}

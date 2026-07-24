namespace Part3_EscapeRoom
{
    /// <summary>
    /// Decoy object that gives a clear dead-end message.
    /// </summary>
    public class DecoyInteractable : InteractableView
    {
        private const string DefaultMessage = "This red box does nothing. Keep searching.";

        public override InteractionResult Interact(InteractionContext context)
        {
            var message = string.IsNullOrWhiteSpace(ExamineText) ? DefaultMessage : ExamineText;
            return new InteractionResult(true, message);
        }

        public override void Accept(IRoomItemVisitor visitor) => visitor.VisitDecoy(this);
    }
}

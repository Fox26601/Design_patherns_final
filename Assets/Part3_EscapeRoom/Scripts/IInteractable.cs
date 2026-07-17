namespace Part3_EscapeRoom
{
    public interface IInteractable
    {
        string InteractableId { get; }
        bool CanInteract(InteractionContext context);
        InteractionResult Interact(InteractionContext context);
    }
}

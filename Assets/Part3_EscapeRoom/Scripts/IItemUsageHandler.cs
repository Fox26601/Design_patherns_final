namespace Part3_EscapeRoom
{
    public interface IItemUsageHandler
    {
        bool CanHandle(string sourceItemId, string targetInteractableId);
        UsageResult Apply(string sourceItemId, IInteractable target, InteractionContext context);
    }
}

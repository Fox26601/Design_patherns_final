namespace Part3_EscapeRoom
{
    /// <summary>
    /// Example handler: rusty key opens drawer, key is consumed, note becomes visible.
    /// </summary>
    public class KeyOnDrawerHandler : IItemUsageHandler
    {
        public bool CanHandle(string sourceItemId, string targetInteractableId) =>
            sourceItemId == "rustyKey" && targetInteractableId == "drawer";

        public UsageResult Apply(string sourceItemId, IInteractable target, InteractionContext context)
        {
            if (context.RoomState.GetState("drawer") == RoomObjectState.Open)
            {
                return new UsageResult(false, "The drawer is already open.");
            }

            context.Inventory.ConsumeItem(sourceItemId);
            context.RoomState.SetState("drawer", RoomObjectState.Open);
            context.RoomState.SetState("note", RoomObjectState.Revealed);
            return new UsageResult(true, "Drawer opened. A note appeared.");
        }
    }
}

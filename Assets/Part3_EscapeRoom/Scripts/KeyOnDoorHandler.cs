namespace Part3_EscapeRoom
{
    /// <summary>
    /// Gold key unlocks the door and triggers escape win.
    /// </summary>
    public class KeyOnDoorHandler : IItemUsageHandler
    {
        public bool CanHandle(string sourceItemId, string targetInteractableId) =>
            sourceItemId == "goldKey" && targetInteractableId == "door";

        public UsageResult Apply(string sourceItemId, IInteractable target, InteractionContext context)
        {
            if (context.RoomState.GetState("door") == RoomObjectState.Unlocked)
            {
                return new UsageResult(false, "The door is already unlocked.");
            }

            context.Inventory.ConsumeItem(sourceItemId);
            context.RoomState.SetState("door", RoomObjectState.Unlocked);
            context.Controller?.RequestWin();
            return new UsageResult(true, "The door unlocks. You escaped!");
        }
    }
}

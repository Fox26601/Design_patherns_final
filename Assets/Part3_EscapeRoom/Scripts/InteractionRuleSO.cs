using UnityEngine;

namespace Part3_EscapeRoom
{
    public enum InteractionActionType
    {
        ConsumeItem,
        SetState,
        AddItem,
        SetFlag,
        Win
    }

    [System.Serializable]
    public class InteractionAction
    {
        public InteractionActionType ActionType;
        public string TargetId;
        public RoomObjectState State;
        public string Message;
    }

    /// <summary>
    /// Data-driven item-on-target rule (Strategy content without new C# handlers).
    /// </summary>
    [CreateAssetMenu(fileName = "InteractionRule", menuName = "DesignPatterns/EscapeRoom/Interaction Rule")]
    public class InteractionRuleSO : ScriptableObject, IItemUsageHandler
    {
        public string SourceItemId;
        public string TargetInteractableId;
        public InteractionAction[] Actions;
        public string SuccessMessage = "Done.";

        public bool CanHandle(string sourceItemId, string targetInteractableId) =>
            sourceItemId == SourceItemId && targetInteractableId == TargetInteractableId;

        public UsageResult Apply(string sourceItemId, IInteractable target, InteractionContext context)
        {
            if (TargetInteractableId == "drawer" &&
                context.RoomState.GetState("drawer") == RoomObjectState.Open)
            {
                return new UsageResult(false, "The drawer is already open.");
            }

            if (TargetInteractableId == "door" &&
                context.RoomState.GetState("door") == RoomObjectState.Unlocked)
            {
                return new UsageResult(false, "The door is already unlocked.");
            }

            if (Actions == null || Actions.Length == 0)
            {
                return new UsageResult(true, SuccessMessage);
            }

            foreach (var action in Actions)
            {
                ApplyAction(action, sourceItemId, context);
            }

            var message = string.IsNullOrEmpty(SuccessMessage) ? "Done." : SuccessMessage;
            if (TargetInteractableId == "drawer" &&
                message.IndexOf("note", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                message = "Drawer opened. A note appeared.";
            }

            return new UsageResult(true, message);
        }

        private static void ApplyAction(InteractionAction action, string sourceItemId, InteractionContext context)
        {
            if (action == null)
            {
                return;
            }

            switch (action.ActionType)
            {
                case InteractionActionType.ConsumeItem:
                    context.Inventory.ConsumeItem(string.IsNullOrEmpty(action.TargetId) ? sourceItemId : action.TargetId);
                    break;
                case InteractionActionType.SetState:
                    context.RoomState.SetState(action.TargetId, action.State);
                    break;
                case InteractionActionType.AddItem:
                    context.Inventory.AddItem(action.TargetId);
                    break;
                case InteractionActionType.SetFlag:
                    context.Controller?.SetFlag(action.TargetId, true);
                    break;
                case InteractionActionType.Win:
                    context.Controller?.RequestWin();
                    break;
            }
        }
    }
}

using UnityEngine;

namespace Part3_EscapeRoom
{
    public class PickupInteractable : InteractableView
    {
        [SerializeField] private string itemId;
        [SerializeField] private string pickupMessage;

        public void ConfigurePickup(string id, string displayName, EscapeRoomController controller)
        {
            itemId = id;
            pickupMessage = $"You picked up {displayName}.";
            Configure(id, displayName, controller);
        }

        public override bool CanInteract(InteractionContext context)
        {
            return !context.Inventory.HasItem(itemId) && !context.Inventory.WasConsumed(itemId);
        }

        public override InteractionResult Interact(InteractionContext context)
        {
            if (!CanInteract(context))
            {
                return new InteractionResult(false, "You already have this.");
            }

            var display = context.Controller?.GetDisplayName(itemId) ?? itemId;
            context.Inventory.AddItem(itemId);
            gameObject.SetActive(false);
            return new InteractionResult(true, string.IsNullOrEmpty(pickupMessage)
                ? $"You picked up {display}."
                : pickupMessage);
        }

        public override void Accept(IRoomItemVisitor visitor) => visitor.VisitPickup(this);
    }
}

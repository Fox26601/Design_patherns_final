using System.Collections.Generic;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Picks the matching IItemUsageHandler for item-on-object use.
    /// </summary>
    public class InteractionResolver
    {
        private readonly List<IItemUsageHandler> _handlers = new();

        public void RegisterHandler(IItemUsageHandler handler)
        {
            if (handler != null && !_handlers.Contains(handler))
            {
                _handlers.Add(handler);
            }
        }

        public UsageResult TryUseItemOnTarget(
            string sourceItemId,
            IInteractable target,
            InteractionContext context)
        {
            if (target == null)
            {
                return new UsageResult(false, "No target selected.");
            }

            foreach (var handler in _handlers)
            {
                if (!handler.CanHandle(sourceItemId, target.InteractableId))
                {
                    continue;
                }

                return handler.Apply(sourceItemId, target, context);
            }

            return new UsageResult(false, "Nothing happens.");
        }
    }
}

using System.Collections.Generic;

namespace Part3_EscapeRoom
{
    /// <summary>
    /// Composite pattern: all child conditions must pass.
    /// </summary>
    public class CompositeCondition : ICondition
    {
        private readonly List<ICondition> _children = new();

        public void Add(ICondition condition)
        {
            if (condition != null)
            {
                _children.Add(condition);
            }
        }

        public bool Evaluate(InteractionContext context)
        {
            if (_children.Count == 0)
            {
                return true;
            }

            foreach (var child in _children)
            {
                if (!child.Evaluate(context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

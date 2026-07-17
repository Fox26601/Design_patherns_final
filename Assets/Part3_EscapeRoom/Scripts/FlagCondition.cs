namespace Part3_EscapeRoom
{
    /// <summary>
    /// Runtime condition that checks a named boolean flag on the escape room session.
    /// </summary>
    public class FlagCondition : ICondition
    {
        private readonly string _flagName;
        private readonly EscapeRoomController _controller;

        public FlagCondition(string flagName, EscapeRoomController controller)
        {
            _flagName = flagName;
            _controller = controller;
        }

        public bool Evaluate(InteractionContext context)
        {
            return _controller != null && _controller.GetFlag(_flagName);
        }
    }
}

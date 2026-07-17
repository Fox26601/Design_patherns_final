namespace Part1_TicTacToe
{
    /// <summary>
    /// Command contract for undo/redo support.
    /// Pattern: Command (https://www.unitydesignpatterns.com/patterns/command)
    /// </summary>
    public interface IGameCommand
    {
        void Execute();
        void Undo();
    }
}

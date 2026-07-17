using System.Collections.Generic;

namespace Part1_TicTacToe
{
    /// <summary>
    /// Maintains undo and redo stacks for discrete commands.
    /// </summary>
    public class CommandHistory
    {
        private readonly Stack<IGameCommand> _undoStack = new();
        private readonly Stack<IGameCommand> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Execute(IGameCommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }

        public void Redo()
        {
            if (!CanRedo)
            {
                return;
            }

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}

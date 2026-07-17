namespace Part1_TicTacToe
{
    /// <summary>
    /// Encapsulates a single board move for undo/redo.
    /// </summary>
    public class PlaceMarkCommand : IGameCommand
    {
        private readonly BoardModel _board;
        private readonly int _index;
        private readonly PlayerMark _mark;

        public PlaceMarkCommand(BoardModel board, int index, PlayerMark mark)
        {
            _board = board;
            _index = index;
            _mark = mark;
        }

        public void Execute()
        {
            _board.TryPlaceMark(_index, _mark);
        }

        public void Undo()
        {
            _board.ClearCell(_index);
        }
    }
}

namespace Part1_TicTacToe
{
    /// <summary>
    /// Pure game board logic without Unity dependencies.
    /// Pattern: MVP Model (https://www.unitydesignpatterns.com/patterns/mvp)
    /// </summary>
    public class BoardModel
    {
        public const int Size = 3;
        private readonly PlayerMark[] _cells = new PlayerMark[Size * Size];

        public PlayerMark GetCell(int index)
        {
            return _cells[index];
        }

        public bool TryPlaceMark(int index, PlayerMark mark)
        {
            if (index < 0 || index >= _cells.Length || mark == PlayerMark.None || _cells[index] != PlayerMark.None)
            {
                return false;
            }

            _cells[index] = mark;
            return true;
        }

        public void ClearCell(int index)
        {
            if (index >= 0 && index < _cells.Length)
            {
                _cells[index] = PlayerMark.None;
            }
        }

        public void Reset()
        {
            for (var i = 0; i < _cells.Length; i++)
            {
                _cells[i] = PlayerMark.None;
            }
        }

        public GamePhase EvaluatePhase(PlayerMark currentTurn)
        {
            if (HasWinner(PlayerMark.X))
            {
                return GamePhase.XWon;
            }

            if (HasWinner(PlayerMark.O))
            {
                return GamePhase.OWon;
            }

            if (IsBoardFull())
            {
                return GamePhase.Draw;
            }

            return currentTurn == PlayerMark.X ? GamePhase.XTurn : GamePhase.OTurn;
        }

        private bool HasWinner(PlayerMark mark)
        {
            for (var row = 0; row < Size; row++)
            {
                if (_cells[row * Size] == mark && _cells[row * Size + 1] == mark && _cells[row * Size + 2] == mark)
                {
                    return true;
                }
            }

            for (var col = 0; col < Size; col++)
            {
                if (_cells[col] == mark && _cells[Size + col] == mark && _cells[2 * Size + col] == mark)
                {
                    return true;
                }
            }

            return (_cells[0] == mark && _cells[4] == mark && _cells[8] == mark)
                || (_cells[2] == mark && _cells[4] == mark && _cells[6] == mark);
        }

        private bool IsBoardFull()
        {
            foreach (var cell in _cells)
            {
                if (cell == PlayerMark.None)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

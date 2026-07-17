namespace Part1_TicTacToe
{
    /// <summary>
    /// Coordinates model, view, commands, and game manager.
    /// Pattern: MVP Presenter (https://www.unitydesignpatterns.com/patterns/mvp)
    /// </summary>
    public class GamePresenter
    {
        private readonly BoardModel _board = new();
        private readonly BoardView _view;
        private readonly CommandHistory _history = new();

        public GamePresenter(BoardView view)
        {
            _view = view;
            _view.OnCellClicked += HandleCellClicked;
            _view.OnUndoClicked += HandleUndo;
            _view.OnRedoClicked += HandleRedo;
            _view.OnRestartClicked += HandleRestart;
        }

        public void Initialize()
        {
            TicTacToeGameManager.Instance.ResetRound();
            _board.Reset();
            _history.Clear();
            RefreshView();
        }

        private void HandleCellClicked(int index)
        {
            var manager = TicTacToeGameManager.Instance;
            if (manager.IsRoundOver())
            {
                return;
            }

            var command = new PlaceMarkCommand(_board, index, manager.CurrentTurn);
            if (_board.GetCell(index) != PlayerMark.None)
            {
                return;
            }

            _history.Execute(command);
            manager.SwitchTurn();
            manager.RegisterResult(_board.EvaluatePhase(manager.CurrentTurn));
            RefreshView();
        }

        private void HandleUndo()
        {
            var manager = TicTacToeGameManager.Instance;
            if (!_history.CanUndo || manager.IsRoundOver())
            {
                return;
            }

            _history.Undo();
            manager.SwitchTurn();
            manager.SetPhase(_board.EvaluatePhase(manager.CurrentTurn));
            RefreshView();
        }

        private void HandleRedo()
        {
            var manager = TicTacToeGameManager.Instance;
            if (!_history.CanRedo || manager.IsRoundOver())
            {
                return;
            }

            _history.Redo();
            manager.SwitchTurn();
            manager.RegisterResult(_board.EvaluatePhase(manager.CurrentTurn));
            RefreshView();
        }

        private void HandleRestart()
        {
            Initialize();
        }

        private void RefreshView()
        {
            var manager = TicTacToeGameManager.Instance;

            for (var i = 0; i < BoardModel.Size * BoardModel.Size; i++)
            {
                _view.RenderCell(i, _board.GetCell(i));
            }

            _view.SetScore(manager.XScore, manager.OScore);
            _view.SetUndoAvailable(_history.CanUndo && !manager.IsRoundOver());
            _view.SetRedoAvailable(_history.CanRedo && !manager.IsRoundOver());
            _view.SetStatus(BuildStatusMessage(manager.Phase, manager.CurrentTurn));
        }

        private static string BuildStatusMessage(GamePhase phase, PlayerMark currentTurn)
        {
            return phase switch
            {
                GamePhase.XWon => "Player X wins!",
                GamePhase.OWon => "Player O wins!",
                GamePhase.Draw => "Draw!",
                _ => $"Turn: {currentTurn}"
            };
        }
    }
}

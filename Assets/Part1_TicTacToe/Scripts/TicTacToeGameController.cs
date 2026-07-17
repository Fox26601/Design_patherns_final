using Core;
using UnityEngine;

namespace Part1_TicTacToe
{
    /// <summary>
    /// Scene entry point for tic-tac-toe mode.
    /// </summary>
    public class TicTacToeGameController : MonoBehaviour
    {
        [SerializeField] private BoardView boardView;

        private GamePresenter _presenter;

        private void Start()
        {
            if (FindFirstObjectByType<GamePauseHandler>() == null)
            {
                var pauseObject = new GameObject("GamePauseHandler");
                pauseObject.AddComponent<GamePauseHandler>();
            }

            _presenter = new GamePresenter(boardView);
            _presenter.Initialize();
        }
    }
}

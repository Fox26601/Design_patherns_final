using Shared;

namespace Part1_TicTacToe
{
    /// <summary>
    /// Tracks match score and current turn across rounds.
    /// Pattern: Singleton (https://www.unitydesignpatterns.com/patterns/singleton)
    /// </summary>
    public class TicTacToeGameManager : Singleton<TicTacToeGameManager>
    {
        public int XScore { get; private set; }
        public int OScore { get; private set; }
        public PlayerMark CurrentTurn { get; private set; } = PlayerMark.X;
        public GamePhase Phase { get; private set; } = GamePhase.XTurn;

        public void ResetRound()
        {
            CurrentTurn = PlayerMark.X;
            Phase = GamePhase.XTurn;
        }

        public void RegisterResult(GamePhase phase)
        {
            Phase = phase;

            if (phase == GamePhase.XWon)
            {
                XScore++;
            }
            else if (phase == GamePhase.OWon)
            {
                OScore++;
            }
        }

        public void SetPhase(GamePhase phase)
        {
            Phase = phase;
        }

        public void SwitchTurn()
        {
            CurrentTurn = CurrentTurn == PlayerMark.X ? PlayerMark.O : PlayerMark.X;
        }

        public bool IsRoundOver()
        {
            return Phase is GamePhase.XWon or GamePhase.OWon or GamePhase.Draw;
        }
    }
}

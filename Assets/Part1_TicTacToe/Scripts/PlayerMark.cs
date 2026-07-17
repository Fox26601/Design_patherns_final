namespace Part1_TicTacToe
{
    public enum PlayerMark
    {
        None = 0,
        X = 1,
        O = 2
    }

    public enum GamePhase
    {
        XTurn,
        OTurn,
        XWon,
        OWon,
        Draw
    }
}

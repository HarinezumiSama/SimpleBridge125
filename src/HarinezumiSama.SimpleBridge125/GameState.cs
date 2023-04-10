namespace HarinezumiSama.SimpleBridge125;

public enum GameState
{
    /// <summary>
    ///     Indicates that the dealer player is to make their first move with the last dealt card.
    /// </summary>
    DealerFirstMove = 1,

    /// <summary>
    ///     The current player is to make their first move after the previous player completed their turn.
    /// </summary>
    PlayerTurnStarted = 2,

    PlayerTurnContinued = 3,

    /// <summary>
    ///     The round ended and score is to be counted.
    /// </summary>
    RoundEnded = 4
}
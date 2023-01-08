using System.Collections.Generic;

namespace HarinezumiSama.SimpleBridge125;

public static class PlayingCardRankExtensions
{
    private static readonly Dictionary<PlayingCardRank, string> Strings =
        new()
        {
            { PlayingCardRank.Six, "6" },
            { PlayingCardRank.Seven, "7" },
            { PlayingCardRank.Eight, "8" },
            { PlayingCardRank.Nine, "9" },
            { PlayingCardRank.Ten, "10" },
            { PlayingCardRank.Jack, "J" },
            { PlayingCardRank.Queen, "Q" },
            { PlayingCardRank.King, "K" },
            { PlayingCardRank.Ace, "A" }
        };

    public static string AsString(this PlayingCardRank value) => Strings[value];
}
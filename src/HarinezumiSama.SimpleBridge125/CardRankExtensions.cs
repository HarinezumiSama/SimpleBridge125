using System.Collections.Generic;

namespace HarinezumiSama.SimpleBridge125;

public static class CardRankExtensions
{
    private static readonly Dictionary<CardRank, string> Strings =
        new()
        {
            { CardRank.Six, "6" },
            { CardRank.Seven, "7" },
            { CardRank.Eight, "8" },
            { CardRank.Nine, "9" },
            { CardRank.Ten, "10" },
            { CardRank.Jack, "J" },
            { CardRank.Queen, "Q" },
            { CardRank.King, "K" },
            { CardRank.Ace, "A" }
        };

    public static string AsString(this CardRank value) => Strings[value];
}
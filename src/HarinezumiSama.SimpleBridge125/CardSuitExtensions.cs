using System.Collections.Generic;

namespace HarinezumiSama.SimpleBridge125;

public static class CardSuitExtensions
{
    private static readonly Dictionary<CardSuit, string> Strings =
        new()
        {
            { CardSuit.Spades, "♠" },
            { CardSuit.Clubs, "♣" },
            { CardSuit.Diamonds, "♦" },
            { CardSuit.Hearts, "♥" }
        };

    public static string AsString(this CardSuit value) => Strings[value];
}
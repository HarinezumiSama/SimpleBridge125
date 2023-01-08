using System.Collections.Generic;

namespace HarinezumiSama.SimpleBridge125
{
    public static class PlayingCardSuitExtensions
    {
        private static readonly Dictionary<PlayingCardSuit, string> Strings =
            new()
            {
                { PlayingCardSuit.Spades, "♠" },
                { PlayingCardSuit.Clubs, "♣" },
                { PlayingCardSuit.Diamonds, "♦" },
                { PlayingCardSuit.Hearts, "♥" }
            };

        public static string AsString(this PlayingCardSuit value) => Strings[value];
    }
}
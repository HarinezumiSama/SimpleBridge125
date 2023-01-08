using System;
using System.Diagnostics;

namespace HarinezumiSama.SimpleBridge125
{
    [DebuggerDisplay(@"{ToDebuggerString(),nq}")]
    public readonly record struct PlayingCard(PlayingCardRank Rank, PlayingCardSuit Suit)
    {
        public override string ToString() => $@"{Rank.AsString()}{Suit.AsString()}";

        private string ToDebuggerString() => $@"{GetType().GetQualifiedName()}: {ToString()}";
    }
}
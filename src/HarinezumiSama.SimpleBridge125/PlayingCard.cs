using System;
using System.Diagnostics;

namespace HarinezumiSama.SimpleBridge125
{
    //// ReSharper disable once UseNameofExpression :: False positive
    [DebuggerDisplay(@"{ToDebugString(),nq}")]
    public struct PlayingCard : IEquatable<PlayingCard>
    {
        public PlayingCard(PlayingCardRank rank, PlayingCardSuit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public PlayingCardRank Rank
        {
            get;
        }

        public PlayingCardSuit Suit
        {
            get;
        }

        public override string ToString() => $@"{Rank.AsString()}{Suit.AsString()}";

        public override bool Equals(object obj) => obj is PlayingCard other && Equals(other);

        public override int GetHashCode() => Rank.CombineHashCodes(Suit);

        public bool Equals(PlayingCard other) => Equals(this, other);

        public static bool Equals(PlayingCard left, PlayingCard right)
            => left.Rank == right.Rank && left.Suit == right.Suit;

        public static bool operator ==(PlayingCard left, PlayingCard right) => Equals(left, right);

        public static bool operator !=(PlayingCard left, PlayingCard right) => !Equals(left, right);

        private string ToDebugString() => $@"{GetType().GetQualifiedName()}: {ToString()}";
    }
}
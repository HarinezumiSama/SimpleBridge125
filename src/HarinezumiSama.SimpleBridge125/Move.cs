using System;
using System.Diagnostics;

namespace HarinezumiSama.SimpleBridge125
{
    //// ReSharper disable once UseNameofExpression :: False positive
    [DebuggerDisplay(@"{ToDebugString(),nq}")]
    public struct Move
    {
        //// TODO [HarinezumiSama] Implement Move: One move should allow a few cards of the same rank

        public Move(PlayingCard card, bool isBridgeDeclared, PlayingCardSuit? requestedSuit)
        {
            if (card.Rank == PlayingCardRank.Jack)
            {
                if (!requestedSuit.HasValue)
                {
                    throw new ArgumentNullException(
                        nameof(requestedSuit),
                        $@"The requested suit must be specified when the card is {PlayingCardRank.Jack}.");
                }
            }
            else
            {
                if (requestedSuit.HasValue)
                {
                    throw new ArgumentException(
                        $@"The requested suit cannot be specified when the card is {card}.",
                        nameof(requestedSuit));
                }
            }

            Card = card;
            IsBridgeDeclared = isBridgeDeclared;
            RequestedSuit = requestedSuit;
        }

        public PlayingCard Card
        {
            get;
        }

        public bool IsBridgeDeclared
        {
            get;
        }

        public PlayingCardSuit? RequestedSuit
        {
            get;
        }

        public override string ToString()
            => $@"{nameof(Card)} = {Card}, {nameof(IsBridgeDeclared)} = {IsBridgeDeclared}, {nameof(
                RequestedSuit)} = {RequestedSuit.ToUIString()}";

        private string ToDebugString() => $@"{GetType().GetQualifiedName()}: {ToString()}";
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace HarinezumiSama.SimpleBridge125
{
    [DebuggerDisplay(@"{ToDebuggerString(),nq}")]
    public sealed class Move
    {
        public Move(IReadOnlyList<PlayingCard> cards, bool isBridgeDeclared, PlayingCardSuit? requestedSuit)
        {
            if (cards is null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            if (cards.Count == 0)
            {
                throw new ArgumentException(@"A move must contain at least one card.", nameof(cards));
            }

            var cardRank = cards[0].Rank;
            for (var index = 1; index < cards.Count - 1; index++)
            {
                if (cards[index].Rank != cardRank)
                {
                    throw new ArgumentException(@"The cards in a move must be of the same rank.", nameof(cards));
                }
            }

            if (cardRank == PlayingCardRank.Jack)
            {
                if (!requestedSuit.HasValue)
                {
                    throw new ArgumentNullException(
                        nameof(requestedSuit),
                        $@"The requested suit must be specified when the card rank is '{PlayingCardRank.Jack}'.");
                }
            }
            else
            {
                if (requestedSuit.HasValue)
                {
                    throw new ArgumentException(
                        $@"The requested suit cannot be specified when the card rank is '{cardRank}'.",
                        nameof(requestedSuit));
                }
            }

            Cards = cards.ToArray().AsReadOnly();
            FirstCard = cards.First();
            LastCard = cards.Last();
            IsBridgeDeclared = isBridgeDeclared;
            RequestedSuit = requestedSuit;
        }

        public IReadOnlyList<PlayingCard> Cards { get; }

        public PlayingCard FirstCard { get; }

        public PlayingCard LastCard { get; }

        public bool IsBridgeDeclared { get; }

        public PlayingCardSuit? RequestedSuit { get; }

        public override string ToString()
            => $@"{nameof(Cards)}.{nameof(Cards.Count)} = {Cards.Count}, {nameof(IsBridgeDeclared)} = {
                IsBridgeDeclared}, {nameof(RequestedSuit)} = {RequestedSuit.ToUIString()}";

        private string ToDebuggerString() => $@"{GetType().GetQualifiedName()}: {ToString()}";
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Omnifactotum;

namespace HarinezumiSama.SimpleBridge125
{
    //// ReSharper disable once UseNameofExpression :: False positive
    [DebuggerDisplay(@"{ToString(),nq}")]
    public sealed class Player
    {
        private readonly HashSet<PlayingCard> _cards;

        public Player(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    @"The value can be neither empty nor whitespace-only string nor null.",
                    nameof(name));
            }

            Name = name;

            _cards = new HashSet<PlayingCard>();
            Cards = _cards.AsReadOnly();
            Score = 0;
        }

        public string Name
        {
            get;
        }

        public int Score
        {
            get;
            private set;
        }

        public ReadOnlySet<PlayingCard> Cards
        {
            get;
        }

        public void AppendCard(PlayingCard card)
        {
            if (!_cards.Add(card))
            {
                throw new ArgumentException($@"The player {Name.ToUIString()} already has card {card} in hand.");
            }
        }

        public void RemoveCard(PlayingCard card)
        {
            if (!_cards.Remove(card))
            {
                throw new ArgumentException($@"The player {Name.ToUIString()} does not have card {card} in hand.");
            }
        }

        public override string ToString()
            => $@"{GetType().GetQualifiedName()}: {nameof(Name)} = {Name.ToUIString()}, {nameof(Score)} = {Score}, {
                nameof(Cards)}.{nameof(Cards.Count)} = {Cards.Count}";
    }
}
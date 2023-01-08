using System;
using System.Collections.Generic;
using System.Linq;
using Omnifactotum;

namespace HarinezumiSama.SimpleBridge125;

public static class Constants
{
    public static readonly IReadOnlyCollection<PlayingCardSuit> Suits =
        EnumFactotum.GetAllValues<PlayingCardSuit>().AsReadOnly();

    public static readonly IReadOnlyCollection<PlayingCardRank> Ranks =
        EnumFactotum.GetAllValues<PlayingCardRank>().AsReadOnly();

    public static class Cards
    {
        public static readonly IReadOnlyList<PlayingCard> All = CreateAll();
        public static readonly IReadOnlyList<PlayingCard> Empty = Array.Empty<PlayingCard>().AsReadOnly();

        public static readonly IReadOnlyList<PlayingCard> Trump =
            All.Where(card => card.Rank == PlayingCardRank.Jack).ToArray().AsReadOnly();

        public static readonly IReadOnlyDictionary<PlayingCardSuit, IReadOnlyList<PlayingCard>> BySuit =
            new System.Collections.ObjectModel.ReadOnlyDictionary<PlayingCardSuit, IReadOnlyList<PlayingCard>>(
                EnumFactotum
                    .GetAllValues<PlayingCardSuit>()
                    .ToDictionary(
                        suit => suit,
                        suit => (IReadOnlyList<PlayingCard>)All.Where(card => card.Suit == suit).ToArray().AsReadOnly()));

        public static readonly IReadOnlyDictionary<PlayingCardRank, IReadOnlyList<PlayingCard>> ByRank =
            new System.Collections.ObjectModel.ReadOnlyDictionary<PlayingCardRank, IReadOnlyList<PlayingCard>>(
                EnumFactotum
                    .GetAllValues<PlayingCardRank>()
                    .ToDictionary(
                        rank => rank,
                        rank => (IReadOnlyList<PlayingCard>)All.Where(card => card.Rank == rank).ToArray().AsReadOnly()));

        private static IReadOnlyList<PlayingCard> CreateAll()
        {
            var allCards = new PlayingCard[Ranks.Count * Suits.Count];

            var index = 0;
            foreach (var rank in Ranks)
            {
                foreach (var suit in Suits)
                {
                    allCards[index++] = new PlayingCard(rank, suit);
                }
            }

            return allCards.AsReadOnly();
        }
    }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Omnifactotum;

namespace HarinezumiSama.SimpleBridge125;

public static class Constants
{
    public static class CardRanks
    {
        public static readonly IReadOnlySet<CardRank> All = CoreConstants.AllCardRanks;

        public const CardRank Trump = CardRank.Jack;
        public const CardRank SuitRequesting = Trump;
    }

    public static class CardSuits
    {
        public static readonly IReadOnlySet<CardSuit> All = CoreConstants.AllCardSuits;
    }

    public static class Cards
    {
        public static readonly IReadOnlyList<Card> None = ImmutableList<Card>.Empty;

        public static readonly IReadOnlyList<Card> All = CoreConstants.AllCards;

        public static readonly IReadOnlySet<Card> Trump = CoreConstants.AllCards.Where(card => card.Rank == CardRanks.Trump).ToImmutableHashSet();

        public static readonly IReadOnlyDictionary<CardSuit, IReadOnlySet<Card>> BySuit =
            EnumFactotum
                .GetAllValues<CardSuit>()
                .ToImmutableDictionary(
                    suit => suit,
                    suit => (IReadOnlySet<Card>)CoreConstants.AllCards.Where(card => card.Suit == suit).ToImmutableHashSet());

        public static readonly IReadOnlyDictionary<CardRank, IReadOnlySet<Card>> ByRank =
            EnumFactotum
                .GetAllValues<CardRank>()
                .ToImmutableDictionary(
                    rank => rank,
                    rank => (IReadOnlySet<Card>)CoreConstants.AllCards.Where(card => card.Rank == rank).ToImmutableHashSet());
    }

    private static class CoreConstants
    {
        public static readonly IReadOnlySet<CardSuit> AllCardSuits = EnumFactotum.GetAllValues<CardSuit>().ToImmutableHashSet();

        public static readonly IReadOnlySet<CardRank> AllCardRanks = EnumFactotum.GetAllValues<CardRank>().ToImmutableHashSet();

        public static readonly IReadOnlyList<Card> AllCards =
            AllCardRanks.SelectMany(rank => AllCardSuits.Select(suit => new Card(rank, suit))).ToImmutableList();
    }
}
using System;
using System.Collections.Generic;
using Omnifactotum;

namespace HarinezumiSama.SimpleBridge125
{
    public static class PlayingCards
    {
        public static readonly IReadOnlyCollection<PlayingCard> All = CreateAll();

        private static IReadOnlyCollection<PlayingCard> CreateAll()
        {
            var allRanks = EnumFactotum.GetAllValues<PlayingCardRank>();
            var allSuits = EnumFactotum.GetAllValues<PlayingCardSuit>();

            var allCards = new PlayingCard[allRanks.Length * allSuits.Length];

            var index = 0;
            foreach (var rank in allRanks)
            {
                foreach (var suit in allSuits)
                {
                    allCards[index++] = new PlayingCard(rank, suit);
                }
            }

            return allCards.AsReadOnly();
        }
    }
}
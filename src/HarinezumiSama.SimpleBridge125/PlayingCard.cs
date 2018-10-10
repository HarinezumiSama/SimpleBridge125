namespace HarinezumiSama.SimpleBridge125
{
    public struct PlayingCard
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

        public override string ToString()
        {
            return $@"{Rank.AsString()}{Suit.AsString()}";
        }
    }
}
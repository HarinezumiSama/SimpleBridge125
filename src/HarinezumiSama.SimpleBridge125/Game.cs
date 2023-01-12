using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarinezumiSama.SimpleBridge125.Abstractions;

namespace HarinezumiSama.SimpleBridge125;

[DebuggerDisplay(@"{ToDebuggerString(),nq}")]
public sealed class Game
{
    private const int MinPlayerCount = 2;
    private const int MaxPlayerCount = 5;

    private const int CardsPerPlayer = 5;

    public Game(IRandomNumberProvider randomNumberProvider, IReadOnlyCollection<string> playerNames)
    {
        if (randomNumberProvider is null)
        {
            throw new ArgumentNullException(nameof(randomNumberProvider));
        }

        if (playerNames is null)
        {
            throw new ArgumentNullException(nameof(playerNames));
        }

        if (playerNames.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException(@"The collection contains a null or blank player name.", nameof(playerNames));
        }

        if (playerNames.Count is < MinPlayerCount or > MaxPlayerCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(playerNames),
                playerNames.Count,
                $@"The number of players is out of the valid range ({MinPlayerCount} .. {MaxPlayerCount}).");
        }

        Players = playerNames.Select(name => new Player(name)).ToArray().AsReadOnly();
        DrawingStack = new CardStack(randomNumberProvider, Constants.Cards.All);
        ActiveStack = new CardStack(randomNumberProvider, Constants.Cards.Empty);
        CurrentDealerIndex = 0;
        CurrentPlayerIndex = 0;
        PointsRatio = 1;
        RequiredFirstCard = null;

        Initialize();
    }

    public IReadOnlyList<Player> Players { get; }

    public CardStack DrawingStack { get; }

    public CardStack ActiveStack { get; }

    public int CurrentDealerIndex { get; private set; }

    public Player CurrentDealer => Players[CurrentDealerIndex];

    public int CurrentPlayerIndex { get; private set; }

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    public Move? LastMove { get; private set; }

    public int PointsRatio { get; private set; }

    public PlayingCard? RequiredFirstCard { get; private set; }

    public override string ToString()
        => $@"{nameof(Players)}.{nameof(Players.Count)} = {Players.Count}, {nameof(CurrentDealerIndex)} = {
            CurrentDealerIndex}, {nameof(PointsRatio)} = {PointsRatio}, {nameof(DrawingStack)}.{
                nameof(DrawingStack.Cards)}.{nameof(DrawingStack.Cards.Count)} = {DrawingStack.Cards.Count}, {
                    nameof(ActiveStack)}.{nameof(ActiveStack.Cards)}.{nameof(ActiveStack.Cards.Count)} = {
                        ActiveStack.Cards.Count}";

    public HashSet<PlayingCard> GetValidMoveCards()
    {
        if (ActiveStack.Cards.Count == 0)
        {
            return new HashSet<PlayingCard>(Constants.Cards.All);
        }

        var lastCard = ActiveStack.Cards[^1];
        if (LastMove is null)
        {
            throw new InvalidOperationException(
                @"[Internal error] The active stack is not empty, but the last move is not set.");
        }

        if (LastMove.LastCard != lastCard)
        {
            throw new InvalidOperationException(
                $@"[Internal error] Inconsistency between the last card ({lastCard}) and last move ({LastMove}).");
        }

        var requestedSuit = LastMove.RequestedSuit ?? lastCard.Suit;

        var result = new HashSet<PlayingCard>(Constants.Cards.Trump);
        result.UnionWith(Constants.Cards.BySuit[requestedSuit]);
        result.UnionWith(Constants.Cards.ByRank[lastCard.Rank]);

        return result;
    }

    public void MakeMove(Move move)
    {
        if (move is null)
        {
            //// TODO [HarinezumiSama] Use null as 'pass'?
            throw new ArgumentNullException(nameof(move));
        }

        if (RequiredFirstCard.HasValue && move.FirstCard != RequiredFirstCard.Value)
        {
            throw new ArgumentException(
                $@"The player {CurrentPlayer.Name.ToUIString()} is a dealer and their first card in the first move must be {
                    RequiredFirstCard.Value} (but was {move.FirstCard}).",
                nameof(move));
        }

        var validMoveCards = GetValidMoveCards();
        if (!validMoveCards.Contains(move.FirstCard))
        {
            var validMoveCardsString = validMoveCards.Select(card => card.ToString()).Join(", ");

            throw new ArgumentException(
                $@"The player {CurrentPlayer.Name.ToUIString()} cannot make a move with {
                    move.FirstCard} as this card is invalid for the current game state (valid cards: {
                        validMoveCardsString}).",
                nameof(move));
        }

        if (move.IsBridgeDeclared && !CanDeclareBridgeWith(move))
        {
            throw new ArgumentException(
                $@"The player {CurrentPlayer.Name.ToUIString()} cannot declare bridge with {move}.",
                nameof(move));
        }

        var newCurrentPlayerIndex = CurrentPlayerIndex;
        var victimPlayerIndex = GetNextPlayerIndex(CurrentPlayerIndex);

        void MoveToNextPlayer(bool skipMove)
        {
            GetNextPlayerIndex(ref newCurrentPlayerIndex);

            if (!skipMove)
            {
                return;
            }

            GetNextPlayerIndex(ref victimPlayerIndex);
            if (victimPlayerIndex == CurrentPlayerIndex)
            {
                GetNextPlayerIndex(ref victimPlayerIndex);
            }
        }

        void DrawCardsToVictimPlayer(int count)
        {
            if (count <= 0 || count > Constants.Cards.All.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    count,
                    $@"The value is out of the valid range (1 .. {Constants.Cards.All.Count}).");
            }

            for (var index = 0; index < count; index++)
            {
                var card = DrawCard();

                var nextPlayer = Players[victimPlayerIndex];
                nextPlayer.AppendCard(card);
            }
        }

        if (move.FirstCard.Rank != PlayingCardRank.Six)
        {
            MoveToNextPlayer(false);
        }

        for (var index = 0; index < move.Cards.Count; index++)
        {
            var card = move.Cards[index];

            if (!CurrentPlayer.Cards.Contains(card))
            {
                throw new ArgumentException(
                    $@"The player {CurrentPlayer.Name.ToUIString()} cannot make a move with {
                        card} since they don't have this card in hand.",
                    nameof(move));
            }

            CurrentPlayer.RemoveCard(card);
            ActiveStack.DepositCardOnTop(card);

            switch (card.Rank)
            {
                case PlayingCardRank.Seven:
                    DrawCardsToVictimPlayer(1);
                    break;

                case PlayingCardRank.Eight:
                    DrawCardsToVictimPlayer(2);
                    MoveToNextPlayer(true);
                    break;

                case PlayingCardRank.Six:
                case PlayingCardRank.Nine:
                case PlayingCardRank.Ten:
                case PlayingCardRank.Jack:
                case PlayingCardRank.King:
                    break;

                case PlayingCardRank.Queen:
                    if (card.Suit == PlayingCardSuit.Spades && index == move.Cards.Count - 1)
                    {
                        DrawCardsToVictimPlayer(5);
                        MoveToNextPlayer(true);
                    }

                    break;

                case PlayingCardRank.Ace:
                    MoveToNextPlayer(true);
                    break;

                default:
                    throw card.Rank.CreateEnumValueNotImplementedException();
            }
        }

        if (move.IsBridgeDeclared
            || (CurrentPlayer.Cards.Count == 0 && move.FirstCard.Rank != PlayingCardRank.Six))
        {
            throw new NotImplementedException("[TODO] Implement end of round.");
        }

        CurrentPlayerIndex = newCurrentPlayerIndex;
        LastMove = move;
        RequiredFirstCard = null;
    }

    public bool CanDeclareBridgeWith(Move move)
    {
        var rank = move.FirstCard.Rank;

        var sameRankCount = move.Cards.Count;
        for (var index = ActiveStack.Cards.Count - 1; index >= 0; index--)
        {
            if (ActiveStack.Cards[index].Rank != rank)
            {
                break;
            }

            sameRankCount++;
        }

        return sameRankCount >= Constants.Suits.Count;
    }

    private int GetNextPlayerIndex(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Players.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(playerIndex),
                playerIndex,
                $@"The value is out of the valid range (0 .. {Players.Count - 1}).");
        }

        return (playerIndex + 1) % Players.Count;
    }

    private void GetNextPlayerIndex(ref int playerIndex) => playerIndex = GetNextPlayerIndex(playerIndex);

    private PlayingCard DrawCard()
    {
        if (DrawingStack.IsEmpty)
        {
            throw new InvalidOperationException(@"[Internal error] The drawing stack is empty.");
        }

        var result = DrawingStack.WithdrawTopCard();

        if (DrawingStack.IsEmpty)
        {
            var refillCards = ActiveStack.WithdrawAllCardsExceptTopCardWithSameRank();
            if (refillCards.Count == 0)
            {
                throw new NotImplementedException("TODO: End the game and calculate the points.");
            }

            DrawingStack.Refill(refillCards);
            DrawingStack.Shuffle();

            PointsRatio++;
        }

        return result;
    }

    private string ToDebuggerString() => $@"{GetType().GetQualifiedName()}: {ToString()}";

    private void Initialize()
    {
        DrawingStack.Shuffle();

        for (var index = 0; index < CardsPerPlayer; index++)
        {
            for (var playerIndex = 0; playerIndex < Players.Count; playerIndex++)
            {
                var actualPlayerIndex = (playerIndex + CurrentDealerIndex + 1) % Players.Count;

                var card = DrawingStack.WithdrawTopCard();
                Players[actualPlayerIndex].AppendCard(card);

                if (index == CardsPerPlayer - 1 && actualPlayerIndex == CurrentDealerIndex)
                {
                    RequiredFirstCard = card;
                }
            }
        }

        if (ActiveStack.Cards.Count != 0)
        {
            throw new InvalidOperationException(
                $@"[Internal error] After initialization the active stack must be empty but it has {ActiveStack.Cards.Count} cards.");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Omnifactotum.Annotations;

namespace HarinezumiSama.SimpleBridge125
{
    //// ReSharper disable once UseNameofExpression :: False positive
    [DebuggerDisplay(@"{ToDebugString(),nq}")]
    public sealed class Game
    {
        private const int MinPlayerCount = 2;
        private const int MaxPlayerCount = 5;

        private const int CardsPerPlayer = 5;

        public Game([NotNull] IReadOnlyCollection<string> playerNames)
        {
            if (playerNames is null)
            {
                throw new ArgumentNullException(nameof(playerNames));
            }
            if (playerNames.Any(item => item is null))
            {
                throw new ArgumentException(@"The collection contains a null element.", nameof(playerNames));
            }

            if (playerNames.Count < MinPlayerCount || playerNames.Count > MaxPlayerCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(playerNames),
                    playerNames.Count,
                    $@"The number of players is out of the valid range ({MinPlayerCount} .. {MaxPlayerCount}).");
            }

            Players = playerNames.Select(name => new Player(name)).ToArray().AsReadOnly();
            DrawingStack = new CardStack(Constants.Cards.All);
            ActiveStack = new CardStack(Constants.Cards.Empty);
            CurrentDealerIndex = 0;
            CurrentPlayerIndex = 0;
            PointsRatio = 1;

            Initialize();
        }

        public IReadOnlyList<Player> Players
        {
            get;
        }

        public CardStack DrawingStack
        {
            get;
        }

        public CardStack ActiveStack
        {
            get;
        }

        public int CurrentDealerIndex
        {
            get;
            private set;
        }

        public int CurrentPlayerIndex
        {
            get;
            private set;
        }

        public Player CurrentPlayer => Players[CurrentPlayerIndex];

        public Move LastMove
        {
            get;
            private set;
        }

        public int PointsRatio
        {
            get;
            private set;
        }

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

            var lastCard = ActiveStack.Cards.Last();
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

        public void MakeMove([NotNull] Move move)
        {
            if (move is null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            foreach (var card in move.Cards)
            {
                if (!CurrentPlayer.Cards.Contains(card))
                {
                    throw new ArgumentException(
                        $@"The player {CurrentPlayer.Name.ToUIString()} cannot make a move with {
                            card} since they don't have this card in hand.",
                        nameof(move));
                }
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

            throw new NotImplementedException();
        }

        private bool CanDeclareBridgeWith([NotNull] Move move)
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

        private string ToDebugString() => $@"{GetType().GetQualifiedName()}: {ToString()}";

        private void Initialize()
        {
            DrawingStack.Shuffle();

            for (var index = 0; index < CardsPerPlayer; index++)
            {
                for (var playerIndex = 0; playerIndex < Players.Count; playerIndex++)
                {
                    var actualPlayerIndex = (playerIndex + CurrentDealerIndex + 1) % Players.Count;

                    var card = DrawingStack.WithdrawTopCard();
                    if (index == CardsPerPlayer - 1 && actualPlayerIndex == CurrentDealerIndex)
                    {
                        ActiveStack.DepositCardOnTop(card);
                    }
                    else
                    {
                        Players[actualPlayerIndex].AppendCard(card);
                    }
                }
            }

            if (ActiveStack.Cards.Count != 1)
            {
                throw new InvalidOperationException(
                    $@"[Internal error] After initialization the active stack must have exactly one card but it has {
                        ActiveStack.Cards.Count} cards.");
            }
        }
    }
}
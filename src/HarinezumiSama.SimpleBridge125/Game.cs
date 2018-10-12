﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Omnifactotum.Annotations;

namespace HarinezumiSama.SimpleBridge125
{
    //// ReSharper disable once UseNameofExpression :: False positive
    [DebuggerDisplay(@"{ToString(),nq}")]
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
            DrawingStack = new CardStack(PlayingCards.All);
            ActiveStack = new CardStack(PlayingCards.Empty);
            CurrentDealerIndex = 0;
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

        public int PointsRatio
        {
            get;
            private set;
        }

        public override string ToString()
            => $@"{GetType().GetQualifiedName()}: {nameof(Players)}.{nameof(Players.Count)} = {Players.Count}, {
                nameof(CurrentDealerIndex)} = {CurrentDealerIndex}, {nameof(PointsRatio)} = {PointsRatio}, {
                nameof(DrawingStack)}.{nameof(DrawingStack.Cards)}.{nameof(DrawingStack.Cards.Count)} = {
                DrawingStack.Cards.Count}, {nameof(ActiveStack)}.{nameof(ActiveStack.Cards)}.{
                nameof(ActiveStack.Cards.Count)} = {ActiveStack.Cards.Count}";

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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarinezumiSama.SimpleBridge125.Abstractions;
using Omnifactotum;

namespace HarinezumiSama.SimpleBridge125;

[DebuggerDisplay(@"{ToDebuggerString(),nq}")]
public sealed class Game
{
    private const int MinPlayerCount = 2;
    private const int MaxPlayerCount = 5;

    private const int CardsPerPlayer = 5;

    private readonly List<PlayerMove> _playerMoves;
    private readonly HashSet<Card> _validMoveCards;

    public Game(IRandomNumberProvider randomNumberProvider, IReadOnlyList<string> playerNames)
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
        ActiveStack = new CardStack(randomNumberProvider, Constants.Cards.None);
        CurrentDealerIndex = 0;
        CurrentPlayerIndex = 0;

        _playerMoves = new List<PlayerMove>(Constants.Cards.All.Count);
        PlayerMoves = _playerMoves.AsReadOnly();

        _validMoveCards = new HashSet<Card>();
        ValidMoveCards = _validMoveCards.AsReadOnly();

        PointsRatio = 1;
        RequiredFirstCard = null;

        Initialize();
    }

    public GameState State { get; private set; }

    public IReadOnlyList<Player> Players { get; }

    public CardStack DrawingStack { get; }

    public CardStack ActiveStack { get; }

    public int CurrentDealerIndex { get; private set; }

    public Player CurrentDealer => Players[CurrentDealerIndex];

    public int CurrentPlayerIndex { get; private set; }

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    public IReadOnlyList<PlayerMove> PlayerMoves { get; }

    public IReadOnlySet<Card> ValidMoveCards { get; }

    public int PointsRatio { get; private set; }

    public Card? RequiredFirstCard { get; private set; }

    public override string ToString()
        => $@"{nameof(Players)}.{nameof(Players.Count)} = {Players.Count}, {nameof(CurrentDealerIndex)} = {CurrentDealerIndex}, {
            nameof(PointsRatio)} = {PointsRatio}, {nameof(DrawingStack)}.{nameof(DrawingStack.Cards)}.{nameof(DrawingStack.Cards.Count)} = {
                DrawingStack.Cards.Count}, {nameof(ActiveStack)}.{nameof(ActiveStack.Cards)}.{nameof(ActiveStack.Cards.Count)} = {ActiveStack.Cards.Count}";

    public void MakeMove(Move move)
    {
        if (move is null)
        {
            throw new ArgumentNullException(nameof(move));
        }

        if (!ValidMoveCards.Contains(move.Card))
        {
            throw new ArgumentException(
                $@"Invalid move card ""{move.Card}"". State: {State}. Valid move cards: [{ValidMoveCards.Select(c => c.ToString()).Order().Join(",\x0020")}].",
                nameof(move));
        }

        if (move.IsBridgeDeclared && !CanDeclareBridgeWith(move.Card))
        {
            throw new ArgumentException(
                $@"The player {CurrentPlayer.Name.ToUIString()} cannot declare bridge with ""{move.Card}"".",
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

        if (move.FirstCard.Rank != CardRank.Six)
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
                case CardRank.Seven:
                    DrawCardsToVictimPlayer(1);
                    break;

                case CardRank.Eight:
                    DrawCardsToVictimPlayer(2);
                    MoveToNextPlayer(true);
                    break;

                case CardRank.Six:
                case CardRank.Nine:
                case CardRank.Ten:
                case CardRank.Jack:
                case CardRank.King:
                    break;

                case CardRank.Queen:
                    if (card.Suit == CardSuit.Spades && index == move.Cards.Count - 1)
                    {
                        DrawCardsToVictimPlayer(5);
                        MoveToNextPlayer(true);
                    }

                    break;

                case CardRank.Ace:
                    MoveToNextPlayer(true);
                    break;

                default:
                    throw card.Rank.CreateEnumValueNotImplementedException();
            }
        }

        if (move.IsBridgeDeclared
            || (CurrentPlayer.Cards.Count == 0 && move.FirstCard.Rank != CardRank.Six))
        {
            throw new NotImplementedException("[TODO] Implement end of round.");
        }

        CurrentPlayerIndex = newCurrentPlayerIndex;
        LastMove = move;
        RequiredFirstCard = null;
    }

    public bool CanDeclareBridgeWith(Card card)
    {
        var rank = card.Rank;

        var sameRankCount = 1;
        for (var index = ActiveStack.Cards.Count - 1; index >= 0 && ActiveStack.Cards[index].Rank == rank; index--)
        {
            sameRankCount++;
        }

        return sameRankCount >= Constants.CardSuits.All.Count;
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

    private Card DrawCard()
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

        Card? lastDealtCard = null;

        for (var index = 0; index < CardsPerPlayer; index++)
        {
            for (var playerIndex = 0; playerIndex < Players.Count; playerIndex++)
            {
                var actualPlayerIndex = (playerIndex + CurrentDealerIndex + 1) % Players.Count;

                var card = DrawingStack.WithdrawTopCard();
                Players[actualPlayerIndex].AppendCard(card);

                lastDealtCard = card;
            }
        }

        if (!ActiveStack.IsEmpty)
        {
            throw new InvalidOperationException(
                $@"[Internal error] After initialization the active stack must be empty but it has {ActiveStack.Cards.Count} card(s).");
        }

        RequiredFirstCard = lastDealtCard.EnsureNotNull();

        State = GameState.DealerFirstMove;
        UpdateValidMoveCards();
    }

    private void UpdateValidMoveCards()
    {
        _validMoveCards.Clear();
        switch (State)
        {
            case GameState.DealerFirstMove:
                {
                    Factotum.Assert(RequiredFirstCard is not null);
                    Factotum.Assert(ActiveStack.IsEmpty);
                    Factotum.Assert(PlayerMoves.Count == 0);

                    _validMoveCards.Add(RequiredFirstCard.EnsureNotNull());
                    break;
                }

            case GameState.PlayerTurnStarted:
                {
                    Factotum.Assert(!ActiveStack.IsEmpty);
                    Factotum.Assert(PlayerMoves.Count > 0);

                    var previousCard = ActiveStack.Cards[^1];
                    var (previousPlayer, previousMove) = PlayerMoves[^1];

                    Factotum.Assert(!previousMove.IsBridgeDeclared);
                    Factotum.Assert(previousMove.IsTurnCompleted);
                    Factotum.Assert(previousMove.Card == previousCard);
                    Factotum.Assert(previousPlayer != CurrentPlayer);

                    _validMoveCards.UnionWith(Constants.Cards.Trump);

                    if (previousMove.RequestedSuit is { } requestedSuit)
                    {
                        _validMoveCards.UnionWith(Constants.Cards.BySuit[requestedSuit]);
                    }
                    else
                    {
                        _validMoveCards.UnionWith(Constants.Cards.BySuit[previousCard.Suit]);
                        _validMoveCards.UnionWith(Constants.Cards.ByRank[previousCard.Rank]);
                    }

                    break;
                }

            case GameState.PlayerTurnContinued:
                {
                    Factotum.Assert(!ActiveStack.IsEmpty);
                    Factotum.Assert(PlayerMoves.Count > 0);

                    var previousCard = ActiveStack.Cards[^1];
                    var (previousPlayer, previousMove) = PlayerMoves[^1];

                    Factotum.Assert(!previousMove.IsBridgeDeclared);
                    Factotum.Assert(!previousMove.IsTurnCompleted);
                    Factotum.Assert(previousMove.Card == previousCard);
                    Factotum.Assert(previousPlayer == CurrentPlayer);

                    _validMoveCards.UnionWith(Constants.Cards.ByRank[previousCard.Rank]);
                    break;
                }

            case GameState.RoundEnded:
                // No valid move cards
                break;

            default:
                throw State.CreateEnumValueNotImplementedException();
        }
    }
}
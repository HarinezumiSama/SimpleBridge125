using System;
using System.Diagnostics;

namespace HarinezumiSama.SimpleBridge125;

[DebuggerDisplay(@"{ToDebuggerString(),nq}")]
public sealed record Move
{
    public Move(Card card, bool isBridgeDeclared, CardSuit? requestedSuit, bool isTurnCompleted)
    {
        if (isTurnCompleted && card.Rank == Constants.CardRanks.SuitRequesting)
        {
            if (!requestedSuit.HasValue)
            {
                throw new ArgumentNullException(
                    nameof(requestedSuit),
                    $@"The requested suit must be specified when the card rank is {Constants.CardRanks.SuitRequesting.ToString().ToUIString()}.");
            }
        }
        else
        {
            if (requestedSuit.HasValue)
            {
                throw new ArgumentException(
                    $@"The requested suit cannot be specified when the card rank is {card.Rank.ToString().ToUIString()}.",
                    nameof(requestedSuit));
            }
        }

        if (isBridgeDeclared && !isTurnCompleted)
        {
            throw new ArgumentException("Bridge cannot be declared when the turn is not completed.", nameof(isBridgeDeclared));
        }

        Card = card;
        IsBridgeDeclared = isBridgeDeclared;
        RequestedSuit = requestedSuit?.EnsureDefined();
        IsTurnCompleted = isTurnCompleted;
    }

    public Card Card { get; }

    public bool IsBridgeDeclared { get; }

    public CardSuit? RequestedSuit { get; }

    public bool IsTurnCompleted { get; }

    public override string ToString()
        => $"{nameof(Card)} = {Card}, {nameof(IsBridgeDeclared)} = {IsBridgeDeclared}, {
            nameof(RequestedSuit)} = {RequestedSuit.ToUIString()}, {nameof(IsTurnCompleted)} = {IsTurnCompleted}";

    private string ToDebuggerString() => $@"{GetType().GetQualifiedName()}: {ToString()}";
}
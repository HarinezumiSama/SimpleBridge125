using System;
using System.Diagnostics;

namespace HarinezumiSama.SimpleBridge125;

[DebuggerDisplay(@"{ToDebuggerString(),nq}")]
public readonly record struct Card(CardRank Rank, CardSuit Suit)
{
    public CardRank Rank { get; } = Rank.EnsureDefined();

    public CardSuit Suit { get; } = Suit.EnsureDefined();

    public override string ToString() => $"{Rank.AsString()}{Suit.AsString()}";

    private string ToDebuggerString() => $"{GetType().GetQualifiedName()}: {ToString()}";
}
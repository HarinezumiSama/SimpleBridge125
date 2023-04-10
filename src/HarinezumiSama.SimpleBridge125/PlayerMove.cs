using System;
using System.Diagnostics;

namespace HarinezumiSama.SimpleBridge125;

[DebuggerDisplay(@"{ToDebuggerString(),nq}")]
public readonly record struct PlayerMove(Player Player, Move Move)
{
    public Player Player { get; init; } = Player ?? throw new ArgumentNullException(nameof(Player));

    public Move Move { get; init; } = Move ?? throw new ArgumentNullException(nameof(Move));

    public override string ToString() => $"{nameof(Player)} = {Player}, {nameof(Move)} = {Move}";

    private string ToDebuggerString() => $@"{GetType().GetQualifiedName()}: {ToString()}";
}
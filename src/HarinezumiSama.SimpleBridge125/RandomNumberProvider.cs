using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using HarinezumiSama.SimpleBridge125.Abstractions;

namespace HarinezumiSama.SimpleBridge125;

[ExcludeFromCodeCoverage]
internal sealed class RandomNumberProvider : IRandomNumberProvider
{
    public int GetZeroBasedRandomNumber(int exclusiveUpperBound)
        => exclusiveUpperBound switch
        {
            <= 0 => throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound), exclusiveUpperBound, @"The value must be greater than zero."),
            _ => RandomNumberGenerator.GetInt32(exclusiveUpperBound)
        };
}
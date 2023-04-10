using System;
using HarinezumiSama.SimpleBridge125.Abstractions;
using Moq;
using NUnit.Framework;

namespace HarinezumiSama.SimpleBridge125.UnitTests;

[TestFixture]
internal sealed class GameTests
{
    private Mock<IRandomNumberProvider>? _randomNumberProviderMock;

    [SetUp]
    public void SetUp() => _randomNumberProviderMock = new Mock<IRandomNumberProvider>(MockBehavior.Strict);

    [TearDown]
    public void TearDown() => _randomNumberProviderMock = null;

    [Test]
    [TestCase(new object[] { new[] { "John Doe", "Jill Doe" } })]
    public void TestConstructionWithValidArgument(string[] playerNames)
    {
        _randomNumberProviderMock!
            .Setup(provider => provider.GetZeroBasedRandomNumber(It.Is<int>(exclusiveUpperBound => exclusiveUpperBound > 0)))
            .Returns(0);

        var game = new Game(GetRandomNumberProvider(), playerNames);

        Assert.That(game.Players, Is.Not.Null);
        Assert.That(game.Players.Count, Is.EqualTo(playerNames.Length));
        Assert.That(game.CurrentDealerIndex, Is.EqualTo(0));
        Assert.That(game.PointsRatio, Is.EqualTo(1));
        Assert.That(game.ActiveStack.Cards.Count, Is.EqualTo(0));
        Assert.That(game.RequiredFirstCard, Is.Not.Null);
        Assert.That(game.State, Is.EqualTo(GameState.DealerFirstMove));
    }

    [Test]
    public void TestConstructionWithInvalidArgument()
    {
        Assert.That(() => new Game(GetRandomNumberProvider(), null!), Throws.ArgumentNullException);
        Assert.That(() => new Game(null!, Array.Empty<string>()), Throws.ArgumentNullException);

        Assert.That(() => new Game(GetRandomNumberProvider(), Array.Empty<string>()), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Game(GetRandomNumberProvider(), new[] { "One player" }), Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(() => new Game(GetRandomNumberProvider(), new[] { "One player", null! }), Throws.ArgumentException);
        Assert.That(() => new Game(GetRandomNumberProvider(), new[] { "One player", string.Empty }), Throws.ArgumentException);
    }

    private IRandomNumberProvider GetRandomNumberProvider() => _randomNumberProviderMock.EnsureNotNull().Object;
}
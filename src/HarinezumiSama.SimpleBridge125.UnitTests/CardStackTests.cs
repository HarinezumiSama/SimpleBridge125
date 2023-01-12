using System;
using System.Collections.Immutable;
using System.Linq;
using HarinezumiSama.SimpleBridge125.Abstractions;
using Moq;
using NUnit.Framework;

namespace HarinezumiSama.SimpleBridge125.UnitTests;

[TestFixture]
internal sealed class CardStackTests
{
    private Mock<IRandomNumberProvider>? _randomNumberProviderMock;

    [SetUp]
    public void SetUp() => _randomNumberProviderMock = new Mock<IRandomNumberProvider>(MockBehavior.Strict);

    [TearDown]
    public void TearDown() => _randomNumberProviderMock = null;

    [Test]
    public void TestConstruction()
    {
        var stack = new CardStack(GetRandomNumberProvider(), Constants.Cards.All);
        Assert.That(stack.Cards, Is.EqualTo(Constants.Cards.All));
    }

    [Test]
    public void TestShuffle()
    {
        var allCards = Constants.Cards.All.ToImmutableArray();
        for (var offset = 0; offset < allCards.Length; offset++)
        {
            _randomNumberProviderMock.EnsureNotNull().Reset();

            var offsetCopy = offset;

            _randomNumberProviderMock
                .Setup(provider => provider.GetZeroBasedRandomNumber(It.IsAny<int>()))
                .Returns<int>(
                    exclusiveUpperBound => exclusiveUpperBound <= 0
                        ? throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound), exclusiveUpperBound, null)
                        : offsetCopy < exclusiveUpperBound
                            ? offsetCopy
                            : 0);

            var stack = new CardStack(GetRandomNumberProvider(), allCards);
            for (var index = 0; index < allCards.Length; index++)
            {
                var beforeShuffleCards = stack.Cards.ToArray();

                stack.Shuffle();

                Assert.That(stack.Cards, Is.EquivalentTo(allCards));
                Assert.That(stack.Cards, Is.EquivalentTo(beforeShuffleCards));
                Assert.That(stack.Cards, offset % allCards.Length == 0 ? Is.EqualTo(beforeShuffleCards) : Is.Not.EqualTo(beforeShuffleCards));

                var expectedCards = beforeShuffleCards.Skip(offset).Concat(beforeShuffleCards.Take(offset)).ToArray();
                Assert.That(stack.Cards, Is.EqualTo(expectedCards));

                if ((index + 1) % allCards.Length == 0)
                {
                    Assert.That(expectedCards, Is.EqualTo(allCards));
                }
            }
        }
    }

    private IRandomNumberProvider GetRandomNumberProvider() => _randomNumberProviderMock.EnsureNotNull().Object;
}
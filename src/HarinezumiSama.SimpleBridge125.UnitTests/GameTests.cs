using System;
using NUnit.Framework;

namespace HarinezumiSama.SimpleBridge125.UnitTests
{
    [TestFixture]
    internal sealed class GameTests
    {
        [Test]
        [TestCase(new object[] { new[] { "John Doe", "Jill Doe" } })]
        public void TestConstructionWithValidArgument(string[] playerNames)
        {
            var game = new Game(playerNames);

            Assert.That(game.Players, Is.Not.Null);
            Assert.That(game.Players.Count, Is.EqualTo(playerNames.Length));
            Assert.That(game.CurrentDealerIndex, Is.EqualTo(0));
            Assert.That(game.PointsRatio, Is.EqualTo(1));
            Assert.That(game.ActiveStack.Cards.Count, Is.EqualTo(0));
            Assert.That(game.RequiredFirstCard, Is.Not.Null);
        }

        [Test]
        public void TestConstructionWithInvalidArgument()
        {
            //// ReSharper disable once AssignNullToNotNullAttribute - Negative test case
            Assert.That(() => new Game(null), Throws.ArgumentNullException);

            Assert.That(() => new Game(new string[0]), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new Game(new[] { "One player" }), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new Game(new[] { "One player", null }), Throws.ArgumentException);
            Assert.That(() => new Game(new[] { "One player", string.Empty }), Throws.ArgumentException);
        }
    }
}
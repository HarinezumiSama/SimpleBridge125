using NUnit.Framework;

namespace HarinezumiSama.SimpleBridge125.UnitTests
{
    [TestFixture]
    internal sealed class GameTests
    {
        [Test]
        public void TestConstruction()
        {
            var game = new Game(new[] { "John Doe", "Jill Doe" });

            Assert.That(game.Players, Is.Not.Null);
            Assert.That(game.Players.Count, Is.EqualTo(2));
            Assert.That(game.CurrentDealerIndex, Is.EqualTo(0));
            Assert.That(game.PointsRatio, Is.EqualTo(1));
            Assert.That(game.ActiveStack.Cards.Count, Is.EqualTo(1));
        }
    }
}
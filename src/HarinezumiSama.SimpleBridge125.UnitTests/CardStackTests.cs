using System.Linq;
using NUnit.Framework;

namespace HarinezumiSama.SimpleBridge125.UnitTests
{
    [TestFixture]
    internal sealed class CardStackTests
    {
        private const int ShuffleCount = 1024;

        [Test]
        public void TestConstruction()
        {
            var stack = new CardStack(PlayingCards.All);
            Assert.That(stack.Cards, Is.EqualTo(PlayingCards.All));
        }

        [Test]
        public void TestShuffle()
        {
            var stack = new CardStack(PlayingCards.All);

            for (var index = 0; index < ShuffleCount; index++)
            {
                var beforeShuffleCards = stack.Cards.ToArray();

                stack.Shuffle();

                Assert.That(stack.Cards, Is.EquivalentTo(PlayingCards.All));
                Assert.That(stack.Cards, Is.Not.EqualTo(beforeShuffleCards));
            }
        }
    }
}
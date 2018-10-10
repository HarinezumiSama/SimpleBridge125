using System;
using NUnit.Framework;

namespace HarinezumiSama.SimpleBridge125.UnitTests
{
    [TestFixture]
    internal sealed class PlayingCardsTests
    {
        [Test]
        public void TestAll()
        {
            Assert.That(PlayingCards.All, Is.Not.Null);
            Assert.That(PlayingCards.All.Count, Is.EqualTo(4 * 9));
        }
    }
}
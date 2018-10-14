using NUnit.Framework;

namespace HarinezumiSama.SimpleBridge125.UnitTests
{
    [TestFixture]
    internal sealed class PlayingCardsTests
    {
        [Test]
        public void TestAll()
        {
            Assert.That(Constants.Cards.All, Is.Not.Null);
            Assert.That(Constants.Cards.All.Count, Is.EqualTo(36));
        }
    }
}
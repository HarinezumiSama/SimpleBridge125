using System.Collections.Generic;
using Omnifactotum.Annotations;

namespace HarinezumiSama.SimpleBridge125
{
    public sealed class DrawingStack : CardStack
    {
        public DrawingStack()
            : base(PlayingCards.All)
        {
            // Nothing to do
        }
    }
}
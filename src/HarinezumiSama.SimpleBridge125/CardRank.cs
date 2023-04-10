using System.ComponentModel;

namespace HarinezumiSama.SimpleBridge125;

public enum CardRank
{
    [Description("6")]
    Six,

    [Description("7")]
    Seven,

    [Description("8")]
    Eight,

    [Description("9")]
    Nine,

    [Description("10")]
    Ten,

    [Description("J")]
    Jack,

    [Description("Q")]
    Queen,

    [Description("K")]
    King,

    [Description("A")]
    Ace
}
namespace HarinezumiSama.SimpleBridge125.Abstractions;

public interface IRandomNumberProvider
{
    int GetZeroBasedRandomNumber(int exclusiveUpperBound);
}
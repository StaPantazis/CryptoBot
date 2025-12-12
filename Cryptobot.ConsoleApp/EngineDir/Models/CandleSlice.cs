namespace Cryptobot.ConsoleApp.EngineDir.Models;

public class CandleSlice<T>(int size) where T : Candle
{
    private readonly List<T> _candles = new(size);

    public CandleSlice(List<T> candles) : this(candles.Count)
    {
        _candles = candles;
    }

    public IReadOnlyList<T> Candles => _candles;
    public T? LastCandle { get; private set; }
    public T? LiveCandle { get; private set; }

    public void AddLiveCandle(T liveCandle)
    {
        if (LiveCandle != null)
        {
            _candles.Add(LiveCandle);
        }

        LastCandle = LiveCandle;
        LiveCandle = liveCandle;

        if (_candles.Count > size)
        {
            _candles.RemoveAt(0);
        }
    }

    public CandleSlice<T> GetSlice(int size)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(size, _candles.Count);

        var newSliceCandles = _candles.Skip(_candles.Count - size).Take(size).ToList();
        return new(newSliceCandles);
    }
}

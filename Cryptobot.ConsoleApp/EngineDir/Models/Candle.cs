namespace Cryptobot.ConsoleApp.EngineDir.Models;

public abstract class Candle
{
    private long? _dayTicks;

    public string Id { get; protected set; } = Guid.NewGuid().ToString();
    public long DayTicks
    {
        get
        {
            _dayTicks ??= OpenTime.Date.Ticks;
            return _dayTicks.Value;
        }
    }
    public DateTime OpenTime { get; set; }
    public DateTime CloseTime { get; set; }
    public double OpenPrice { get; set; }
    public double ClosePrice { get; set; }
    public double HighPrice { get; set; }
    public double LowPrice { get; set; }
    /// <summary>
    /// Total number of Bitcoin traded
    /// </summary>
    public double Volume { get; set; }
    /// <summary>
    /// Total number of Dollars traded
    /// </summary>
    public double QuoteVolume { get; set; }
    public Indicators Indicators { get; } = new();

    public double PriceDif => Math.Round(ClosePrice - OpenPrice, 2);

    public override string ToString() => $"{OpenTime:dd/MM/yyyy HH:mm}| {Math.Round(OpenPrice, 2)} / {Math.Round(ClosePrice, 2)}  ({PriceDif})";
}

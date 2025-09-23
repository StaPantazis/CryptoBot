namespace Cryptobot.ConsoleApp.Binance;

public class BinanceCandlestick
{
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
    /// <summary>
    /// Number of trades.
    /// </summary>
    public int Count { get; set; }
    public double TakerBuyVolume { get; set; }
    public double TakerBuyQuoteVolume { get; set; }
    public bool Ignore { get; set; }

    public double PriceDifPercentage => Math.Round(ClosePrice - OpenPrice, 2);

    public override string ToString() => $"{OpenTime:dd/MM HH:mm}| {Math.Round(OpenPrice, 2)} / {Math.Round(ClosePrice, 2)}  ({PriceDifPercentage})";
}

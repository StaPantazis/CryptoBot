using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.ConsoleApp.Binance;

public class BinanceCandle : Candle
{
    /// <summary>
    /// Number of trades.
    /// </summary>
    public int Count { get; set; }
    public double TakerBuyVolume { get; set; }
    public double TakerBuyQuoteVolume { get; set; }
    public bool Ignore { get; set; }
}

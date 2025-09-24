using Cryptobot.ConsoleApp.Bybit.Models;

namespace Cryptobot.ConsoleApp.Backtesting;

public class BybitOutputCandle : BybitCandle
{
    public int? TradeIndex { get; set; }
    public double? EntryPrice { get; set; }
    public double? StopLoss { get; set; }
    public double? TakeProfit { get; set; }
    public double? TradeSize { get; set; }
    public double? ExitPrice { get; set; }
    public double? PnL { get; set; }
    public bool? IsProfit { get; set; }

    public static BybitOutputCandle FromBybitCandle(BybitCandle candle)
    {
        return new()
        {
            Id = candle.Id,
            OpenTime = candle.OpenTime,
            CloseTime = candle.CloseTime,
            OpenPrice = candle.OpenPrice,
            ClosePrice = candle.ClosePrice,
            HighPrice = candle.HighPrice,
            LowPrice = candle.LowPrice,
            Volume = candle.Volume,
            QuoteVolume = candle.QuoteVolume,
        };
    }
}

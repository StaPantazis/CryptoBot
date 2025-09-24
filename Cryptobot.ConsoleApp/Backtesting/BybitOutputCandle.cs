using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Extensions;

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
            OpenPrice = candle.OpenPrice.Round(1),
            ClosePrice = candle.ClosePrice.Round(1),
            HighPrice = candle.HighPrice.Round(1),
            LowPrice = candle.LowPrice.Round(1),
            Volume = candle.Volume.Round(1),
            QuoteVolume = candle.QuoteVolume.Round(1),
        };
    }
}

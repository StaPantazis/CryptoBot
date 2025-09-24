using Cryptobot.ConsoleApp.Backtesting;
using Newtonsoft.Json.Serialization;

namespace Cryptobot.ConsoleApp.Utils;

public class ShortNameContractResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
        return propertyName switch
        {
            nameof(BybitOutputCandle.TradeIndex) => "ti",
            nameof(BybitOutputCandle.EntryPrice) => "ep",
            nameof(BybitOutputCandle.StopLoss) => "sl",
            nameof(BybitOutputCandle.TakeProfit) => "tp",
            nameof(BybitOutputCandle.TradeSize) => "ts",
            nameof(BybitOutputCandle.ExitPrice) => "xp",
            nameof(BybitOutputCandle.PnL) => "p",
            nameof(BybitOutputCandle.IsProfit) => "pr",
            nameof(BybitOutputCandle.OpenTime) => "t",
            nameof(BybitOutputCandle.CloseTime) => "ct",
            nameof(BybitOutputCandle.OpenPrice) => "o",
            nameof(BybitOutputCandle.ClosePrice) => "c",
            nameof(BybitOutputCandle.HighPrice) => "h",
            nameof(BybitOutputCandle.LowPrice) => "l",
            nameof(BybitOutputCandle.Volume) => "v",
            nameof(BybitOutputCandle.QuoteVolume) => "q",
            _ => base.ResolvePropertyName(propertyName)
        };
    }
}
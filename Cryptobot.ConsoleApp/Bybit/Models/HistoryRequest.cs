using Cryptobot.ConsoleApp.Bybit.Enums;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Bybit.Models;

public record HistoryRequest(CandlestickInterval Interval, string Symbol, string MarketCategory)
{
    public string SymbolDescribed => Symbol switch
    {
        Constants.SYMBOL_BTCUSDT => "BTC_USDT",
        _ => throw new NotImplementedException()
    };

    public string MarketCategoryDescribed => MarketCategory switch
    {
        Constants.MARKET_SPOT => "Spot",
        Constants.MARKET_PERPETUAL_FUTURES => "Perpetual Futures",
        Constants.MARKET_INVERSE_PERPETUAL_FUTURES => "Inverse Perpetual Futures",
        Constants.MARKET_OPTIONS => "Options",
        _ => throw new NotImplementedException()
    };

    public string IntervalShortString => Interval switch
    {
        CandlestickInterval.One_Minute => "1m",
        CandlestickInterval.Five_Minutes => "5m",
        _ => throw new NotImplementedException(),
    };

    public int CandlesticksDailyCount => Interval switch
    {
        CandlestickInterval.One_Minute => 1440,
        CandlestickInterval.Five_Minutes => 288,
        _ => throw new NotImplementedException()
    };
}

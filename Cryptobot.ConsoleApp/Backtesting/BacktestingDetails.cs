using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Backtesting;

public record BacktestingDetails(CandleInterval Interval, string Symbol, string MarketCategory, double Budget = 10000, params StrategyBundleBase[] Strategies)
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
        CandleInterval.One_Minute => "1m",
        CandleInterval.Three_Minutes => "3m",
        CandleInterval.Five_Minutes => "5m",
        CandleInterval.Fifteen_Minutes => "15m",
        _ => throw new NotImplementedException(),
    };

    public int CandlesticksDailyCount => Interval switch
    {
        CandleInterval.One_Minute => 1440,
        CandleInterval.Three_Minutes => 480,
        CandleInterval.Five_Minutes => 288,
        CandleInterval.Fifteen_Minutes => 96,
        _ => throw new NotImplementedException()
    };
}

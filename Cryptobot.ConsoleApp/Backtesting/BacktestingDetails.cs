using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Backtesting;

public class BacktestingDetails
{
    public CandleInterval Interval { get; }
    public string Symbol { get; }
    public string MarketCategory { get; }
    public double Budget { get; }
    public StrategyBundleBase[] Strategies { get; }

    public BacktestingDetails(
        CandleInterval interval,
        string symbol = Constants.SYMBOL_BTCUSDT,
        string marketCategory = Constants.MARKET_PERPETUAL_FUTURES,
        double budget = 10000,
        params StrategyBundleBase[] strategies)
    {
        Interval = interval;
        Symbol = symbol;
        MarketCategory = marketCategory;
        Budget = budget;
        Strategies = strategies;
    }

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
        CandleInterval.One_Day => "1d",
        _ => throw new NotImplementedException(),
    };

    public int CandlesticksDailyCount => Interval switch
    {
        CandleInterval.One_Minute => 1440,
        CandleInterval.Three_Minutes => 480,
        CandleInterval.Five_Minutes => 288,
        CandleInterval.Fifteen_Minutes => 96,
        CandleInterval.One_Day => 1,
        _ => throw new NotImplementedException()
    };

    public int IntervalMinutes => Interval switch
    {
        CandleInterval.One_Minute => 1,
        CandleInterval.Three_Minutes => 3,
        CandleInterval.Five_Minutes => 5,
        CandleInterval.Fifteen_Minutes => 15,
        CandleInterval.One_Day => 1440,
        _ => throw new NotImplementedException()
    };
}

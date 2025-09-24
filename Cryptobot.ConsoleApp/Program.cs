using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.TradeStrategies;
using Cryptobot.ConsoleApp.Bybit.Enums;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Utils;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var strat = new StrategyBundle<TS_EveryFiveCandles, BS_OnePercent>();

var request = new HistoryRequest(
    Interval: CandleInterval.Fifteen_Minutes,
    Symbol: Constants.SYMBOL_BTCUSDT,
    MarketCategory: Constants.MARKET_PERPETUAL_FUTURES,
    Strategies: [
        new StrategyBundle<TS_EveryFiveCandles, BS_OnePercent>(),
        new StrategyBundle<TS_EveryCandle, BS_OnePercent>()
        ]);

//await BybitHistory.Download(request);
await Backtester.Run(request);

Console.ReadLine();

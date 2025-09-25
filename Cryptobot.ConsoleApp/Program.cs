using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.Bybit.Enums;
using Cryptobot.ConsoleApp.Utils;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var backtestingDetails = new BacktestingDetails(
    Interval: CandleInterval.Fifteen_Minutes,
    Symbol: Constants.SYMBOL_BTCUSDT,
    MarketCategory: Constants.MARKET_PERPETUAL_FUTURES,
    Strategies: [
        new StrategyBundle<TS_Every100Candles_SL5_TP5, BS_OnePercent>(),
        //new StrategyBundle<TS_Every200Candles_SL5_TP8, BS_OnePercent>(),
        ]);

//await BybitHistory.Download(request);
Backtester.Run(backtestingDetails);

Console.ReadLine();

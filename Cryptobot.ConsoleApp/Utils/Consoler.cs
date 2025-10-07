using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.Bybit;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using System.Text;

namespace Cryptobot.ConsoleApp.Utils;

public static class Consoler
{
    public static async Task Run()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var backtestingDetails = new BacktestingDetails(
            Interval: CandleInterval.Fifteen_Minutes,
            Symbol: Constants.SYMBOL_BTCUSDT,
            MarketCategory: Constants.MARKET_PERPETUAL_FUTURES,
            Strategies: [
                //new StrategyBundle<TS_Every100Candles_SL5_TP5, BS_OnePercent>(),
                //new StrategyBundle<TS_LongWhenMovingAverageIncreases_SL5_TP5, BS_OnePercent>(),
                //new StrategyBundle<TS_LongWhenHigherThan50MAAndNeutralOrBullish_SL3_TP3, BS_100Percent>(),
                new StrategyBundle(VariationSandboxFactory.AllInMA(), new BS_XPercent(100)),
                //new StrategyBundle(VariationSandboxFactory.AllInMA(), new BS_XPercent(20)),
                ]);

        var choiceMade = false;

        while (true)
        {
            Printer.MainMenu(!choiceMade);

            var input = Console.ReadLine();
            Printer.EmptyLine();

            switch (input)
            {
                case "1":
                    await BybitHistory.Download(backtestingDetails);
                    Printer.PressKeyToContinue();
                    choiceMade = true;
                    break;

                case "2":
                    await Backtester.RunBacktest(backtestingDetails);
                    Printer.PressKeyToContinue();
                    choiceMade = true;
                    break;

                case "3":
                    var profilerConfig = new TrendConfiguration(window: TrendConfiguration._default_window);

                    await Backtester.RunTrendProfiler(backtestingDetails, profilerConfig);
                    Printer.PressKeyToContinue();
                    choiceMade = true;
                    break;

                case "4":
                    return;

                default:
                    Printer.InvalidChoice();
                    break;
            }
        }
    }
}

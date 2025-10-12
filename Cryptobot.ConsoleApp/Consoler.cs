using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp;

public static class Consoler
{
    public static async Task Run(CacheService cacheManager)
    {
        var backtestingDetails = new BacktestingDetails(
            Interval: CandleInterval.Fifteen_Minutes,
            Symbol: Constants.SYMBOL_BTCUSDT,
            MarketCategory: Constants.MARKET_PERPETUAL_FUTURES,
            Strategies: [
                new StrategyBundle<TS_Every100Candles_SL5_TP5, BS_1Percent>(),
                //new StrategyBundle<TS_LongWhenHigherThan50MAAndNeutralOrBullish_SL3_TP3, BS_100Percent>(),
                //new StrategyBundle<TS_Simple_120_days_SMA, BS_1Percent>(),
                //new StrategyBundle<VariationSandboxFactory, BS_100Percent>(),
                ]);

        var backtester = new Backtester(cacheManager);
        var choiceMade = false;

        while (true)
        {
            Printer.MainMenu(!choiceMade);

            var input = Console.ReadLine();
            Printer.EmptyLine();

            switch (input)
            {
                case "1":
                    await backtester.RunBacktest(backtestingDetails);
                    Printer.PressKeyToContinue();
                    choiceMade = true;
                    break;

                case "2":
                    while (true)
                    {
                        Printer.TrendProfilerScope();

                        input = Console.ReadLine();
                        Printer.EmptyLine();

                        var scope = input switch
                        {
                            "1" => IndicatorType.MicroTrend,
                            "2" => IndicatorType.SemiTrend,
                            "3" => IndicatorType.MacroTrend,
                            _ => (IndicatorType?)null
                        };

                        if (scope != null)
                        {
                            var profilerConfig = new TrendConfiguration(window: TrendConfiguration._default_window);

                            await backtester.RunTrendProfiler(backtestingDetails, profilerConfig, (IndicatorType)scope);
                            Printer.PressKeyToContinue();

                            break;
                        }
                        else
                        {
                            Printer.InvalidChoice();
                        }
                    }

                    choiceMade = true;
                    break;

                case "3":
                    return;

                default:
                    Printer.InvalidChoice();
                    break;
            }
        }
    }
}

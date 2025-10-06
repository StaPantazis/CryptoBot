using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;
using Cryptobot.ConsoleApp.Bybit;
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
                new StrategyBundle<TS_Every100Candles_SL5_TP5, BS_OnePercent>(),
                ]);

        var choiceMade = false;

        while (true)
        {
            Printer.MainMenu(!choiceMade);

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    await BybitHistory.Download(backtestingDetails);
                    Printer.PressKeyToContinue();
                    choiceMade = true;
                    break;

                case "2":
                    await Backtester.Run(backtestingDetails);
                    Printer.PressKeyToContinue();
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

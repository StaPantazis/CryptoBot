using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Extensions;
using System.Diagnostics;

namespace Cryptobot.ConsoleApp;

public static class Printer
{
    // History
    public static void HistoryDownloadTitle(HistoryRequest historyRequest) => WriteLine($"Downloadig intervals of {(int)historyRequest.Interval} minutes...\n", White);

    public static void CheckingHistory(DateTime day)
    {
        Write("Checking ", White);
        Write($"{day:dd/MM/yyyy}", Blue);
        Write("... ", White);
    }

    public static void AlreadyDownloaded() => WriteLine("Already downloaded!", Yellow);

    public static void Downloading() => Write("Downloading... ", White);

    public static void ValidatingFile() => WriteLine($"Validating file... ", White);

    public static void ValidatingData() => WriteLine($"Validating data... ", White);

    public static void WrongHistory(DateTime day, int mergeCount) => WriteLine($"\nWrong data for file {day}, found {mergeCount} records!", Red);

    public static void WrongHistory(string filepath) => WriteLine($"\nWrong data for file {filepath}!", Red);

    public static void CouldNotDownload() => Write($"Could not download.", Red);

    // Backtesting
    public static void BacktesterStrategies(Spot spot)
    {
        WriteLine("Backtesting...", Blue);

        Write("Trading Strategy: ", White);
        WriteLine(spot.TradeStrategy.Name, Yellow);

        Write("Budgeting Strategy: ", White);
        WriteLine(spot.BudgetStrategy.Name, Yellow);
    }

    public static void BacktesterResult(Spot spot, Stopwatch sw)
    {
        var trades = spot.Trades;

        WriteLine("\n_Result_", Blue);

        Write("Total Trades: ", White);
        WriteLine(trades.Count, Yellow);

        Write("Completed Trades: ", White);
        WriteLine(trades.Where(x => x.IsClosed).Count(), Yellow);

        Write("Incomplete Trades: ", White);
        WriteLine(trades.Where(x => !x.IsClosed).Count(), Yellow);

        Write("Initial Budget: ", White);
        WriteLine(spot.InitialBudget.Euro(), Yellow);

        if (trades.Count > 0)
        {
            Write("Final Budget (without open trades): ", White);

            var openTradesSizesAndFees = trades
                .Where(x => !x.IsClosed)
                .Sum(x => x.TradeFees + x.TradeSize);

            var budgetAfterLastClosed = spot.Budget + openTradesSizesAndFees;

            WriteLine(budgetAfterLastClosed.Euro(), Yellow);

            Write("Total PnL: ", White);

            var totalPnL = (double)budgetAfterLastClosed - spot.InitialBudget;
            WriteLine($"{totalPnL.Euro(plusIfPositive: true)}", totalPnL >= 0 ? Green : Red);
        }


        Write("Runtime: ", White);
        WriteLine(sw.ElapsedMilliseconds.MillisecondsToFormattedTime(), Yellow);
    }

    // General
    public static void Done() => WriteLine("Done!", Green);
    public static void Finished() => WriteLine("Finished!", Green);

    #region Privates
    private static void Write(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = White;
    }

    private static void WriteLine(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = White;
    }

    private static ConsoleColor Yellow => ConsoleColor.Yellow;
    private static ConsoleColor Blue => ConsoleColor.Cyan;
    private static ConsoleColor Green => ConsoleColor.Green;
    private static ConsoleColor White => ConsoleColor.White;
    private static ConsoleColor Red => ConsoleColor.Red;
    #endregion
}

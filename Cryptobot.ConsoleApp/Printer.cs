using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.Metrics;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Extensions;
using System.Diagnostics;

namespace Cryptobot.ConsoleApp;

public static class Printer
{
    // History
    public static void HistoryDownloadTitle(BacktestingDetails details) => WriteLine($"Downloadig intervals of {(int)details.Interval} minutes...\n", White);

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
    public static void BacktesterInitialization(Spot spot)
    {
        WriteLine("__BACKTESTING__", Blue);

        Write("Trading Strategy: ", White);
        WriteLine(spot.TradeStrategy.Name, Yellow);

        Write("Budgeting Strategy: ", White);
        WriteLine(spot.BudgetStrategy.Name, Yellow);
    }

    public static void CalculatingCandles(int candleCount, int totalCandles)
    {
        if (candleCount == 0)
        {
            candleCount = 1;
        }
        else if (candleCount % 1000 != 0)
        {
            return;
        }

        Write($"\rCalculating {candleCount}/{totalCandles}...", Yellow);
    }

    public static void BacktesterResult(Spot spot, Stopwatch sw)
    {
        var metrics = new BacktestMetrics(spot);

        EraseLineContent();
        Write("Backtesting Runtime: ", White);
        WriteLine(sw.ElapsedMilliseconds.MillisecondsToFormattedTime(), Yellow);

        EmptyLine();
        WriteLine("__RESULTS__", Blue);

        Write("Score: ", White);
        WriteLine($"{metrics.StrategyScore.Round(0)}/100", GradeColor(metrics.StrategyGrade));

        Write("Total Trades: ", White);
        WriteLine(metrics.TotalTrades, Yellow);

        Write("Completed Trades: ", White);
        WriteLine(metrics.TotalClosedTrades, Yellow);

        Write("Incomplete Trades: ", White);
        WriteLine(metrics.TotalOpenTrades, Yellow);

        Write("Initial Budget: ", White);
        WriteLine(metrics.InitialBudget.Euro(), Yellow);

        if (metrics.TotalTrades > 0)
        {
            Write("Final Budget (ignore open trades): ", White);
            WriteLine(metrics.BudgetWithoutOpenTrades.Euro(), Yellow);

            Write("Total PnL: ", White);
            WriteLine(metrics.PnL.Euro(plusIfPositive: true), GreenRed(metrics.PnL));

            Write("Costs (already in PnL): ", White);
            Write(metrics.TradeFees.Euro(), Red);
            Write(" (Fees) + ", Yellow);
            Write(metrics.SlippageCosts.Euro(), Red);
            Write(" (Slippage) = ", Yellow);
            WriteLine(metrics.TotalCosts.Euro(), Red);

            EmptyLine();
            IndentedWriteLine("_Performance_", Blue);

            IndentedWrite("Average Win: ", White);
            WriteLine(metrics.AverageWin.Euro(plusIfPositive: true), Green);

            IndentedWrite("Average Loss: ", White);
            WriteLine(metrics.AverageLoss.Euro(), Red);

            IndentedWrite("Initial Budget % profit per trade: ", White);
            WriteLine(metrics.AverageReturnPerTradeToInitialBudget.Percent(digits: 5, plusIfPositive: true), metrics.AverageReturnPerTradeToInitialBudget, GreenRed(metrics.AverageReturnPerTradeToInitialBudget));

            IndentedWrite("Win Rate: ", White);
            WriteLine(metrics.WinRate.Percent(digits: 2), metrics.WinRate, GreenRed(metrics.WinRate));

            IndentedWrite("Payoff Ratio (Win/Loss): ", White);
            WriteLine(metrics.PayoffRatio.Round(2), metrics.PayoffRatio, GreenRed(metrics.PayoffRatio));

            IndentedWrite("Expectancy: ", White);
            WriteLine(metrics.Expectancy.Euro(plusIfPositive: true), metrics.Expectancy, GreenRed(metrics.Expectancy));

            IndentedWrite("Standard Deviation: ", White);
            WriteLine(metrics.StandardDeviation.Round(2), metrics.StandardDeviation, GreenRed(metrics.StandardDeviation));

            IndentedWrite("Sharpe Ratio: ", White);
            WriteLine(metrics.SharpeRatio.Round(2), metrics.SharpeRatio, GreenRed(metrics.SharpeRatio));

            IndentedWrite("Sortino Ratio: ", White);
            WriteLine(metrics.SortinoRatio.Round(2), metrics.SortinoRatio, GreenRed(metrics.SortinoRatio));

            EmptyLine();
            IndentedWriteLine("_Streaks_", Blue);

            IndentedWrite("Average Win Streak: ", White);
            WriteLine(metrics.Streaks.AvgWinStreak.Round(2), Green);

            IndentedWrite("Average Lose Streak: ", White);
            WriteLine(metrics.Streaks.AvgLoseStreak.Round(2), Red);

            IndentedWrite("Longest Win Streak: ", White);
            WriteLine(metrics.Streaks.LongestWinStreak.Value, metrics.Streaks.LongestWinStreak, Green);

            IndentedWrite("Longest Lose Streak: ", White);
            WriteLine(metrics.Streaks.LongestLoseStreak.Value, metrics.Streaks.LongestLoseStreak, Red);

            IndentedWrite("Win Streak Deviation: ", White);
            WriteLine(metrics.Streaks.StdDevWinStreak.Round(2), metrics.Streaks.StdDevWinStreak, Yellow);

            IndentedWrite("Lose Streak Deviation: ", White);
            WriteLine(metrics.Streaks.StdDevLoseStreak.Round(2), metrics.Streaks.StdDevLoseStreak, Yellow);
        }
    }

    public static void BacktesterOutputStart() => WriteLine("\n__STORING__", Blue);

    public static void BacktesterOutputEnd(int candlesCount, Stopwatch sw)
    {
        Write("Total Candles: ", White);
        WriteLine(candlesCount, Yellow);

        Write("Saving Runtime: ", White);
        WriteLine(sw.ElapsedMilliseconds.MillisecondsToFormattedTime(), Yellow);
    }

    public static void BacktesterTotalRuntime(Stopwatch sw)
    {
        Write("Total Runtime: ", White);
        WriteLine(sw.ElapsedMilliseconds.MillisecondsToFormattedTime(), Yellow);
    }

    // General
    public static void Done(string? prefix = null) => WriteLine($"{prefix ?? string.Empty}Done!", Green);
    public static void Finished() => WriteLine("Finished!", Green);
    public static void Divider() => WriteLine("\n-----------------------------------------\n", Red);

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

    private static void WriteLine(object message, Grade grade, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write($"{message} ");
        Console.ForegroundColor = GradeColor(grade);
        Console.WriteLine($"[{grade.GetDisplayName()}]");
        Console.ForegroundColor = White;
    }

    private static void IndentedWrite(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write($"\t{message}");
        Console.ForegroundColor = White;
    }

    private static void IndentedWriteLine(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine($"\t{message}");
        Console.ForegroundColor = White;
    }

    private static ConsoleColor GradeColor(Grade grade)
    {
        return grade switch
        {
            Grade.APlus => ConsoleColor.Green,
            Grade.A => ConsoleColor.DarkGreen,
            Grade.B => ConsoleColor.Cyan,
            Grade.C => ConsoleColor.Yellow,
            Grade.D => ConsoleColor.DarkYellow,
            Grade.E => ConsoleColor.Red,
            Grade.F => ConsoleColor.DarkRed,
            _ => throw new NotImplementedException(),
        };
    }

    private static void EmptyLine() => Console.WriteLine();

    private static void EraseLineContent() => Console.Write("\r                                          \r");

    private static ConsoleColor Yellow => ConsoleColor.Yellow;
    private static ConsoleColor Blue => ConsoleColor.Cyan;
    private static ConsoleColor Green => ConsoleColor.Green;
    private static ConsoleColor White => ConsoleColor.White;
    private static ConsoleColor Red => ConsoleColor.Red;
    private static ConsoleColor DarkGreen => ConsoleColor.DarkGreen;
    private static ConsoleColor DarkYellow => ConsoleColor.DarkYellow;
    private static ConsoleColor DarkRed => ConsoleColor.DarkRed;
    private static ConsoleColor GreenRed(double value, bool positiveIsGreen = true)
        => positiveIsGreen ? (value >= 0 ? Green : Red) : (value <= 0 ? Green : Red);
    #endregion
}

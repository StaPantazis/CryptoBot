using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Backtesting.Metrics;
using Cryptobot.ConsoleApp.Backtesting.OutputModels;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;
using System.Diagnostics;
using static Cryptobot.ConsoleApp.Utils.ConsoleColors;

namespace Cryptobot.ConsoleApp.Utils;

public static class Printer
{
    // Consoler
    public static void MainMenu(bool clear)
    {
        if (clear)
        {
            ClearConsole();
        }
        else
        {
            EmptyLine();
            EmptyLine();
            EmptyLine();
        }

        WriteLine("Choose an option:", Cyan);
        WriteLine("  1) Run backtest", Yellow);
        WriteLine("  2) Run trend profiler", Yellow);
        WriteLine("  3) Exit", Red);
    }

    public static void TrendProfilerScope()
    {
        EmptyLine();
        WriteLine("Which scope do you want:", Cyan);
        WriteLine("  1) Moving Average", Yellow);
        WriteLine("  2) AI Trend", Yellow);
    }

    public static void AiTrendProfileSelection()
    {
        EmptyLine();
        WriteLine("Which AI Trend profile do you want:", Cyan);

        var profiles = Enum.GetValues<AiTrendProfile>().ToArray();

        for (var i = 0; i < profiles.Length; i++)
        {
            WriteLine($"  {i + 1}) {profiles[i]}", Yellow);
        }
    }

    public static void PressKeyToContinue()
    {
        EmptyLine();
        WriteLine("Done! Press any key to continue.", DarkYellow);
        EmptyLine();
        EmptyLine();
        Wait();
    }

    public static void InvalidChoice() => WriteLine("Invalid choice.", Red);

    // History
    public static void HistoryDownloadTitle(BacktestingDetails details) => WriteLine($"Downloading intervals of {(int)details.Interval} minutes...", White);

    public static void CheckingHistory(DateTime dayFrom, DateTime? dayTo = null)
    {
        Write("Checking ", White);

        if (dayTo != null)
        {
            Write("From ", White);
            Write($"{dayFrom:dd/MM/yyyy}", Cyan);
            Write(" - Until ", White);
            Write($"{dayTo:dd/MM/yyyy}", Cyan);
        }
        else
        {
            Write($"{dayFrom:dd/MM/yyyy}", Cyan);
        }

        Write("... ", White);
    }

    public static void AlreadyDownloaded() => WriteLine("Already downloaded!", Yellow);

    public static void AlreadyComputed() => WriteLine("Already computed!", Yellow);

    public static void Downloading() => Write("Downloading... ", White);

    public static void ValidatingFile() => WriteLine($"Validating file... ", White);

    public static void ValidatingData() => WriteLine($"Validating data... ", White);

    public static void WrongHistoryMinute(DateTime day, int mergeCount) => WriteLine($"\nWrong data for file {day}, found {mergeCount} records!", Red);
    public static void WrongHistoryDaily(DateTime batchStart, DateTime batchEnd, Exception ex) => WrongHistoryDaily(batchStart, batchEnd, ex.Message);
    public static void WrongHistoryDaily(DateTime batchStart, DateTime batchEnd, string message)
        => WriteLine($"Failed to fetch data from {batchStart:yyyy-MM-dd} to {batchEnd:yyyy-MM-dd}: {message}", Red);

    public static void WrongHistory(string filepath) => WriteLine($"\nWrong data for file {filepath}!", Red);

    public static void CouldNotDownload() => Write($"Could not download.", Red);

    // Backtesting
    public static void LoadingCandlesStart(BacktestingDetails details)
    {
        Write("Loading data for ", White);
        WriteLine($"{details.IntervalShortString} {details.SymbolDescribed} - {details.MarketCategoryDescribed}", Yellow);
    }

    public static void LoadingCandlesEnd(Stopwatch sw)
    {
        Write("Loading Runtime: ", White);
        WriteLine(sw.ElapsedMilliseconds.MillisecondsToFormattedTime(), Yellow);

        EmptyLine();
    }

    public static void BacktesterInitialization(Spot spot, int? index, int? totalStrategies)
    {
        WriteLine("__BACKTESTING__", Cyan);

        BacktesterStrategyName(spot, index, totalStrategies);

        Write("Budgeting Strategy: ", White);
        WriteLine(spot.BudgetStrategy.Name, Yellow);
    }

    public static void BacktesterStrategyName(Spot spot, int? index, int? totalStrategies)
    {
        Write($"{(index != null ? $"{index}/{totalStrategies}] " : "")}Trading Strategy: ", White);
        WriteLine(spot.TradeStrategy.Name, Yellow);
    }

    public static void CalculatingCandles(int candleCount, int totalCandles)
    {
        if (candleCount == 0)
        {
            candleCount = 1;
        }
        else if (candleCount % 1000 != 0 && candleCount != totalCandles)
        {
            return;
        }

        Write($"\rCalculating {candleCount}/{totalCandles}...", Yellow);
    }

    public static void BacktesterResult(Spot spot, Stopwatch sw, string sectionName)
    {
        var fullMetrics = spot.Metrics.Full;

        EraseLineContent();
        Write("Backtesting Runtime: ", White);
        WriteLine(sw.ElapsedMilliseconds.MillisecondsToFormattedTime(), Yellow);

        EmptyLine();
        WriteLine("__RESULTS__", Cyan);

        if (spot.Trades.Count != 0)
        {
            Write("Score: ", White);
            WriteLine($"{fullMetrics.StrategyScore.Round(0)}/100", GradeColor(fullMetrics.StrategyGrade));

            Write("Initial Budget: ", White);
            WriteLine(spot.InitialBudget.Euro(), Yellow);
        }
        else
        {
            WriteLine("No trades made for this strategy!", Red);
        }

        if (fullMetrics.TotalTrades > 0)
        {
            var longMetrics = spot.Metrics.Long;
            var shortMetrics = spot.Metrics.Short;

            var table = new PrintTable(sectionName: sectionName, ignoreFirstColumnForAlignment: true,
                "_Budgeting_", "Total Trades", "Closed Trades", "Open Trades", $"Open Trades {Constants.STRING_EURO}", "Final Full Budget", $"Final Available Budget", "Total PnL", "Fees", "Slippage", "Total Costs", "Avg Win", "Avg Loss",
                "_Performance_", "Max Drawdown", "Payoff Ratio W/L", "Std Deviation", "Sharpe Ratio", "Sortino Ratio", "Budget % per trade", "Win Rate", "Expectancy",
                "_Streaks_", "Avg Win Streak", "Avg Loss Streak", "Longest Win Streak", "Longest Lose Streak", "Win Streak Deviation", "Lose Streak Deviation");

            var metrics = new List<BacktestMetrics> { fullMetrics, longMetrics, shortMetrics };

            foreach (var metric in metrics)
            {
                if (metric.StrategyGrade is Grade.Ungraded)
                {
                    continue;
                }

                table.AddColumn(
                    ($"{metric.Title} - {metric.StrategyScore.Round(0)}/100", GradeColor(metric.StrategyGrade), null),

                    // Budgeting
                    (metric.TotalTrades, Yellow, null),
                    (metric.TotalClosedTrades, Yellow, null),
                    (metric.TotalOpenTrades, Yellow, null),
                    (metric.OpenTradesAmounts.Euro(), Yellow, null),
                    (metric.FinalFullBudget.Euro(), GreenRed(metric.FinalFullBudget - spot.InitialBudget), null),
                    (metric.FinalAvailableBudget.Euro(), GreenRed(metric.FinalAvailableBudget - spot.InitialBudget), null),
                    (metric.PnL.Euro(plusIfPositive: true), GreenRed(metric.PnL), null),
                    (metric.TradeFees.Euro(), Red, null),
                    (metric.SlippageCosts.Euro(), Red, null),
                    ($"= {metric.TotalCosts.Euro()}", Red, null),
                    (metric.AverageWin.Euro(plusIfPositive: true), Green, null),
                    (metric.AverageLoss.Euro(), Red, null),

                    // Performance
                    (metric.MaximumDrawdown.Percent(2), Yellow, metric.MaximumDrawdown.Grade),
                    (metric.PayoffRatio.Round(2), Yellow, metric.PayoffRatio.Grade),
                    (metric.StandardDeviation.Round(2), Yellow, metric.StandardDeviation.Grade),
                    (metric.SharpeRatio.Round(2).ToString("F2"), Yellow, metric.SharpeRatio.Grade),
                    (metric.SortinoRatio.Round(2).ToString("F2"), Yellow, metric.SortinoRatio.Grade),
                    (metric.AverageReturnPerTradeToInitialBudget.Percent(digits: 5, plusIfPositive: true), GreenRed(metric.AverageReturnPerTradeToInitialBudget), metric.AverageReturnPerTradeToInitialBudget.Grade),
                    (metric.WinRate.Percent(digits: 2, isZeroToOne: true), Yellow, metric.WinRate.Grade),
                    (metric.Expectancy.Euro(plusIfPositive: true), GreenRed(metric.Expectancy), metric.Expectancy.Grade),

                    // Streaks
                    (metric.Streaks.AvgWinStreak.Round(2).ToString("F2"), Green, null),
                    (metric.Streaks.AvgLoseStreak.Round(2).ToString("F2"), Red, null),
                    (metric.Streaks.LongestWinStreak.Value, Green, metric.Streaks.LongestWinStreak.Grade),
                    (metric.Streaks.LongestLoseStreak.Value, Red, metric.Streaks.LongestLoseStreak.Grade),
                    (metric.Streaks.StdDevWinStreak.Round(2).ToString("F2"), Yellow, metric.Streaks.StdDevWinStreak.Grade),
                    (metric.Streaks.StdDevLoseStreak.Round(2).ToString("F2"), Yellow, metric.Streaks.StdDevLoseStreak.Grade));
            }

            EmptyLine();
            table.Print();
        }
    }

    public static void ShouldSaveQuestion()
    {
        EmptyLine();
        Write("Do you want to save this backtest? Y/N ", Cyan);
        Write("('csv' or 'both')", Yellow);
    }

    public static void SavingOutputStart() => WriteLine("\n__STORING__", Cyan);

    public static void SavingOutputEnd(int candlesCount, Stopwatch sw)
    {
        Write("Total Candles: ", White);
        WriteLine(candlesCount, Yellow);

        Write("Saving Runtime: ", White);
        WriteLine(sw.ElapsedMilliseconds.MillisecondsToFormattedTime(), Yellow);
    }

    public static void ExportingCsvStart() => Write("\nExporting csv... ", Cyan);

    public static void SavedOutputFileName(string fileName)
    {
        Write("Saved file: ", White);
        WriteLine(Path.GetFileName(fileName), DarkYellow);
    }

    public static void TotalRuntime(Stopwatch sw)
    {
        Write("Total Runtime: ", White);
        WriteLine(sw.ElapsedMilliseconds.MillisecondsToFormattedTime(), Yellow);
    }

    public static void ProfilerInitialization() => WriteLine("Running Trend Profiler...", Cyan);

    public static void ProfilerRunEnded(List<TrendCandle> candles, Stopwatch sw)
    {
        EraseLineContent();
        Write("Profiling Runtime: ", White);
        WriteLine(sw.ElapsedMilliseconds.MillisecondsToFormattedTime(), Yellow);

        EmptyLine();

        WriteLine($"Full Bear: {candles.Count(x => x.Trend is Trend.FullBear)}", DarkRed);
        WriteLine($"Bear: {candles.Count(x => x.Trend is Trend.Bear)}", DarkRed);
        WriteLine($"Neutral: {candles.Count(x => x.Trend is Trend.Neutral)}", White);
        WriteLine($"Bull: {candles.Count(x => x.Trend is Trend.Bull)}", Green);
        WriteLine($"Full Bull: {candles.Count(x => x.Trend is Trend.FullBull)}", DarkGreen);

        EmptyLine();
    }

    public static void FullStrategyPnL(Spot spot) => GreenRed(spot.Metrics.Full.PnL);

    // General
    public static void Done(string? prefix = null) => WriteLine($"{prefix ?? string.Empty}Done!", Green);
    public static void Finished()
    {
        WriteLine("Finished!", Green);
        EmptyLine();
    }

    public static void Divider() => WriteLine("\n-----------------------------------------\n", Red);

    #region Functionality
    public static void EmptyLine() => Console.WriteLine();

    public static void Write(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = White;
    }

    public static void WriteLine(object message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = White;
    }

    private static void EraseLineContent() => Console.Write("\r                                          \r");

    private static void Write(object message, Grade grade, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write($"{message}");
        Console.ForegroundColor = GradeColor(grade);
        Console.Write($" {grade.GetDisplayName()}");
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
            Grade.Ungraded => ConsoleColor.DarkRed,
            _ => throw new NotImplementedException(),
        };
    }

    private static void ClearConsole() => Console.Clear();

    private static void Wait() => Console.ReadLine();

    public static ConsoleColor GreenRed(double value, bool positiveIsGreen = true)
        => positiveIsGreen ? value is >= 0 or double.NaN ? Green : Red : value <= 0 ? Green : Red;
    #endregion

    private class PrintTable
    {
        private readonly List<int> _breakIndexes = [];
        private readonly List<int> _titleIndexes = [];
        private readonly bool _ignoreFirstColumnForAlignment;
        private readonly List<PrintColumn> Columns = [];

        public PrintTable(string sectionName, bool ignoreFirstColumnForAlignment, params string[] firstColumn)
        {
            _ignoreFirstColumnForAlignment = ignoreFirstColumnForAlignment;
            var rows = new List<PrintCell>() {
                string.IsNullOrEmpty(sectionName) ? PrintCell.Empty : new PrintCell(sectionName, White)
            };

            foreach (var row in firstColumn)
            {
                if (row.StartsWith('_'))
                {
                    rows.Add(new(row.ToUpper(), Cyan));
                }
                else
                {
                    rows.Add(new(row, Yellow));
                }
            }

            Columns.Add(new PrintColumn(rows));

            _breakIndexes = rows
                .Select((x, i) => (x.IsEmpty() || x.Text.Contains('_')) ? (int?)i : null)
                .Where(x => x is not null and > 0)
                .Cast<int>()
                .ToList();

            var titlesMet = 1;

            _titleIndexes = rows
                .Select((x, i) =>
                {
                    var index = x.Text.Contains('_') ? (int?)i + titlesMet : null;
                    titlesMet += index != null ? 1 : 0;
                    return index;
                })
                .Where(x => x is not null)
                .Cast<int>()
                .ToList();
        }

        public void AddColumn(params (object col, ConsoleColor colColor, Grade? grade)[] column)
        {
            var columnCells = column.Select(x => new PrintCell(x.col.ToString()!, x.colColor, x.grade)).ToList();
            _breakIndexes.ForEach(i => columnCells.Insert(i, PrintCell.Empty));

            Columns.Add(new(columnCells));
        }

        public void Print()
        {
            var columnsToAlign = _ignoreFirstColumnForAlignment ? Columns.Skip(1).ToList() : Columns;
            var maxColumnWidth = Math.Max(10, columnsToAlign.Max(x => x.MaxWidth));
            var firstColumnWidth = _ignoreFirstColumnForAlignment ? Columns[0].MaxWidth : maxColumnWidth;

            AddBorderRow(0, Border.TOP_LEFT, Border.TOP_T, Border.TOP_RIGHT);
            _titleIndexes.ForEach(i => AddBorderRow(i, i == _titleIndexes.Min() ? Border.TOP_LEFT : Border.LEFT_T, Border.CROSS, Border.RIGHT_T));
            AddBorderRow(Columns[0].Cells.Count, Border.BOTTOM_LEFT, Border.BOTTOM_T, Border.BOTTOM_RIGHT);

            if (_ignoreFirstColumnForAlignment)
            {
                Columns[0].Format(firstColumnWidth, true);
            }

            columnsToAlign
                .Select((column, i) => new { column, i })
                .ForEachDo(x => x.column.Format(maxColumnWidth, columnsToAlign.Count == Columns.Count && x.i == 0));

            Enumerable.Range(0, Columns[0].Cells.Count)
                .Select(i => new PrintRow(Columns.Select(x => x.Cells[i]).ToList()))
                .ToList()
                .ForEach(x => x.Print());
        }

        private void AddBorderRow(int cellIndex, char start, char middle, char end)
        {
            Columns[0].Cells.Insert(cellIndex, cellIndex == 0 ? PrintCell.TopLeftBorder() : PrintCell.Border(start));

            for (var i = 1; i < Columns.Count - 1; i++)
            {
                Columns[i].Cells.Insert(cellIndex == 1 ? 2 : cellIndex, PrintCell.Border(middle));
            }

            Columns.Last().Cells.Insert(cellIndex, PrintCell.Border(end));
        }
    }

    private class PrintCell(string text, ConsoleColor color, Grade? grade = null, bool isBorder = false)
    {
        public string Text { get; set; } = isBorder ? text : $"{text}{(text.Contains(Constants.STRING_EURO) || text.Contains(Constants.STRING_PERCENT) ? "" : "  ")}";
        public ConsoleColor Color { get; } = color;
        public Grade? Grade { get; } = grade;
        public bool IsBorder { get; } = isBorder;
        public bool IsTopLeftBorder { get; } = isBorder && text == "top_left";
        public string PaddingLeft { get; set; }
        public string PaddingRight { get; set; }
        public string FullText => $"{Text} {(Grade is null ? string.Empty : $"[{Grade.GetDisplayName()}]")}";

        public bool IsEmpty() => string.IsNullOrWhiteSpace(Text);

        public static PrintCell Empty => new(string.Empty, default);
        public static PrintCell Border(char border) => new(border.ToString(), White, isBorder: true);
        public static PrintCell TopLeftBorder() => new("top_left", White, isBorder: true);

        public override string ToString() => $"|{PaddingLeft}{FullText}{PaddingRight}|";
    }

    private record PrintColumn
    {
        public int MaxWidth { get; }
        public List<PrintCell> Cells { get; }

        public PrintColumn(List<PrintCell> cells)
        {
            Cells = cells;
            MaxWidth = cells.Select(x => x.FullText.Length).Max();
        }

        public void Format(int maxWidth, bool isHeaderColumn)
        {
            maxWidth += isHeaderColumn ? 1 : 3;

            for (var i = 0; i < Cells.Count; i++)
            {
                var cell = Cells[i];

                if (cell.IsBorder)
                {
                    cell.Text = cell.Text[0] switch
                    {
                        _ when cell.IsTopLeftBorder => $"{Pad(maxWidth)}{Border.TOP_LEFT}",
                        Border.TOP_LEFT => $"{cell.Text}{Repeat(Border.HORIZONTAL, maxWidth - 1)}{Border.CROSS}",
                        Border.TOP_T => $"{Repeat(Border.HORIZONTAL, maxWidth - 1)}{Border.TOP_T}",
                        Border.TOP_RIGHT => $"{Repeat(Border.HORIZONTAL, maxWidth)}{cell.Text}",

                        Border.LEFT_T => $"{cell.Text}{Repeat(Border.HORIZONTAL, maxWidth - 1)}{Border.CROSS}",
                        Border.CROSS => $"{Repeat(Border.HORIZONTAL, maxWidth - 1)}{Border.CROSS}",
                        Border.RIGHT_T => $"{Repeat(Border.HORIZONTAL, maxWidth)}{cell.Text}",

                        Border.BOTTOM_LEFT => $"{cell.Text}{Repeat(Border.HORIZONTAL, maxWidth - 1)}{Border.BOTTOM_T}",
                        Border.BOTTOM_T => $"{Repeat(Border.HORIZONTAL, maxWidth - 1)}{Border.BOTTOM_T}",
                        Border.BOTTOM_RIGHT => $"{Repeat(Border.HORIZONTAL, maxWidth)}{cell.Text}",
                        _ => throw new NotImplementedException()
                    };

                    continue;
                }

                var padding = maxWidth - cell.FullText.Length + (cell.Grade is null ? 0 : 1);
                var paddingGrade = cell.Grade is null ? 6 : 1;

                if (i == 1)
                {
                    var paddingRight = padding / 2;
                    var leftPadding = padding - paddingRight;

                    cell.PaddingRight = Pad(paddingRight);
                    cell.PaddingLeft = Pad(leftPadding);
                }
                else if (isHeaderColumn)
                {
                    cell.PaddingRight = Pad(padding - 1);
                    cell.PaddingLeft = Pad(1);
                }
                else
                {
                    cell.PaddingRight = Pad(paddingGrade);
                    cell.PaddingLeft = Pad(padding - paddingGrade);
                }
            }
        }

        private static string Pad(int padding) => new(' ', Math.Max(0, padding));
        private static string Repeat(char character, int times) => new(character, Math.Max(0, times));
    };

    private record PrintRow(params List<PrintCell> Cells)
    {
        public void Print()
        {
            if (Cells[0].IsBorder)
            {
                foreach (var cell in Cells)
                {
                    Write(cell.Text, White);
                }

                EmptyLine();
                return;
            }

            for (var i = 0; i < Cells.Count; i++)
            {
                var cell = Cells[i];

                Write($"{(i == 0 && Cells.Any(x => x.Text.Contains(PositionSide.Short.ToString())) ? " " : Border.VERTICAL)}{cell.PaddingLeft}", White);

                if (cell.Grade != null)
                {
                    Write(cell.Text, (Grade)cell.Grade, cell.Color);
                }
                else
                {
                    Write(cell.Text, cell.Color);
                }

                Write($"{cell.PaddingRight}", White);
            }

            WriteLine($" {Border.VERTICAL}", White);
        }
    }

    private class Border
    {
        public const char VERTICAL = '│';
        public const char HORIZONTAL = '─';
        public const char CROSS = '┼';

        public const char TOP_LEFT = '┌';
        public const char TOP_RIGHT = '┐';
        public const char BOTTOM_RIGHT = '┘';
        public const char BOTTOM_LEFT = '└';

        public const char TOP_T = '┬';
        public const char BOTTOM_T = '┴';
        public const char LEFT_T = '├';
        public const char RIGHT_T = '┤';
    }
}

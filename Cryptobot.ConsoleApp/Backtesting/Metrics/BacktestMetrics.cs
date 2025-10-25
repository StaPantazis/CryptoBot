using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.Extensions;

namespace Cryptobot.ConsoleApp.Backtesting.Metrics;

public class BacktestMetrics
{
    public string Title { get; }

    public BacktestMetrics(IReadOnlyList<Trade> trades, Spot spot, string title)
    {
        Title = title;

        if (trades.Count == 0)
        {
            return;
        }

        var openTrades = trades.Where(x => !x.IsClosed).ToArray();
        var closedTrades = trades.Where(x => x.IsClosed).ToArray();

        TotalTrades = trades.Count;
        TotalOpenTrades = openTrades.Length;
        OpenTradesAmounts = openTrades.Sum(x => x.TradeSize);
        TotalClosedTrades = closedTrades.Length;
        InitialBudget = spot.InitialBudget;

        PnL = closedTrades.Sum(x => x.PnL!.Value);
        FinalFullBudget = spot.InitialBudget + PnL;
        FinalAvailableBudget = spot.InitialBudget + PnL + OpenTradesAmounts;
        AverageReturnPerTradeToInitialBudget = TotalClosedTrades > 0 ? PnL / (InitialBudget * TotalClosedTrades) * 100 : 0;

        TradeFees = closedTrades.Sum(x => x.TradeFees);
        SlippageCosts = closedTrades.Sum(x => x.SlippageCosts);
        TotalCosts = closedTrades.Sum(x => x.SlippageCosts + x.TradeFees);

        if (closedTrades.Length != 0)
        {
            var pnls = closedTrades.Select(t => t.PnL!.Value).ToArray();
            var winners = pnls.Where(x => x > 0).ToArray();
            var losers = pnls.Where(x => x <= 0).ToArray();

            WinRate = winners.Length / (double)TotalClosedTrades;
            AverageWin = winners.Length != 0 ? winners.Average() : 0;
            AverageLoss = losers.Length != 0 ? losers.Average() : 0;
            PayoffRatio = AverageLoss != 0 ? AverageWin / Math.Abs(AverageLoss) : double.NaN;
            Expectancy = (WinRate * AverageWin) + ((1 - WinRate) * AverageLoss);

            var meanPnL = pnls.Average();
            StandardDeviation = Math.Sqrt(pnls.Average(x => Math.Pow(x - meanPnL, 2)));
            SharpeRatio = StandardDeviation > 0 ? meanPnL / StandardDeviation : double.NaN;

            var downside = pnls.Where(x => x < meanPnL).ToArray();
            var downsideDev = downside.Length != 0 ? Math.Sqrt(downside.Average(x => Math.Pow(x - meanPnL, 2))) : 0;
            SortinoRatio = downsideDev > 0 ? meanPnL / downsideDev : double.NaN;

            CalculateMaxDrawdown(closedTrades, InitialBudget);

            Streaks = new StreaksModel(closedTrades);
        }

        this.GradeMetrics();
        (StrategyScore, StrategyGrade) = StrategyScorer.Compute(this);
    }

    public double StrategyScore { get; }
    public Grade StrategyGrade { get; }

    public int TotalTrades { get; }
    public int TotalClosedTrades { get; }
    public int TotalOpenTrades { get; }
    public double OpenTradesAmounts { get; }
    public double InitialBudget { get; }

    public double FinalFullBudget { get; }
    public double FinalAvailableBudget { get; }
    public double PnL { get; }                // = FinalFullBudget - InitialBudget
    public double RealizedPnLSum { get; }     // sum of closed-trade PnL (diagnostic)

    public GradedMetric<double> AverageReturnPerTradeToInitialBudget { get; }

    public double TradeFees { get; }
    public double SlippageCosts { get; }
    public double TotalCosts { get; }

    public GradedMetric<double> WinRate { get; }
    public double AverageWin { get; }
    public double AverageLoss { get; }
    public GradedMetric<double> MaximumDrawdown { get; private set; }
    public GradedMetric<double> PayoffRatio { get; }
    public GradedMetric<double> Expectancy { get; }
    public GradedMetric<double> StandardDeviation { get; }
    public GradedMetric<double> SharpeRatio { get; }
    public GradedMetric<double> SortinoRatio { get; }
    public StreaksModel Streaks { get; }

    private void CalculateMaxDrawdown(Trade[] trades, double initialBudget)
    {
        if (trades == null || trades.Length == 0)
        {
            MaximumDrawdown = 0;
            return;
        }

        var events = new List<(DateTime time, double equity, int order)>(trades.Length * 2);

        // Seed baseline just before first trade
        var firstTime = trades.Min(t => t.EntryTime);
        events.Add((firstTime.AddTicks(-1), initialBudget, -1));

        foreach (var t in trades)
        {
            // Entry: total equity (cash + open) at the moment of entering
            events.Add((t.EntryTime, t.FullBudgetOnEntry, 0));

            // Exit: realized total equity
            if (t.IsClosed && t.ExitTime.HasValue && t.FullBudgetAfterExit.HasValue)
            {
                events.Add((t.ExitTime.Value, t.FullBudgetAfterExit.Value, 1));
            }
        }

        var ordered = events
            .OrderBy(e => e.time)
            .ThenBy(e => e.order)   // baseline (-1), then entry (0), then exit (1)
            .ToList();

        var peak = ordered[0].equity;
        var maxDrawdown = 0.0;

        foreach (var (time, equity, order) in ordered)
        {
            if (equity > peak)
            {
                peak = equity;
            }

            if (peak > 0)
            {
                var dd = (peak - equity) / peak;
                if (dd > maxDrawdown)
                {
                    maxDrawdown = dd;
                }
            }
        }

        MaximumDrawdown = maxDrawdown * 100.0;
    }

    public class StreaksModel
    {
        public StreaksModel(Trade[] closedTrades)
        {
            var closed = closedTrades
                .OrderBy(t => t.ExitTime!.Value)
                    .ThenBy(t => t.EntryTime)
                .ToArray();

            if (closed.Length == 0)
            {
                return;
            }

            var winStreaks = new List<int>();
            var loseStreaks = new List<int>();

            var currentStreak = 0;
            bool? currentResultType = null; // true = win, false = loss

            foreach (var t in closed)
            {
                var isWin = t.PnL!.Value >= 0;

                if (currentResultType == null || isWin != currentResultType)
                {
                    // Flush previous streak
                    if (currentStreak > 0)
                    {
                        if (currentResultType is true)
                        {
                            winStreaks.Add(currentStreak);
                        }

                        if (currentResultType is false)
                        {
                            loseStreaks.Add(currentStreak);
                        }
                    }

                    currentStreak = 1;
                    currentResultType = isWin;
                }
                else
                {
                    currentStreak++;
                }
            }

            // Flush tail streak
            if (currentResultType is true)
            {
                winStreaks.Add(currentStreak);
            }
            else if (currentResultType is false)
            {
                loseStreaks.Add(currentStreak);
            }

            LongestWinStreak = winStreaks.DefaultIfEmpty(0).Max();
            LongestLoseStreak = loseStreaks.DefaultIfEmpty(0).Max();
            AvgWinStreak = winStreaks.Count > 0 ? winStreaks.Average() : 0;
            AvgLoseStreak = loseStreaks.Count > 0 ? loseStreaks.Average() : 0;
            StdDevWinStreak = winStreaks.StdDev();
            StdDevLoseStreak = loseStreaks.StdDev();
        }

        public GradedMetric<int> LongestWinStreak { get; init; }
        public GradedMetric<int> LongestLoseStreak { get; init; }
        public double AvgWinStreak { get; init; }
        public double AvgLoseStreak { get; init; }
        public GradedMetric<double> StdDevWinStreak { get; init; }
        public GradedMetric<double> StdDevLoseStreak { get; init; }
    }
}

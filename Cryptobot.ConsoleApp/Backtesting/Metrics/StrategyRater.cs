namespace Cryptobot.ConsoleApp.Backtesting.Metrics;

public static class StrategyScorer
{
    // Weights must sum to 1.0
    private const double _weight_AVGRETURN = 0.14;
    private const double _weight_WINRATE = 0.10;
    private const double _weight_PAYOFF = 0.12;
    private const double _weight_EXPECT = 0.16;
    private const double _weight_SHARPE = 0.16;
    private const double _weight_SORTINO = 0.10;
    private const double _weight_STDDEV = 0.08;
    private const double _weight_LONGWIN = 0.04;
    private const double _weight_LONGLOSE = 0.06;
    private const double _weight_DEVWIN = 0.02;
    private const double _weight_DEVLOSE = 0.02;
    private static readonly double[] _bandScores = [0, 30, 50, 65, 80, 90, 100];

    public static (double score, Grade grade) Compute(BacktestMetrics m)
    {
        // Pull raw values (not the letter grades) and map to 0..100 subscores.
        // NOTE: WinRate.Value is expected as 0..1 fraction (not %).
        var sAvgReturn = Score_AverageReturnPct(m.AverageReturnPerTradeToInitialBudget.Value);
        var sWinRate = Score_WinRate(m.WinRate.Value);
        var sPayoff = Score_Payoff(m.PayoffRatio.Value);
        var sExpect = Score_Expectancy(m.Expectancy.Value);

        var sSharpe = Score_Sharpe(m.SharpeRatio.Value);
        var sSortino = Score_Sortino(m.SortinoRatio.Value);
        var sStdDev = Score_PnLStdDev(m.StandardDeviation.Value);  // lower is better

        var sLongWin = Score_LongestWinStreak(m.Streaks.LongestWinStreak.Value);
        var sLongLose = Score_LongestLoseStreak(m.Streaks.LongestLoseStreak.Value); // lower is better
        var sDevWin = Score_StreakDeviation(m.Streaks.StdDevWinStreak.Value);     // lower is better
        var sDevLose = Score_StreakDeviation(m.Streaks.StdDevLoseStreak.Value);    // lower is better

        var score = (_weight_AVGRETURN * sAvgReturn)
            + (_weight_WINRATE * sWinRate)
            + (_weight_PAYOFF * sPayoff)
            + (_weight_EXPECT * sExpect)
            + (_weight_SHARPE * sSharpe)
            + (_weight_SORTINO * sSortino)
            + (_weight_STDDEV * sStdDev)
            + (_weight_LONGWIN * sLongWin)
            + (_weight_LONGLOSE * sLongLose)
            + (_weight_DEVWIN * sDevWin)
            + (_weight_DEVLOSE * sDevLose);

        var overall = Math.Round(score, 2);
        var letter = GradeFromScore(overall);
        return (overall, letter);
    }

    private static Grade GradeFromScore(double score) => score switch
    {
        >= 95 => Grade.APlus,
        >= 90 => Grade.A,
        >= 80 => Grade.B,
        >= 70 => Grade.C,
        >= 60 => Grade.D,
        >= 40 => Grade.E,
        _ => Grade.F
    };

    // ---------- Scoring functions (0..100) ----------
    private static double Score_AverageReturnPct(double vPct) => PiecewiseHigher(vPct, [-0.05, -0.01, 0.00, 0.02, 0.05, 0.10]); // % per trade (already *100 in your metric)

    private static double Score_WinRate(double vFr) => PiecewiseHigher(vFr, [0.15, 0.25, 0.35, 0.45, 0.55, 0.65]); // 0..1

    private static double Score_Payoff(double v) => PiecewiseHigher(v, [0.7, 0.9, 1.0, 1.2, 1.5, 2.0]);

    private static double Score_Expectancy(double eur) => PiecewiseHigher(eur, [-1.0, -0.5, 0.0, 0.5, 1.0, 2.0]);

    private static double Score_Sharpe(double v) => PiecewiseHigher(v, [-0.5, 0.0, 0.5, 1.0, 1.5, 2.0]);

    private static double Score_Sortino(double v) => PiecewiseHigher(v, [0.0, 0.5, 1.0, 1.5, 2.0, 3.0]);

    // Lower is better
    private static double Score_PnLStdDev(double eur) => PiecewiseLower(eur, [12.0, 8.0, 5.0, 3.0, 2.0, 1.0]);

    private static double Score_LongestWinStreak(int n) => PiecewiseHigher(n, [0.0, 5.0, 10.0, 20.0, 30.0, 50.0]);

    private static double Score_LongestLoseStreak(int n) => PiecewiseLower(n, [50.0, 30.0, 20.0, 10.0, 5.0, 3.0]);

    private static double Score_StreakDeviation(double v) => PiecewiseLower(v, [12.0, 8.0, 5.0, 3.0, 2.0, 1.0]);

    // ---------- Generic piecewise mappers ----------
    // Thresholds are 6 cut points for the 7 grade bands (F..A+).
    // We map linearly within bands to scores {0,30,50,65,80,90,100}.
    private static double PiecewiseHigher(double v, double[] t) => InterpBands(v, t, higherIsBetter: true); // t ascending

    private static double PiecewiseLower(double v, double[] t) => InterpBands(v, t, higherIsBetter: false); // t descending quality (i.e., larger is worse)

    private static double InterpBands(double v, double[] t, bool higherIsBetter)
    {
        // t has 6 thresholds that split the 7 bands.
        // For "higherIsBetter", bands are:
        // [ -inf .. t0 ), [t0..t1), [t1..t2), [t2..t3), [t3..t4), [t4..t5), [t5.. +inf )
        // For "lowerIsBetter", we just flip interpretation (so smaller value maps to higher score).
        if (double.IsNaN(v) || double.IsInfinity(v))
        {
            return 50; // neutral if undefined
        }

        if (higherIsBetter)
        {
            if (v < t[0])
            {
                return _bandScores[0];
            }
            else if (v >= t[^1])
            {
                return _bandScores[^1];
            }

            for (var i = 0; i < t.Length - 1; i++)
            {
                if (v >= t[i] && v < t[i + 1])
                {
                    return Lerp(t[i], _bandScores[i + 1 - 1], t[i + 1], _bandScores[i + 1], v); // between scores[i] and scores[i+1]
                }
            }

            // between t[4]..t[5]
            return Lerp(t[^2], _bandScores[^2], t[^1], _bandScores[^1], v);
        }
        else
        {
            // lower is better → invert thresholds by checking from best band downwards
            if (v <= t[^1])
            {
                return _bandScores[^1]; // <= best (A+)
            }
            else if (v > t[0])
            {
                return _bandScores[0]; // worse than worst cutoff (F)
            }

            for (var i = t.Length - 1; i > 0; i--)
            {
                if (v > t[i - 1] && v <= t[i])
                {
                    return Lerp(t[i], _bandScores[t.Length - i], t[i - 1], _bandScores[t.Length - i + 1], v);
                }
            }
            return _bandScores[0];
        }
    }

    private static double Lerp(double x0, double y0, double x1, double y1, double x)
    {
        if (Math.Abs(x1 - x0) < 1e-12)
        {
            return (y0 + y1) * 0.5;
        }

        var t = (x - x0) / (x1 - x0);
        return y0 + (t * (y1 - y0));
    }
}

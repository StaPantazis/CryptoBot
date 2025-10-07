namespace Cryptobot.ConsoleApp.Backtesting.Metrics;

public static class MetricGrader
{
    public static void GradeMetrics(this BacktestMetrics metrics)
    {
        GradeAverageReturnPerTradeToInitialBudget(metrics.AverageReturnPerTradeToInitialBudget);
        GradeWinRate(metrics.WinRate);
        GradePayoffRatio(metrics.PayoffRatio);
        GradeExpectancy(metrics.Expectancy);
        GradeStandardDeviation(metrics.StandardDeviation);
        GradeSharpeRatio(metrics.SharpeRatio);
        GradeSortinoRatio(metrics.SortinoRatio);
        GradeMaximumDrawdown(metrics.MaximumDrawdown);

        GradeWinStreak(metrics.Streaks.LongestWinStreak);
        GradeLoseStreak(metrics.Streaks.LongestLoseStreak);
        GradeStreakDeviation(metrics.Streaks.StdDevWinStreak);
        GradeStreakDeviation(metrics.Streaks.StdDevLoseStreak);
    }

    // Profitability & Efficiency
    private static Grade GradeAverageReturnPerTradeToInitialBudget<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        > 0.10 => Grade.APlus,
        > 0.05 => Grade.A,
        > 0.02 => Grade.B,
        > 0.00 => Grade.C,
        > -0.01 => Grade.D,
        > -0.05 => Grade.E,
        _ => Grade.F
    };

    private static Grade GradeWinRate<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        > 0.65 => Grade.APlus,
        > 0.55 => Grade.A,
        > 0.45 => Grade.B,
        > 0.35 => Grade.C,
        > 0.25 => Grade.D,
        > 0.15 => Grade.E,
        _ => Grade.F
    };

    private static Grade GradePayoffRatio<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        > 2.0 or _ when metric.Value is double val && double.IsNaN(val) => Grade.APlus,
        > 1.5 => Grade.A,
        > 1.2 => Grade.B,
        > 1.0 => Grade.C,
        > 0.9 => Grade.D,
        > 0.7 => Grade.E,
        _ => Grade.F
    };

    private static Grade GradeExpectancy<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        > 2.0 => Grade.APlus,
        > 1.0 => Grade.A,
        > 0.5 => Grade.B,
        > 0.0 => Grade.C,
        > -0.5 => Grade.D,
        > -1.0 => Grade.E,
        _ => Grade.F
    };

    public static Grade GradeStandardDeviation<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        <= 1.0 => Grade.APlus,
        <= 2.0 => Grade.A,
        <= 3.0 => Grade.B,
        <= 5.0 => Grade.C,
        <= 8.0 => Grade.D,
        <= 12.0 => Grade.E,
        _ => Grade.F
    };

    // Risk Adjusted
    private static Grade GradeSharpeRatio<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        > 2.0 => Grade.APlus,
        > 1.5 => Grade.A,
        > 1.0 => Grade.B,
        > 0.5 => Grade.C,
        > 0.0 => Grade.D,
        > -0.5 => Grade.E,
        _ => Grade.F
    };

    private static Grade GradeSortinoRatio<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        > 3.0 => Grade.APlus,
        > 2.0 => Grade.A,
        > 1.5 => Grade.B,
        > 1.0 => Grade.C,
        > 0.5 => Grade.D,
        > 0.0 => Grade.E,
        _ => Grade.F
    };

    private static Grade GradeMaximumDrawdown<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        <= 5.0 => Grade.APlus,
        <= 10.0 => Grade.A,
        <= 15.0 => Grade.B,
        <= 20.0 => Grade.C,
        <= 30.0 => Grade.D,
        <= 40.0 => Grade.E,
        _ => Grade.F
    };

    // Streaks (penalizing long losing streaks, rewarding stability)
    private static Grade GradeWinStreak<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        > 50 => Grade.APlus,
        > 30 => Grade.A,
        > 20 => Grade.B,
        > 10 => Grade.C,
        > 5 => Grade.D,
        > 0 => Grade.E,
        _ => Grade.F
    };

    private static Grade GradeLoseStreak<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        <= 3 => Grade.APlus,
        <= 5 => Grade.A,
        <= 10 => Grade.B,
        <= 20 => Grade.C,
        <= 30 => Grade.D,
        <= 50 => Grade.E,
        _ => Grade.F
    };

    private static Grade GradeStreakDeviation<T>(GradedMetric<T> metric) => metric.Grade = metric.Value switch
    {
        <= 1.0 => Grade.APlus,
        <= 2.0 => Grade.A,
        <= 3.0 => Grade.B,
        <= 5.0 => Grade.C,
        <= 8.0 => Grade.D,
        <= 12.0 => Grade.E,
        _ => Grade.F
    };
}

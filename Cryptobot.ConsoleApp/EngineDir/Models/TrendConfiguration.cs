namespace Cryptobot.ConsoleApp.EngineDir.Models;

/// <summary>
/// Configuration parameters for TrendProfiler.
/// Adjust these knobs to fine-tune how trends are judged.
/// </summary>
public class TrendConfiguration(int window)
{
    public const int _default_window = 30;

    /// <summary>
    /// Neutral band around zero score.
    /// If |score| < NeutralBand → treated as Neutral/Sideways.
    /// Range: [0.0, 1.0].
    /// Default: 0.10 (10%).
    /// Larger = more signals classified as Neutral.
    /// Smaller = more signals classified as Bull/Bear.
    /// </summary>
    public double NeutralBand { get; set; } = 0.10;

    /// <summary>
    /// Minimum R² required for a trend to be trusted.
    /// If R² < MinR2ForTrend → treated as Neutral regardless of slope.
    /// Range: [0.0, 1.0].
    /// Default: 0.20 (20%).
    /// Higher = require stronger regression fit, fewer Bull/Bear signals.
    /// Lower = allow noisier data to still count as trend.
    /// </summary>
    public double MinR2ForTrend { get; set; } = 0.20;

    /// <summary>
    /// Threshold for Bear classification (mild).
    /// Score ≤ -ThresholdBear → Bear.
    /// Range: [0.0, 1.0].
    /// Default: 0.15.
    /// Lower = easier to classify as Bear.
    /// Higher = stricter, more candles end up Neutral instead of Bear.
    /// </summary>
    public double ThresholdBear { get; set; } = 0.15;

    /// <summary>
    /// Threshold for FullBear / FullBull (strong trend).
    /// Score ≤ -ThresholdBull → FullBear.
    /// Score ≥ ThresholdBull → FullBull.
    /// Range: [0.0, 1.0].
    /// Default: 0.55.
    /// Lower = more aggressive "full" signals.
    /// Higher = stricter, only very strong trends become FullBear/FullBull.
    /// </summary>
    public double ThresholdBull { get; set; } = 0.55;

    /// <summary>
    /// Weight of breadth (percentage of up vs down candles).
    /// Final score = (1 - BreadthWeight) * (slope/volatility) + BreadthWeight * breadth.
    /// Range: [0.0, 1.0].
    /// Default: 0.30 (30%).
    /// Higher = breadth has more influence, slope/vol less.
    /// Lower = slope/vol dominates the trend classification.
    /// </summary>
    public double BreadthWeight { get; set; } = 0.30;

    /// <summary>
    /// How many candles back we are looking for the profiling.
    /// </summary>
    public int Window { get; set; } = window;

    public override string ToString()
        => $"Window_{Window}" +
           $"-NeutralBand_{NeutralBand}" +
           $"-MinR2ForTrend_{MinR2ForTrend}" +
           $"-ThresholdBear_{ThresholdBear}" +
           $"-ThresholdBull_{ThresholdBull}" +
           $"-BreadthWeight_{BreadthWeight}";

    // --- Presets --- //

    public static TrendConfiguration Balanced() => new(_default_window)
    {
        NeutralBand = 0.10,
        MinR2ForTrend = 0.20,
        ThresholdBear = 0.15,
        ThresholdBull = 0.55,
        BreadthWeight = 0.30
    };

    public static TrendConfiguration Conservative() => new(_default_window)
    {
        NeutralBand = 0.15,    // wider neutral zone
        MinR2ForTrend = 0.30,  // need stronger fit
        ThresholdBear = 0.20,  // stricter bear/bull entry
        ThresholdBull = 0.65,  // very strict for "full" trends
        BreadthWeight = 0.25   // rely more on slope/vol
    };

    public static TrendConfiguration Aggressive() => new(_default_window)
    {
        NeutralBand = 0.05,    // narrow neutral zone
        MinR2ForTrend = 0.10,  // allow weak fits
        ThresholdBear = 0.10,  // classify bear/bull easily
        ThresholdBull = 0.40,  // easier to call "full" trends
        BreadthWeight = 0.40   // breadth matters more
    };
}


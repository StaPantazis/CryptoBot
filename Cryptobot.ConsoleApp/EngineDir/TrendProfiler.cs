using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.EngineDir;

public static class TrendProfiler
{
    public static Trend Profile<T>(IReadOnlyList<T> candles, int window, TrendConfiguration config) where T : Candle
    {
        if (candles == null || window < 3 || candles.Count < window)
        {
            throw new ArgumentException("Not enough candles (need ≥ 3 and ≥ window).");
        }

        // --- Config safety (defaults + clamps) ---
        var cfg = EnsureValid(config ?? TrendConfiguration.Balanced());

        // --- Slice & extract prices ---
        var slice = candles.Skip(candles.Count - window).Take(window).ToArray();
        var closes = slice.Select(c => (double)c.ClosePrice).ToArray();

        // --- Price domain checks for log ---
        if (Array.Exists(closes, p => p <= 0 || double.IsNaN(p) || double.IsInfinity(p)))
        {
            throw new ArgumentException("ClosePrice must be finite and > 0 for log transform.");
        }

        // log-prices & regression axis
        var y = closes.Select(x => Math.Log(x)).ToArray();
        var x = Enumerable.Range(0, window).Select(i => (double)i).ToArray();

        var (slope, r2) = LinearRegressionSlopeR2(x, y);

        // log-returns
        var rets = new double[window - 1];
        for (var i = 1; i < window; i++)
        {
            rets[i - 1] = y[i] - y[i - 1];
        }

        var vol = SampleStdDev(rets);
        var breadth = ComputeBreadthFromReturns(rets); // [-1..+1]

        // --- Zero / ultra-low vol → Neutral (don’t let breadth tilt it) ---
        const double eps = 1e-9;
        if (vol < eps)
        {
            return Trend.Neutral;
        }

        // Drift-to-noise + breadth blend
        var t = slope / vol;
        var raw = ((1.0 - cfg.BreadthWeight) * t) + (cfg.BreadthWeight * breadth);
        var strength = 0.5 + (0.5 * Math.Sqrt(Math.Max(0, r2))); // [0.5..1]
        var score = Math.Tanh(raw * strength);

        // --- Labeling gates (consistent & symmetric) ---
        if (Math.Abs(score) < cfg.NeutralBand || r2 < cfg.MinR2ForTrend)
        {
            return Trend.Neutral;
        }
        else if (score >= cfg.ThresholdBull)
        {
            return Trend.FullBull;
        }
        else if (score >= cfg.ThresholdBear)
        {
            return Trend.Bull;
        }
        else if (score <= -cfg.ThresholdBull)
        {
            return Trend.FullBear;
        }

        return score <= -cfg.ThresholdBear ? Trend.Bear : Trend.Neutral;
    }

    private static TrendConfiguration EnsureValid(TrendConfiguration c)
    {
        // clamp all to [0,1]; enforce ordering for thresholds
        c.NeutralBand = Clamp01(c.NeutralBand);
        c.MinR2ForTrend = Clamp01(c.MinR2ForTrend);
        c.BreadthWeight = Clamp01(c.BreadthWeight);
        c.ThresholdBear = Clamp01(c.ThresholdBear);
        c.ThresholdBull = Clamp01(Math.Max(c.ThresholdBull, c.ThresholdBear)); // ensure Bull ≥ Bear
        return c;
    }

    private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

    private static (double slope, double r2) LinearRegressionSlopeR2(double[] x, double[] y)
    {
        var n = x.Length;
        double meanX = x.Average(), meanY = y.Average();
        double sxx = 0, sxy = 0, syy = 0;
        for (var i = 0; i < n; i++)
        {
            var dx = x[i] - meanX;
            var dy = y[i] - meanY;
            sxx += dx * dx;
            sxy += dx * dy;
            syy += dy * dy;
        }

        var slope = sxx > 0 ? sxy / sxx : 0.0;

        double ssRes = 0;
        for (var i = 0; i < n; i++)
        {
            var yhat = meanY + (slope * (x[i] - meanX));
            var err = y[i] - yhat;
            ssRes += err * err;
        }
        var r2 = syy > 0 ? 1.0 - (ssRes / syy) : 0.0;
        if (double.IsNaN(r2))
        {
            r2 = 0.0;
        }

        return (slope, Math.Max(0, Math.Min(1, r2)));
    }

    private static double SampleStdDev(double[] data)
    {
        var n = data.Length;
        if (n < 2)
        {
            return 0.0;
        }

        var mean = data.Average();
        double sumSq = 0;
        for (var i = 0; i < n; i++)
        {
            var d = data[i] - mean;
            sumSq += d * d;
        }
        return Math.Sqrt(Math.Max(0, sumSq / (n - 1)));
    }

    private static double ComputeBreadthFromReturns(double[] rets)
    {
        if (rets.Length == 0)
        {
            return 0;
        }

        int up = 0, down = 0;
        for (var i = 0; i < rets.Length; i++)
        {
            if (rets[i] > 0)
            {
                up++;
            }
            else if (rets[i] < 0)
            {
                down++;
            }
        }
        var total = up + down;
        return total == 0 ? 0 : (up - down) / (double)total;
    }
}

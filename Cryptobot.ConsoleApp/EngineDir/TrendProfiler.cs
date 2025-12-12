using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.EngineDir;

public static class TrendProfiler
{
    public static Trend ProfileByMovingAverage<T>(CacheService? cache, CandleSlice<T> slice) where T : Candle
    {
        if (cache == null)
        {
            var ma = GetMovingAverage(slice);

            return ma is null
                ? Trend.Neutral
                : (double)slice.LiveCandle.Indicators.MovingAverage! >= (double)ma! ? Trend.Bull : Trend.Bear;
        }

        return cache.MovingAverageTrendCache[slice.LiveCandle.DayKey].Trend;
    }

    public static Trend ProfileAiTrend<T>(CandleSlice<T> slice, AiTrendConfiguration config) where T : Candle
    {
        var candles = slice.Candles;

        if (candles.Count < config.Window)
        {
            return Trend.Neutral;
        }
        else if (candles.Count > config.Window)
        {
            // Case of Moving Average strategy along with AI Trend
            slice = slice.GetSlice(config.Window);
        }

        var closes = candles.Select(c => (double)c.ClosePrice).ToArray();

        // --- Price domain checks for log ---
        if (Array.Exists(closes, p => p <= 0 || double.IsNaN(p) || double.IsInfinity(p)))
        {
            throw new ArgumentException("ClosePrice must be finite and > 0 for log transform.");
        }

        // log-prices & regression axis
        var y = closes.Select(x => Math.Log(x)).ToArray();
        var x = Enumerable.Range(0, config.Window).Select(i => (double)i).ToArray();

        var (slope, r2) = LinearRegressionSlopeR2(x, y);

        // log-returns
        var rets = new double[config.Window - 1];
        for (var i = 1; i < config.Window; i++)
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
        var raw = ((1.0 - config.BreadthWeight) * t) + (config.BreadthWeight * breadth);
        var strength = 0.5 + (0.5 * Math.Sqrt(Math.Max(0, r2))); // [0.5..1]
        var score = Math.Tanh(raw * strength);

        // --- Labeling gates (consistent & symmetric) ---
        if (Math.Abs(score) < config.NeutralBand || r2 < config.MinR2ForTrend)
        {
            return Trend.Neutral;
        }
        else if (score >= config.ThresholdBull)
        {
            return Trend.FullBull;
        }
        else if (score >= config.ThresholdBear)
        {
            return Trend.Bull;
        }
        else if (score <= -config.ThresholdBull)
        {
            return Trend.FullBear;
        }

        return score <= -config.ThresholdBear ? Trend.Bear : Trend.Neutral;
    }

    public static double? GetMovingAverage<T>(CandleSlice<T> slice) where T : Candle
    {
        if (slice.Candles.Count is 0 or < Constants.MOVING_AVERAGE_WINDOW)
        {
            return null;
        }
        else if (slice.Candles.Count != Constants.MOVING_AVERAGE_WINDOW)
        {
            throw new ArgumentException("Slice candle count should match its usage");
        }

        return slice.Candles.Average(c => c.ClosePrice);
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

    private static AiTrendConfiguration EnsureValid(AiTrendConfiguration c)
    {
        // clamp all to [0,1]; enforce ordering for thresholds
        var neutralBand = Clamp01(c.NeutralBand);
        var minR2ForTrend = Clamp01(c.MinR2ForTrend);
        var breadthWeight = Clamp01(c.BreadthWeight);
        var thresholdBear = Clamp01(c.ThresholdBear);
        var thresholdBull = Clamp01(Math.Max(c.ThresholdBull, c.ThresholdBear)); // ensure Bull ≥ Bear

        if (neutralBand != c.NeutralBand
            || minR2ForTrend != c.MinR2ForTrend
            || breadthWeight != c.BreadthWeight
            || thresholdBear != c.ThresholdBear
            || thresholdBull != c.ThresholdBull)
        {
            throw new ArgumentException();
        }

        return c;
    }
}

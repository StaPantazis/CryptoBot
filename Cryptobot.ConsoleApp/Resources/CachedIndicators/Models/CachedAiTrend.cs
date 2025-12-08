using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;

namespace Cryptobot.ConsoleApp.Resources.CachedIndicators.Models;

public class CachedAiTrend(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedTrend(openDateTime, trend)
{
    public override long Key { get; } = candleInterval switch
    {
        CandleInterval.Five_Minutes => openDateTime.BuildFiveMinuteKey(),
        CandleInterval.Fifteen_Minutes => openDateTime.BuildFifteenMinuteKey(),
        _ => throw new NotImplementedException()
    };

    public static string CachedFileName(AiTrendProfile profile, CandleInterval candleInterval)
    {
        var fileName = profile switch
        {
            AiTrendProfile.Default => $"{nameof(CachedAiTrend_Default)}_{candleInterval}",
            AiTrendProfile.Balanced => $"{nameof(CachedAiTrend_Balanced)}_{candleInterval}",
            AiTrendProfile.Conservative => $"{nameof(CachedAiTrend_Conservative)}_{candleInterval}",
            AiTrendProfile.Aggressive => $"{nameof(CachedAiTrend_Aggressive)}_{candleInterval}",
            _ => throw new NotImplementedException(),
        };

        return $"{fileName}.parquet";
    }

    public static CandleInterval GetCandleIntervalFromFileName(string fileName)
    {
        return fileName switch
        {
            _ when fileName.Contains(CandleInterval.Five_Minutes.ToString()) => CandleInterval.Five_Minutes,
            _ when fileName.Contains(CandleInterval.Fifteen_Minutes.ToString()) => CandleInterval.Fifteen_Minutes,
            _ => throw new DataMisalignedException($"Cached AI Trend filename '{fileName}' is not defined")
        };
    }
}

public class CachedAiTrend_Balanced(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedAiTrend(openDateTime, trend, candleInterval);
public class CachedAiTrend_Conservative(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedAiTrend(openDateTime, trend, candleInterval);
public class CachedAiTrend_Aggressive(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedAiTrend(openDateTime, trend, candleInterval);
public class CachedAiTrend_Default(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedAiTrend(openDateTime, trend, candleInterval);
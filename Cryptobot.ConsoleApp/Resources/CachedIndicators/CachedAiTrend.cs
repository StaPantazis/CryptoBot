using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;

namespace Cryptobot.ConsoleApp.Resources.CachedIndicators;

public class CachedAiTrend(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedTrend(openDateTime, trend)
{
    public override long Key { get; } = candleInterval switch
    {
        CandleInterval.Five_Minutes => openDateTime.BuildFiveMinuteKey(),
        CandleInterval.Fifteen_Minutes => openDateTime.BuildFifteenMinuteKey(),
        _ => throw new NotImplementedException()
    };
}

public class CachedAiTrend_Balanced(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedAiTrend(openDateTime, trend, candleInterval);
public class CachedAiTrend_Conservative(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedAiTrend(openDateTime, trend, candleInterval);
public class CachedAiTrend_Aggressive(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedAiTrend(openDateTime, trend, candleInterval);
public class CachedAiTrend_Default(DateTime openDateTime, Trend trend, CandleInterval candleInterval) : CachedAiTrend(openDateTime, trend, candleInterval);
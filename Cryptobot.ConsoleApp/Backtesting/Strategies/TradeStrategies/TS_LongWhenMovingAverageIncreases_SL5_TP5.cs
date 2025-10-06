using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_LongWhenMovingAverageIncreases_SL5_TP5 : TradeStrategy
{
    public override string Name { get; } = "Trade when Moving Average increases 300 times | SL -5% | TP +5%";
    public override string NameOf { get; } = nameof(TS_LongWhenMovingAverageIncreases_SL5_TP5);
    public override IndicatorType[] RelevantIndicators { get; protected set; } = [IndicatorType.MovingAverage];

    protected override double StopLossLong<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 1.05;

    protected override double StopLossShort<T>(List<T> candles, int currentCandleIndex) => 1.08;
    protected override double TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 0.93;

    protected override bool ShouldShort<T>(List<T> candles, int currentCandleIndex) => false;

    protected override bool ShouldLong<T>(List<T> candles, int currentCandleIndex)
    {
        var candle = candles[currentCandleIndex];

        if (currentCandleIndex < 300)
        {
            return false;
        }

        var last300 = candles.Skip(currentCandleIndex - 300).Take(300).Select(x => x.Indicators.MovingAverage).ToArray();
        return last300.All(x => x < candle.Indicators.MovingAverage);
    }
}

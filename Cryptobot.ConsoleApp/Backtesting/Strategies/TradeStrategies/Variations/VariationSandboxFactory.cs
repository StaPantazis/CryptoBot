using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;

public static class VariationSandboxFactory
{
    public static VariationTradeStrategy<BybitCandle> AllInMA()
    {
        return new VariationTradeStrategy<BybitCandle>(new StrategyVariationsBundle<BybitCandle>(
            ShouldLong: (candles, currentCandleIndex, variables) =>
            {
                var nMABackCheck = (int)variables.movingAverageNBack!;
                var candle = candles[currentCandleIndex];

                if (currentCandleIndex < nMABackCheck)
                {
                    return false;
                }

                var lastN = candles.Skip(currentCandleIndex - nMABackCheck).Take(nMABackCheck).Select(x => x.Indicators.MovingAverage).ToArray();
                return lastN.All(x => x < candle.ClosePrice) && variables.applicableTrends!.Contains(candle.Indicators.MicroTrend);
            },

            ShouldShort: (b, n, _) => false,

            MovingAverageNBack: [40, 100],
            StopLossLong: [0.95, 0.97],
            TakeProfitLong: [1.05, 1.03],
            StopLossShort: null,
            TakeProfitShort: null,
            IndicatorTypes: [[IndicatorType.MovingAverage], [IndicatorType.MovingAverage, IndicatorType.MicroTrend]],
            ApplicableTrends: [[Trend.Neutral, Trend.Bull]]));
    }
}

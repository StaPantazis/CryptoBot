using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;

public static class VariationSandboxFactory
{
    public static VariationTradeStrategy<BybitCandle> AllInMA()
    {
        return new VariationTradeStrategy<BybitCandle>(new StrategyVariationsBundle<BybitCandle>(
            ShouldLong: (candles, currentCandleIndex, candleInterval, variables) =>
            {
                var nMABackCheck = (int)variables.movingAverageNBack!;
                var candle = candles[currentCandleIndex];

                if (currentCandleIndex < nMABackCheck || currentCandleIndex < Constants.CandleCountToIgnoreBeforeTrade[candleInterval])
                {
                    return false;
                }

                var lastN = candles.Skip(currentCandleIndex - nMABackCheck).Take(nMABackCheck).Select(x => x.Indicators.MovingAverage).ToArray();
                return lastN.All(x => x < candle.ClosePrice) && variables.applicableTrends!.Contains(candle.Indicators.MicroTrend);
            },
            ShouldShort: (_, _, _, _) => false,

            MovingAverageNBack: [40, 100],
            StopLossLong: [0.95, 0.97],
            TakeProfitLong: [1.05, 1.03],
            StopLossShort: null,
            TakeProfitShort: null,
            IndicatorTypes: [[IndicatorType.MovingAverage], [IndicatorType.MovingAverage, IndicatorType.MicroTrend]],
            ApplicableTrends: [[Trend.Neutral, Trend.Bull]]));
    }
}

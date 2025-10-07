using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;

public class VariationTradeStrategy<T>(StrategyVariationsBundle<T>? strategyVariation) : TradeStrategyBase where T : Candle
{
    private VariationTradeStrategy(StrategyVariation<T> variationToRun) : this((StrategyVariationsBundle<T>?)null)
    {
        Name = NameOf = variationToRun.Name;
        VariationToRun = variationToRun;
        RelevantIndicators = VariationToRun.IndicatorTypes ?? [];
    }

    public override string Name { get; protected set; }
    public override string NameOf { get; protected set; }
    public override IndicatorType[] RelevantIndicators { get; protected set; }
    protected StrategyVariation<T> VariationToRun { get; }
    protected StrategyVariationsBundle<T>? Variations { get; } = strategyVariation;

    public List<VariationTradeStrategy<T>> GetVariations()
    {
        var variationStrategies = new List<VariationTradeStrategy<T>>();
        foreach (var ma in Variations!.MovingAverageNBack ?? [null])
        {
            foreach (var sl_l in Variations.StopLossLong ?? [null])
            {
                foreach (var tp_l in Variations.TakeProfitLong ?? [null])
                {
                    foreach (var sl_s in Variations.StopLossShort ?? [null])
                    {
                        foreach (var tp_s in Variations.TakeProfitShort ?? [null])
                        {
                            foreach (var it in Variations.IndicatorTypes ?? [null])
                            {
                                foreach (var tr in Variations.ApplicableTrends ?? [null])
                                {
                                    var newVariation = new StrategyVariation<T>(
                                        ShouldLong: (candles, currentCandleIndex) => Variations.ShouldLong(candles, currentCandleIndex, (ma, tr)),
                                        ShouldShort: (candles, currentCandleIndex) => Variations.ShouldShort(candles, currentCandleIndex, (ma, tr)),
                                        MovingAverage: ma,
                                        StopLossLong: sl_l,
                                        TakeProfitLong: tp_l,
                                        StopLossShort: sl_s,
                                        TakeProfitShort: tp_s,
                                        IndicatorTypes: it is null ? null : it.Select(x => (IndicatorType)x!).ToArray()!,
                                        ApplicableTrends: tr is null ? null : tr.Select(x => (Trend)x!).ToArray()!);

                                    variationStrategies.Add(new VariationTradeStrategy<T>(newVariation));
                                }
                            }
                        }
                    }
                }
            }
        }

        return variationStrategies;
    }

    protected override bool ShouldLong<T1>(List<T1> candles, int currentCandleIndex)
    {
        return candles is List<T> typed
            ? VariationToRun.ShouldLong(typed, currentCandleIndex)
            : throw new InvalidOperationException($"VariationTradeStrategy expects candles of type {typeof(T).Name} but got {typeof(T1).Name}.");
    }

    protected override bool ShouldShort<T1>(List<T1> candles, int currentCandleIndex)
    {
        return candles is List<T> typed
            ? VariationToRun.ShouldShort(typed, currentCandleIndex)
            : throw new InvalidOperationException($"VariationTradeStrategy expects candles of type {typeof(T).Name} but got {typeof(T1).Name}.");
    }

    protected override double StopLossLong<T1>(List<T1> candles, int currentCandleIndex) => VariationToRun.StopLossLong ?? -1;
    protected override double StopLossShort<T1>(List<T1> candles, int currentCandleIndex) => VariationToRun.StopLossShort ?? -1;
    protected override double TakeProfitLong<T1>(List<T1> candles, int currentCandleIndex) => VariationToRun.TakeProfitLong ?? -1;
    protected override double TakeProfitShort<T1>(List<T1> candles, int currentCandleIndex) => VariationToRun.TakeProfitShort ?? -1;
}

using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies;

/// <summary>
/// We have this xxxOverridable logic because we want to force variations if existing
/// </summary>
/// <param name="variation"></param>
public abstract class TradeStrategyBase(CacheService cache, StrategyVariation? variation = null) : StrategyBase
{
    private readonly double? _stopLossLong = variation?.StopLossLong;
    private readonly double? _stopLossShort = variation?.StopLossShort;
    private readonly double? _takeProfitLong = variation?.TakeProfitLong;
    private readonly double? _takeProfitShort = variation?.TakeProfitShort;

    protected abstract string NameOverridable { get; set; }
    public override string Name => $"{NameOverridable}{(variation is null ? "" : $"_Variation:_{variation.Name}")}";

    protected CacheService Cache { get; } = cache;
    protected IndicatorService IndicatorService => field ??= new IndicatorService(Cache, RelevantIndicators);

    protected virtual AiTrendConfiguration AiTrendConfigOverridable { get; } = AiTrendConfiguration.Create(AiTrendProfile.Default);
    public AiTrendConfiguration AiTrendConfig => field ??= variation?.AiTrendConfig ?? AiTrendConfigOverridable;

    public virtual IndicatorType[] RelevantIndicators { get; } = [];

    public double? StopLoss<T>(List<T> candles, int currentCandleIndex, PositionSide position) where T : Candle
        => position is PositionSide.Long
            ? _stopLossLong ?? StopLossLong(candles, currentCandleIndex)
            : _stopLossShort ?? StopLossShort(candles, currentCandleIndex);

    public double? TakeProfit<T>(List<T> candles, int currentCandleIndex, PositionSide position) where T : Candle
        => position is PositionSide.Long
            ? _takeProfitLong ?? TakeProfitLong(candles, currentCandleIndex)
            : _takeProfitShort ?? TakeProfitShort(candles, currentCandleIndex);

    public virtual bool ShouldOpenTrade<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval, out PositionSide[]? positions) where T : Candle
    {
        if (Spot.AvailableBudget < 10)
        {
            positions = null;
            return false;
        }

        var positionsList = new List<PositionSide>();

        if (ShouldShort(candles, currentCandleIndex, candleInterval))
        {
            positionsList.Add(PositionSide.Short);
        }

        if (ShouldLong(candles, currentCandleIndex, candleInterval))
        {
            positionsList.Add(PositionSide.Long);
        }

        positions = positionsList.Any() ? positionsList.ToArray() : null;
        return positionsList.Any();
    }

    public virtual bool ShouldCloseTrade<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval, Trade trade) where T : Candle
    {
        var candle = candles[currentCandleIndex];
        return (trade.PositionSide is PositionSide.Long
                && (candle.LowPrice <= trade.StopLoss || candle.HighPrice >= trade.TakeProfit || ShouldExitLongTrade(candles, currentCandleIndex, trade, candleInterval)))
            || (trade.PositionSide is PositionSide.Short
                && (candle.HighPrice >= trade.StopLoss || candle.LowPrice <= trade.TakeProfit || ShouldExitShortTrade(candles, currentCandleIndex, trade, candleInterval)));
    }

    public virtual bool ShouldExitLongTrade<T>(List<T> candles, int currentCandleIndex, Trade trade, CandleInterval candleInterval) where T : Candle => false;
    public virtual bool ShouldExitShortTrade<T>(List<T> candles, int currentCandleIndex, Trade trade, CandleInterval candleInterval) where T : Candle => false;
    protected abstract double? StopLossLong<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected abstract double? StopLossShort<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected abstract double? TakeProfitLong<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected abstract double? TakeProfitShort<T>(List<T> candles, int currentCandleIndex) where T : Candle;
    protected virtual bool ShouldShort<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval) where T : Candle => false;
    protected virtual bool ShouldLong<T>(List<T> candles, int currentCandleIndex, CandleInterval candleInterval) where T : Candle => false;
}

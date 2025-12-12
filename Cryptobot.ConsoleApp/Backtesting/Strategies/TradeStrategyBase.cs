using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies;

/// <summary>
/// We have this xxxOverridable logic because we want to force variations if existing
/// </summary>
/// <param name="variation"></param>
public abstract class TradeStrategyBase(
    CacheService cache,
    CandleInterval tradingCandleInterval,
    StrategyVariation? variation = null,
    CandleInterval? aiTrendInterval = null) : StrategyBase
{
    private int? _sliceSize = null;
    private readonly double? _stopLossLong = variation?.StopLossLong;
    private readonly double? _stopLossShort = variation?.StopLossShort;
    private readonly double? _takeProfitLong = variation?.TakeProfitLong;
    private readonly double? _takeProfitShort = variation?.TakeProfitShort;

    protected abstract string NameOverridable { get; set; }
    public override string Name => $"{NameOverridable}{(variation is null ? "" : $"  || _Variation:_ {variation.Name}")}";

    protected CacheService Cache { get; } = cache;
    protected CandleInterval TradingCandleInterval { get; } = tradingCandleInterval;
    protected IndicatorService IndicatorService => field ??= new IndicatorService(Cache, RelevantIndicators);

    protected virtual AiTrendConfiguration AiTrendConfigOverridable { get; } = AiTrendConfiguration.Create(AiTrendProfile.Default);
    public AiTrendConfiguration AiTrendConfig => field ??= variation?.AiTrendConfig ?? AiTrendConfigOverridable;

    public virtual IndicatorType[] RelevantIndicators { get; } = [];
    protected CandleInterval? AiTrendInterval { get; } = aiTrendInterval;

    public int SliceSize => _sliceSize ??= IndicatorService.SliceSizeBasedOnIndicators;

    public double? StopLoss<T>(CandleSlice<T> slice, PositionSide position) where T : Candle
        => position is PositionSide.Long
            ? _stopLossLong ?? StopLossLong(slice)
            : _stopLossShort ?? StopLossShort(slice);

    public double? TakeProfit<T>(CandleSlice<T> slice, PositionSide position) where T : Candle
        => position is PositionSide.Long
            ? _takeProfitLong ?? TakeProfitLong(slice)
            : _takeProfitShort ?? TakeProfitShort(slice);

    public virtual bool ShouldOpenTrade<T>(CandleSlice<T> slice, out PositionSide[]? positions) where T : Candle
    {
        if (Spot.AvailableBudget < 10)
        {
            positions = null;
            return false;
        }

        var positionsList = new List<PositionSide>();

        if (ShouldShort(slice))
        {
            positionsList.Add(PositionSide.Short);
        }

        if (ShouldLong(slice))
        {
            positionsList.Add(PositionSide.Long);
        }

        positions = positionsList.Any() ? positionsList.ToArray() : null;
        return positionsList.Any();
    }

    public virtual bool ShouldCloseTrade<T>(CandleSlice<T> slice, Trade trade) where T : Candle
    {
        return (trade.PositionSide is PositionSide.Long
                && (slice.LastCandle.LowPrice <= trade.StopLoss || slice.LastCandle.HighPrice >= trade.TakeProfit || ShouldExitLongTrade(slice, trade)))
            || (trade.PositionSide is PositionSide.Short
                && (slice.LastCandle.HighPrice >= trade.StopLoss || slice.LastCandle.LowPrice <= trade.TakeProfit || ShouldExitShortTrade(slice, trade)));
    }

    public virtual bool ShouldExitLongTrade<T>(CandleSlice<T> slice, Trade trade) where T : Candle => false;
    public virtual bool ShouldExitShortTrade<T>(CandleSlice<T> slice, Trade trade) where T : Candle => false;
    protected abstract double? StopLossLong<T>(CandleSlice<T> slice) where T : Candle;
    protected abstract double? StopLossShort<T>(CandleSlice<T> slice) where T : Candle;
    protected abstract double? TakeProfitLong<T>(CandleSlice<T> slice) where T : Candle;
    protected abstract double? TakeProfitShort<T>(CandleSlice<T> slice) where T : Candle;
    protected virtual bool ShouldShort<T>(CandleSlice<T> slice) where T : Candle => false;
    protected virtual bool ShouldLong<T>(CandleSlice<T> slice) where T : Candle => false;
}

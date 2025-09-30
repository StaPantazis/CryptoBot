using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies;

public class TS_Every100Candles_SL5_TP5 : TradeStrategy
{
    public override string Name { get; } = "Trade every 100 candles | SL -5% | TP +5%";
    public override string NameOf { get; } = nameof(TS_Every100Candles_SL5_TP5);

    public override bool ShouldOpenTrade<T>(List<T> candles, int currentCandleIndex, out PositionSide? position)
    {
        var baseShouldTrade = base.ShouldOpenTrade(candles, currentCandleIndex, out position);
        return baseShouldTrade && currentCandleIndex % 100 == 0;
    }

    protected override double StopLossLong<T>(List<T> candles, int currentCandleIndex) => 0.95;
    protected override double TakeProfitLong<T>(List<T> candles, int currentCandleIndex) => 1.05;

    protected override double StopLossShort<T>(List<T> candles, int currentCandleIndex) => 1.05;
    protected override double TakeProfitShort<T>(List<T> candles, int currentCandleIndex) => 0.95;

    protected override bool ShouldShort<T>(List<T> candles, int currentCandleIndex) => currentCandleIndex % 400 == 0;

    protected override bool ShouldLong<T>(List<T> candles, int currentCandleIndex) => currentCandleIndex % 400 != 0;
}

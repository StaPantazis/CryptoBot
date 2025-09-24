
namespace Cryptobot.ConsoleApp.Backtesting.TradeStrategies;

public class TS_Every100Candles_SL5_TP5 : TradeStrategy
{
    public override string Name { get; } = "Trade every 100 candles | SL -5% | TP +5%";
    public override string NameOf { get; } = nameof(TS_Every100Candles_SL5_TP5);

    public override double StopLoss<T>(List<T> candles, int currentCandleIndex) => 0.95;
    public override double TakeProfit<T>(List<T> candles, int currentCandleIndex) => 1.05;
    public override bool ShouldOpenTrade<T>(List<T> candles, int currentCandleIndex) => currentCandleIndex % 100 == 0 && base.ShouldOpenTrade(candles, currentCandleIndex);
}

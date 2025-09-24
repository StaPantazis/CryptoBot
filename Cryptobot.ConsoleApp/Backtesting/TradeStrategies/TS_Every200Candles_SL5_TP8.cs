namespace Cryptobot.ConsoleApp.Backtesting.TradeStrategies;

public class TS_Every200Candles_SL5_TP8 : TradeStrategy
{
    public override string Name { get; } = "Trade every 200 candles | SL -5% | TP +8%";
    public override string NameOf { get; } = nameof(TS_Every200Candles_SL5_TP8);

    public override double StopLoss<T>(List<T> candles, int currentCandleIndex) => 0.95;
    public override double TakeProfit<T>(List<T> candles, int currentCandleIndex) => 1.08;
    public override bool ShouldOpenTrade<T>(List<T> candles, int currentCandleIndex) => currentCandleIndex % 200 == 0 && base.ShouldOpenTrade(candles, currentCandleIndex);
}

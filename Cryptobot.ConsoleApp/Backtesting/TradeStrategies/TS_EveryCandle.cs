namespace Cryptobot.ConsoleApp.Backtesting.TradeStrategies;

public class TS_EveryCandle : TradeStrategy
{
    public override string Name { get; } = "Trade every candle";
    public override string NameOf { get; } = nameof(TS_EveryCandle);
}

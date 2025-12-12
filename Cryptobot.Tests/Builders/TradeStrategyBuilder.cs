using Cryptobot.ConsoleApp.Backtesting.Strategies;
using Cryptobot.ConsoleApp.Backtesting.Strategies.TradeStrategies.Variations;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.Tests.Builders.Bases;
using Cryptobot.Tests.Models;

namespace Cryptobot.Tests.Builders;

public class TradeStrategyBuilder : BaseBuilder<TradeStrategyBase, TradeStrategyBuilder>
{
    private bool _long = false;
    private bool _short = false;
    private double? _stopLossLong = null;
    private double? _takeProfitLong = null;
    private double? _stopLossShort = null;
    private double? _takeProfitShort = null;
    private AiTrendConfiguration? _aiTrendConfig = null;

    public TradeStrategyBuilder WithAllowLong()
    {
        _long = true;
        return this;
    }

    public TradeStrategyBuilder WithAllowShort()
    {
        _short = true;
        return this;
    }

    /// <summary>
    /// Default is 5%.
    /// </summary>
    public TradeStrategyBuilder WithStopLossLong(double? stopLossLong)
    {
        _stopLossLong = stopLossLong;
        return this;
    }

    /// <summary>
    /// Default is 5%.
    /// </summary>
    public TradeStrategyBuilder WithTakeProfitLong(double? takeProfitLong)
    {
        _takeProfitLong = takeProfitLong;
        return this;
    }

    /// <summary>
    /// Default is 5%.
    /// </summary>
    public TradeStrategyBuilder WithStopLossShort(double? stopLossShort)
    {
        _stopLossShort = stopLossShort;
        return this;
    }

    /// <summary>
    /// Default is 5%.
    /// </summary>
    public TradeStrategyBuilder WithTakeProfitShort(double? takeProfitShort)
    {
        _takeProfitShort = takeProfitShort;
        return this;
    }

    public TradeStrategyBuilder WithAiTrendConfig(AiTrendConfiguration? aiTrendConfig)
    {
        _aiTrendConfig = aiTrendConfig;
        return this;
    }

    public override TradeStrategyBase Build()
    {
        var defaultVariation = new StrategyVariation()
        {
            StopLossLong = 0.95,
            TakeProfitLong = 1.05,
            StopLossShort = 1.05,
            TakeProfitShort = 0.95,
            AiTrendConfig = null,
        };

        return new TestTradeStrategy(_long, _short, variation: _stopLossLong == null
            ? defaultVariation
            : new StrategyVariation()
            {
                StopLossLong = _stopLossLong,
                TakeProfitLong = _takeProfitLong,
                StopLossShort = _stopLossShort,
                TakeProfitShort = _takeProfitShort,
                AiTrendConfig = _aiTrendConfig,
            });
    }
}

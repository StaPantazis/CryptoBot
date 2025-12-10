using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.EngineDir.Models;

public class FeesSettings(string symbol = Constants.SYMBOL_BTCUSDT, double? tradeFeeOverride = null, double? slippageOverride = null)
{
    private readonly string _symbol = symbol;
    private readonly double? _tradeFeeOverride = tradeFeeOverride;
    private readonly double? _slippageOverride = slippageOverride;

    private double? _slippageMultiplier = null;
    public double SlippageMultiplier => _slippageMultiplier ??= _slippageOverride ?? _symbol switch
    {
        Constants.SYMBOL_BTCUSDT => Constants.SLIPPAGE_MULTIPLIER_BTC,
        _ => throw new NotImplementedException()
    };

    private double? _tradeFee = null;
    public double TradeFeeMultiplier => _tradeFee ??= _tradeFeeOverride ?? Constants.TRADE_FEE_MULTIPLIER;
}

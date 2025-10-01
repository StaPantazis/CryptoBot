namespace Cryptobot.ConsoleApp.Utils;

public static class Constants
{
    public const string STRING_EURO = "€";
    public const string STRING_PERCENT = "%";

    public const string MARKET_SPOT = "spot";
    public const string MARKET_PERPETUAL_FUTURES = "linear";
    public const string MARKET_INVERSE_PERPETUAL_FUTURES = "inverse";
    public const string MARKET_OPTIONS = "option";

    public const string SYMBOL_BTCUSDT = "BTCUSDT";

    public const double TRADE_FEE = 0.00055;
    public const double SLIPPAGE_MULTIPLIER_BTC = 0.0001;
    public const double SLIPPAGE_MULTIPLIER_ALTS = 0.001;

    // STORING
    public const string PARQUET = ".parquet";
    public const string JSON = ".json";
    public const string ZIP = ".zip";
    public const string GZIP = ".gz";
}

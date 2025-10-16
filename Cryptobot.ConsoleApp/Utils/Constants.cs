using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

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

    public const int ONE_MINUTE_CANDLES_120_DAYS = 172800;
    public const int THREE_MINUTE_CANDLES_120_DAYS = 57600;
    public const int FIVE_MINUTE_CANDLES_120_DAYS = 34560;
    public const int FIFTEEN_MINUTE_CANDLES_120_DAYS = 11520;
    public const int ONE_DAY_CANDLES_120_DAYS = 120;

    public static readonly Dictionary<CandleInterval, int> CandleCountToIgnoreBeforeTrade = new() {
        { CandleInterval.One_Minute, ONE_MINUTE_CANDLES_120_DAYS },
        { CandleInterval.Three_Minutes, THREE_MINUTE_CANDLES_120_DAYS },
        { CandleInterval.Five_Minutes, FIVE_MINUTE_CANDLES_120_DAYS },
        { CandleInterval.Fifteen_Minutes, FIFTEEN_MINUTE_CANDLES_120_DAYS },
        { CandleInterval.One_Day, ONE_DAY_CANDLES_120_DAYS },
    };

    // STORING
    public const string PARQUET = ".parquet";
    public const string JSON = ".json";
    public const string ZIP = ".zip";
    public const string GZIP = ".gz";
}

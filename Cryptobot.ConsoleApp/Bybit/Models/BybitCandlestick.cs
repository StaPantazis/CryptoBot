namespace Cryptobot.ConsoleApp.Bybit.Models;

public class BybitCandlestick
{
    public DateTime OpenTime { get; set; }
    public DateTime CloseTime { get; set; }
    public double OpenPrice { get; set; }
    public double ClosePrice { get; set; }
    public double HighPrice { get; set; }
    public double LowPrice { get; set; }
    /// <summary>
    /// Total number of Bitcoin traded
    /// </summary>
    public double Volume { get; set; }
    /// <summary>
    /// Total number of Dollars traded
    /// </summary>
    public double QuoteVolume { get; set; }

    public double PriceDifPercentage => Math.Round(ClosePrice - OpenPrice, 2);

    public override string ToString() => $"{OpenTime:dd/MM HH:mm}| {Math.Round(OpenPrice, 2)} / {Math.Round(ClosePrice, 2)}  ({PriceDifPercentage})";

    public static List<BybitCandlestick> FromResponse(KlineResponse klineResponse)
    {
        var list = klineResponse.Result.List;
        var candlesticks = new List<BybitCandlestick>();

        foreach (var item in list)
        {
            var ts = long.Parse(item[0]);
            var openTime = DateTimeOffset.FromUnixTimeMilliseconds(ts).UtcDateTime;

            candlesticks.Add(new BybitCandlestick
            {
                OpenTime = openTime,
                CloseTime = openTime.AddMinutes(1),

                OpenPrice = double.Parse(item[1]),
                HighPrice = double.Parse(item[2]),
                LowPrice = double.Parse(item[3]),
                ClosePrice = double.Parse(item[4]),

                Volume = double.Parse(item[5]),
                QuoteVolume = double.Parse(item[6]),
            });
        }

        return candlesticks;
    }
}

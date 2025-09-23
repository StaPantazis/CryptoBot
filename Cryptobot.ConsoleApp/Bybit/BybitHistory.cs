using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Utils;
using Newtonsoft.Json;

namespace Cryptobot.ConsoleApp.Bybit;

public static class BybitHistory
{
    private const string _endpoint = "https://api.bybit.com/v5/market/kline";

    public static async Task Download()
    {
        var symbol = "BTCUSDT";
        var category = "linear"; // USDT perpetual
        var interval = "1";      // 1-minute candles

        var resourcesPath = PathHelper.GetResourcesPath();
        var http = new HttpClient();

        // Start from 2020-01-01 UTC
        var startDate = new DateTime(2020, 03, 25, 0, 0, 0, DateTimeKind.Utc);
        var today = DateTime.UtcNow.Date;

        for (var day = startDate; day < today; day = day.AddDays(1))
        {
            var startMs = new DateTimeOffset(day).ToUnixTimeMilliseconds();
            var endMs = new DateTimeOffset(day.AddDays(1)).ToUnixTimeMilliseconds();

            var url = $"{_endpoint}?category={category}&symbol={symbol}&interval={interval}&start={startMs}&end={endMs}&limit=5";

            var json = await http.GetStringAsync(url);
            var resp = JsonConvert.DeserializeObject<KlineResponse>(json);

            if (resp?.Result?.List == null || resp.Result.List.Count == 0)
            {
                Console.WriteLine($"No data for {day:yyyy-MM-dd}");
                continue;
            }

            // Bybit returns reverse chronological → fix order
            resp.Result.List.Reverse();

            var candlesticks = BybitCandlestick.FromResponse(resp);
            var output = JsonConvert.SerializeObject(candlesticks);

            var filename = $"{resourcesPath}\\{symbol}-1m-{day:yyyy-MM-dd}.json";
            await File.WriteAllTextAsync(filename, output);

            Console.WriteLine($"Saved {resp.Result.List.Count} candles to {filename}");
            await Task.Delay(200); // avoid rate limit
        }

        Console.WriteLine("Done.");
    }
}

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
        var startDate = new DateTime(2020, 03, 26, 0, 0, 0, DateTimeKind.Utc);
        var today = DateTime.UtcNow.Date;

        for (var day = startDate; day < today; day = day.AddDays(1))
        {
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("Checking ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{day:dd/MM/yyyy}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("... ");

            var filename = $"{resourcesPath}\\{symbol}-1m-{day:yyyy-MM-dd}.json";

            if (File.Exists(filename))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Already farmed!");
                continue;
            }

            Console.Write("Downloading... ");

            var dayStart = new DateTimeOffset(day).ToUnixTimeMilliseconds();
            var noon = new DateTimeOffset(day.AddHours(12)).ToUnixTimeMilliseconds();
            var dayEnd = new DateTimeOffset(day.AddDays(1)).ToUnixTimeMilliseconds();

            var urls = new[]
            {
                $"{_endpoint}?category={category}&symbol={symbol}&interval={interval}&start={dayStart}&end={noon}&limit=1000",
                $"{_endpoint}?category={category}&symbol={symbol}&interval={interval}&start={noon}&end={dayEnd}&limit=1000"
            };

            var allDailyCandles = new List<BybitCandlestick>();

            foreach (var url in urls)
            {
                var json = await http.GetStringAsync(url);
                var resp = JsonConvert.DeserializeObject<KlineResponse>(json);

                if (resp?.Result?.List == null || resp.Result.List.Count == 0)
                {
                    continue;
                }

                resp.Result.List.Reverse();
                allDailyCandles.AddRange(BybitCandlestick.FromResponse(resp));
                await Task.Delay(200); // avoid hitting rate limit
            }

            // Deduplicate & order
            var merged = allDailyCandles
                .Where(c => c.OpenTime < day.AddDays(1)) // drop next-day candle
                .GroupBy(c => c.OpenTime)
                .Select(g => g.First())
                .OrderBy(c => c.OpenTime)
                .ToList();

            if (merged.Count != 1440)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nWrong data for file {day}, found {merged.Count} records!");
            }

            var output = JsonConvert.SerializeObject(merged, Formatting.None);
            await File.WriteAllTextAsync(filename, output);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Check!");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Finished!");
        Console.ForegroundColor = ConsoleColor.White;
    }
}

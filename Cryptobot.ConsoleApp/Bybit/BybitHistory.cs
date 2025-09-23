using Cryptobot.ConsoleApp.Bybit.Enums;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Utils;
using Newtonsoft.Json;

namespace Cryptobot.ConsoleApp.Bybit;

public static class BybitHistory
{
    private const string _endpoint = "https://api.bybit.com/v5/market/kline";

    public static async Task Download(HistoryRequest historyRequest)
    {
        Console.WriteLine($"Downloadig intervals of {(int)historyRequest.Interval} minutes...\n");

        var resourcesPath = PathHelper.GetHistoryPath(historyRequest);
        var http = new HttpClient();

        // Start from 2020-03-26 UTC
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

            var filename = $"{resourcesPath}\\{historyRequest.Symbol}-{historyRequest.IntervalShortString}-{day:yyyy-MM-dd}.json";

            if (File.Exists(filename))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Already downloaded!");
                continue;
            }

            Console.Write("Downloading... ");

            var urls = GetUrlsByInterval(historyRequest, day);

            var allDailyCandles = new List<BybitCandle>();

            for (var i = 0; i < urls.Length; i++)
            {
                var url = urls[i];

                var json = await http.GetStringAsync(url);
                var resp = JsonConvert.DeserializeObject<KlineResponse>(json);

                if (resp?.Result?.List == null || resp.Result.List.Count == 0)
                {
                    continue;
                }

                allDailyCandles.AddRange(BybitCandle.FromResponse(resp));

                if (i < urls.Length - 1)
                {
                    await Task.Delay(200); // avoid hitting rate limit
                }
            }

            // Deduplicate & order
            var merged = allDailyCandles
                .Where(c => c.OpenTime < day.AddDays(1)) // drop next-day candle
                .GroupBy(c => c.OpenTime)
                .Select(g => g.First())
                .OrderBy(c => c.OpenTime)
                .ToList();

            if (merged.Count != historyRequest.CandlesticksDailyCount)
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

    private static string[] GetUrlsByInterval(HistoryRequest historyRequest, DateTime day)
    {
        var dayStart = new DateTimeOffset(day).ToUnixTimeMilliseconds();
        var dayEnd = new DateTimeOffset(day.AddDays(1)).ToUnixTimeMilliseconds();

        switch (historyRequest.Interval)
        {
            case CandleInterval.One_Minute:
                var noon = new DateTimeOffset(day.AddHours(12)).ToUnixTimeMilliseconds();

                return [
                    $"{_endpoint}?category={historyRequest.MarketCategory}&symbol={historyRequest.Symbol}&interval={(int)historyRequest.Interval}&start={dayStart}&end={noon}&limit=1000",
                    $"{_endpoint}?category={historyRequest.MarketCategory}&symbol={historyRequest.Symbol}&interval={(int)historyRequest.Interval}&start={noon}&end={dayEnd}&limit=1000"
                ];
            case CandleInterval.Five_Minutes:
                return [$"{_endpoint}?category={historyRequest.MarketCategory}&symbol={historyRequest.Symbol}&interval={(int)historyRequest.Interval}&start={dayStart}&end={dayEnd}&limit=1000"];
            default:
                throw new NotImplementedException();
        }
    }
}

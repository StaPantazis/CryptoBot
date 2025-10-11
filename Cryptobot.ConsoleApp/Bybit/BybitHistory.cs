using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;
using Newtonsoft.Json;

namespace Cryptobot.ConsoleApp.Bybit;

public static class BybitHistory
{
    private const string _endpoint = "https://api.bybit.com/v5/market/kline";

    public static async Task Download(BacktestingDetails details)
    {
        Printer.HistoryDownloadTitle(details);

        var resourcesPath = PathHelper.GetHistoryPath(details);
        var http = new HttpClient();

        // Start from 2020-03-26 UTC
        var startDate = new DateTime(2020, 03, 26, 0, 0, 0, DateTimeKind.Utc);
        var today = DateTime.UtcNow.Date;

        // For daily candles, we can download multiple days in one go
        if (details.Interval == CandleInterval.One_Day)
        {
            await DownloadDailyCandles(details, http, resourcesPath, startDate, today);
            return;
        }

        var existingFiles = Directory.GetFiles(resourcesPath, $"{details.Symbol}-{details.IntervalShortString}-*.parquet");

        if (existingFiles.Length > 0)
        {
            var lastFile = existingFiles
                .Select(Path.GetFileNameWithoutExtension)
                .Select(f => f![^10..]) // take last 10 chars = "yyyy-MM-dd"
                .Select(d => DateTime.ParseExact(d, "yyyy-MM-dd", null))
                .OrderBy(d => d)
                .Last();

            startDate = lastFile.AddDays(1);
        }

        if (startDate >= today)
        {
            Printer.AlreadyDownloaded();
            Printer.EmptyLine();
            return;
        }

        for (var day = startDate; day < today; day = day.AddDays(1))
        {
            Printer.CheckingHistory(day);

            var filename = $"{resourcesPath}\\{details.Symbol}-{details.IntervalShortString}-{day:yyyy-MM-dd}{Constants.PARQUET}";

            if (File.Exists(filename))
            {
                Printer.AlreadyDownloaded();
                Printer.EmptyLine();
                continue;
            }

            Printer.Downloading();

            var urls = GetMinuteUrlsByInterval(details, day);
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

            if (merged.Count != details.CandlesticksDailyCount)
            {
                Printer.WrongHistoryMinute(day, merged.Count);
            }

            await ParquetService.SaveBybitCandles(merged, filename);

            Printer.Done();
        }

        Printer.Finished();
    }

    private static async Task DownloadDailyCandles(BacktestingDetails details, HttpClient http, string resourcesPath, DateTime startDate, DateTime endDate)
    {
        var filename = Path.Combine(resourcesPath, $"{details.Symbol}-1D-ALL{Constants.PARQUET}");
        var allCandles = new List<BybitCandle>();
        const int limit = 1000;

        if (File.Exists(filename))
        {
            allCandles = await ParquetService.LoadCandles<BybitCandle>(filename);
        }

        var existingDates = allCandles.Select(c => c.OpenTime.Date).ToHashSet();

        var missingDates = Enumerable
            .Range(0, (endDate - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset))
            .Where(date => !existingDates.Contains(date))
            .ToList();

        if (missingDates.Count == 0)
        {
            Printer.AlreadyDownloaded();
            Printer.EmptyLine();
            return;
        }

        Printer.Downloading();

        // 🔵 Step 3 — Download missing batches (1000 days per request)
        var batchStartIndex = 0;

        while (batchStartIndex < missingDates.Count)
        {
            var batchStart = missingDates[batchStartIndex];
            var batchEnd = missingDates[Math.Min(batchStartIndex + limit - 1, missingDates.Count - 1)];

            var url = $"{_endpoint}?category={details.MarketCategory}&symbol={details.Symbol}&interval=D" +
                      $"&start={ToUnix(batchStart)}&end={ToUnix(batchEnd)}&limit={limit}";

            Printer.CheckingHistory(batchStart, batchEnd);

            string json;
            try
            {
                json = await http.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Printer.WrongHistoryDaily(batchStart, batchEnd, ex);
                await Task.Delay(1000);
                batchStartIndex += limit;
                continue;
            }

            var resp = JsonConvert.DeserializeObject<KlineResponse>(json);

            if (resp?.Result?.List == null || resp.Result.List.Count == 0)
            {
                Printer.WrongHistoryDaily(batchStart, batchEnd, "Error in deserialization.");
                batchStartIndex += limit;
                continue;
            }

            var candles = BybitCandle.FromResponse(resp)
                .OrderBy(c => c.OpenTime)
                .ToList();

            allCandles.AddRange(candles);

            batchStartIndex += limit;
            Printer.Done();

            await Task.Delay(200); // avoid rate limiting
        }

        var merged = allCandles
            .GroupBy(c => c.OpenTime)
            .Select(g => g.First())
            .OrderBy(c => c.OpenTime)
            .ToList();

        await ParquetService.SaveBybitCandles(merged, filename);
        Printer.Finished();
    }

    private static string[] GetMinuteUrlsByInterval(BacktestingDetails details, DateTime day)
    {
        var dayStart = ToUnix(day);
        var dayEnd = ToUnix(day.AddDays(1));

        return details.Interval switch
        {
            CandleInterval.One_Minute => [
                $"{_endpoint}?category={details.MarketCategory}&symbol={details.Symbol}&interval=1&start={dayStart}&end={ToUnix(day.AddHours(12))}&limit=1000",
                $"{_endpoint}?category={details.MarketCategory}&symbol={details.Symbol}&interval=1&start={ToUnix(day.AddHours(12))}&end={dayEnd}&limit=1000"
            ],
            CandleInterval.Three_Minutes => [$"{_endpoint}?category={details.MarketCategory}&symbol={details.Symbol}&interval={(int)details.Interval}&start={dayStart}&end={dayEnd}&limit=1000"],
            CandleInterval.Five_Minutes => [$"{_endpoint}?category={details.MarketCategory}&symbol={details.Symbol}&interval={(int)details.Interval}&start={dayStart}&end={dayEnd}&limit=1000"],
            CandleInterval.Fifteen_Minutes => [$"{_endpoint}?category={details.MarketCategory}&symbol={details.Symbol}&interval={(int)details.Interval}&start={dayStart}&end={dayEnd}&limit=1000"],
            _ => throw new NotSupportedException($"Interval {details.Interval} not supported."),
        };
    }

    private static long ToUnix(DateTime dt) => new DateTimeOffset(dt).ToUnixTimeMilliseconds();
}

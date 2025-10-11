using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Repositories;
using Cryptobot.ConsoleApp.Utils;
using static Cryptobot.ConsoleApp.Utils.ConsoleColors;

namespace Cryptobot.ConsoleApp.Services;

public static class CandleValidatorService
{
    public static async Task ValidateStoredResources()
    {
        Printer.EmptyLine();
        Printer.WriteLine("__Running Validations__", Cyan);

        await ValidateCandles<BybitCandle>(new BacktestingDetails(CandleInterval.Fifteen_Minutes));
        await ValidateCandles<BybitCandle>(new BacktestingDetails(CandleInterval.One_Day));

        // Validate Moving Average
    }

    private static async Task ValidateCandles<T>(BacktestingDetails details) where T : Candle
    {
        Printer.Write($"  Validating {details.IntervalShortString}..", White);

        var candles = await CandlesRepository.GetCandles<BybitCandle>(details);

        if (candles.Count == 0)
        {
            throw new InvalidOperationException($"No candles found for interval {details.Interval}.");
        }

        // 1️] Check same start and end date consistency
        var startDate = candles.First().OpenTime.Date;
        var endDate = candles.Last().OpenTime.Date;

        if (startDate >= endDate)
        {
            throw new InvalidOperationException($"Invalid date range for {details.Interval}: {startDate} → {endDate}");
        }

        // 2️] Check missing dates
        var allDates = candles.Select(c => c.OpenTime.Date).Distinct().OrderBy(d => d).ToList();
        var expectedDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset))
            .ToList();

        var missingDates = expectedDates.Except(allDates).ToList();
        if (missingDates.Any())
        {
            throw new InvalidOperationException(
                $"Missing {missingDates.Count} days in {details.Interval} data: {missingDates.First():yyyy-MM-dd} → {missingDates.Last():yyyy-MM-dd}");
        }

        var invalidDays = candles
            .GroupBy(c => c.OpenTime.Date)
            .Where(g => g.Count() != details.CandlesticksDailyCount)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToList();

        if (invalidDays.Any())
        {
            var bad = invalidDays.First();

            throw new InvalidOperationException(
                $"Invalid candle count for {details.Interval} on {bad.Date:yyyy-MM-dd}: got {bad.Count}, expected {details.CandlesticksDailyCount}");
        }

        Printer.WriteLine(" Validated!", Green);
    }
}

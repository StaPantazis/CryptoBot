using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Services;
using Cryptobot.ConsoleApp.Utils;

namespace Cryptobot.ConsoleApp.Repositories;

public static class CandlesRepository
{
    public static async Task<List<T>> GetCandles<T>(BacktestingDetails details) where T : BybitCandle, new()
    {
        var resourcesPath = PathHelper.GetHistoryPath(details);
        var files = Directory.GetFiles(resourcesPath, $"*{Constants.PARQUET}").OrderBy(f => f);

        var allCandles = new List<T>();

        foreach (var filepath in files)
        {
            var candles = await ParquetService.LoadCandles<T>(filepath);

            if (candles != null)
            {
                allCandles.AddRange(candles);
            }
        }

        return allCandles;
    }
}

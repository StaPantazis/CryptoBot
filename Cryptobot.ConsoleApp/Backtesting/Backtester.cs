using Cryptobot.ConsoleApp.Backtesting.BudgetStrategies;
using Cryptobot.ConsoleApp.Backtesting.TradeStrategies;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Utils;
using Newtonsoft.Json;

namespace Cryptobot.ConsoleApp.Backtesting;

public static class Backtester
{
    public static void Test(HistoryRequest historyRequest)
    {
        var outputPath = PathHelper.GetBacktestingOutputPath();
        var candles = GetCandlesticks(historyRequest);
        Run(candles);
    }

    public static List<Trade> Run(List<BybitCandle> candles)
    {
        var spot = new Spot(10000, new TradeStrategy_EveryFiveCandles(), new BudgetStrategy_OnePercent());

        for (var i = 0; i < candles.Count; i++)
        {
            var candle = candles[i];

            if (spot.TradeStrategy.ShouldOpenTrade(candles, i))
            {
                spot.OpenTrade(candle);
            }

            spot.CheckCloseTrades(candles, i);
        }

        var trades = spot.Trades.OrderBy(x => x.ExitTime).ToList();
        return trades;
    }

    private static List<BybitCandle> GetCandlesticks(HistoryRequest historyRequest)
    {
        var resourcesPath = PathHelper.GetHistoryPath(historyRequest);
        var files = Directory.GetFiles(resourcesPath, "*.json").OrderBy(f => f).Take(3);

        var allCandles = new List<BybitCandle>();

        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            var candles = JsonConvert.DeserializeObject<List<BybitCandle>>(json);

            if (candles != null)
            {
                allCandles.AddRange(candles);
            }
        }

        return allCandles;
    }
}

using Cryptobot.ConsoleApp.EngineDir.Models;
using System.Text;

namespace Cryptobot.ConsoleApp.Services;

public static class CsvService
{
    public static void ExportTrades(IReadOnlyList<Trade> trades)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine(string.Join(",",
            nameof(Trade.PositionSide),
            nameof(Trade.EntryTime),
            nameof(Trade.ExitTime),
            nameof(Trade.EntryPrice),
            nameof(Trade.ExitPrice),
            nameof(Trade.PnL),
            nameof(Trade.StopLoss),
            nameof(Trade.TakeProfit),
            nameof(Trade.Quantity),
            nameof(Trade.TradeSize),
            nameof(Trade.TradeFees),
            nameof(Trade.SlippageCosts),
            nameof(Trade.AvailableBudgetBeforePlaced),
            nameof(Trade.AvailableBudgetAfterEntry),
            nameof(Trade.AvailableBudgetAfterExit),
            nameof(Trade.FullBudgetOnEntry),
            nameof(Trade.FullBudgetAfterExit)
        ));

        // Rows
        foreach (var t in trades)
        {
            sb.AppendLine(string.Join(",",
                t.PositionSide,
                t.EntryTime.ToString("dd/MM/yyyy HH:mm:ss"),
                t.ExitTime?.ToString("dd/MM/yyyy HH:mm:ss") ?? "",
                t.EntryPrice,
                t.ExitPrice,
                t.PnL,
                t.StopLoss,
                t.TakeProfit,
                t.Quantity,
                t.TradeSize,
                t.TradeFees,
                t.SlippageCosts,
                t.AvailableBudgetBeforePlaced,
                t.AvailableBudgetAfterEntry,
                t.AvailableBudgetAfterExit,
                t.FullBudgetOnEntry,
                t.FullBudgetAfterExit
            ));
        }

        File.WriteAllText(@"C:\Users\stath\Downloads\trades.xlsx", sb.ToString());
    }
}

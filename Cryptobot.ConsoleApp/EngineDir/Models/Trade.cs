using Cryptobot.ConsoleApp.EngineDir.Models.Enums;
using Cryptobot.ConsoleApp.Extensions;

namespace Cryptobot.ConsoleApp.EngineDir.Models;

public class Trade
{
    public required PositionSide PositionSide { get; set; }

    public required DateTime EntryTime { get; set; }
    public required double EntryPrice { get; set; }
    public required string EntryCandleId { get; set; }

    public required double StopLoss { get; set; }
    public required double TakeProfit { get; set; }
    public required double Quantity { get; set; }
    public required double TradeSize { get; set; }
    public required double TradeFees { get; set; }
    public required double SlippageCosts { get; set; }
    public required bool IsClosed { get; set; }
    public required double BudgetBeforePlaced { get; set; }
    public required double BudgetAfterEntry { get; set; }

    public DateTime? ExitTime { get; set; }
    public double? ExitPrice { get; set; }
    public string? ExitCandleId { get; set; }
    public double? PnL { get; set; }
    public double? BudgetAfterExit { get; set; }

    public override string ToString()
    {
        return IsClosed
            ? $"{(PnL > 0 ? "W | +" : "L | ")}{((double)PnL!).Round(2)}€ | Budget: {((double)BudgetAfterExit!).Round(2)}€"
            : $"OPEN | Budget: {BudgetAfterEntry}€";
    }
}

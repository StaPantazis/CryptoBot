using Cryptobot.ConsoleApp.Extensions;

namespace Cryptobot.ConsoleApp.Backtesting;

public class Trade
{
    public DateTime EntryTime { get; set; }
    public double EntryPrice { get; set; }

    public double StopLoss { get; set; }
    public double TakeProfit { get; set; }
    public double Quantity { get; set; }
    public double TradeSize { get; set; }
    public double TradeFees { get; set; }
    public bool IsClosed { get; set; } = false;

    public DateTime? ExitTime { get; set; }
    public double? ExitPrice { get; set; }
    public double? PnL { get; set; }

    public double BudgetBeforePlaced { get; set; }
    public double BudgetAfterPlaced { get; set; }
    public double? BudgetAfterClosed { get; set; }

    public override string ToString()
    {
        return IsClosed
            ? $"{(PnL > 0 ? "W | +" : "L | ")}{((double)PnL!).Round(2)}€ | Budget: {((double)BudgetAfterClosed!).Round(2)}€"
            : $"OPEN | Budget: {BudgetAfterPlaced}€";
    }
}

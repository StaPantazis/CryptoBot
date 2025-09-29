namespace Cryptobot.ConsoleApp.Backtesting.OutputModels;

public record LinearGraphNode(int TradeIndex, double? PnL, double Budget, bool? IsProfit);

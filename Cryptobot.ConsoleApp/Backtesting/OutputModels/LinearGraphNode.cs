namespace Cryptobot.ConsoleApp.Backtesting.OutputModels;

public record LinearGraphNode(int TradeIndex, double Budget, bool? IsOpen, DateTime Timestamp);

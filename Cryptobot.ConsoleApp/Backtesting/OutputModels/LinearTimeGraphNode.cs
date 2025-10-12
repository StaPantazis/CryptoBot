namespace Cryptobot.ConsoleApp.Backtesting.OutputModels;

public record LinearTimeGraphNode(int TradeIndex, double Budget, bool? IsOpen, DateTime Timestamp);

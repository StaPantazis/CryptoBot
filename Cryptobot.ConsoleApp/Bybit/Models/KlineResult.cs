namespace Cryptobot.ConsoleApp.Bybit.Models;

public class KlineResult
{
    public string Symbol { get; set; }
    public string Category { get; set; }

    // Each inner list is: [timestamp, open, high, low, close, volume, turnover]
    public List<List<string>> List { get; set; }
}
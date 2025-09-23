namespace Cryptobot.ConsoleApp.Bybit.Models;

public class KlineResponse
{
    public int RetCode { get; set; }
    public string RetMsg { get; set; }
    public KlineResult Result { get; set; }
    public object RetExtInfo { get; set; }
    public long Time { get; set; }
}
using Cryptobot.ConsoleApp.EngineDir.Models;

namespace Cryptobot.Tests.Models;

public class TestCandle : Candle
{
    private readonly string? _idOverride;

    public TestCandle(string? id = null)
    {
        _idOverride = id;

        if (_idOverride != null)
        {
            Id = _idOverride;
        }
    }

    public override string ToString() => $"{(_idOverride != null ? $"{_idOverride} | " : "")}" +
        $"Entry: {Math.Round(OpenPrice, 2)} - Exit: {Math.Round(ClosePrice, 2)} " +
        $"[L:{Math.Round(LowPrice, 2)}-{Math.Round(HighPrice, 2)}:H]";
}

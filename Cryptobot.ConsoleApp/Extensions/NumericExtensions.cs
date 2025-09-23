namespace Cryptobot.ConsoleApp.Extensions;

public static class NumericExtensions
{
    public static double Round(this double value, int digits = 2) => Math.Round(value, digits);
}

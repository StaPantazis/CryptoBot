namespace Cryptobot.ConsoleApp.Extensions;

public static class NumericExtensions
{
    public static double Round(this double value, int digits = 2) => Math.Round(value, digits);
    public static string Euro(this double value, int digits = 2, bool? plusIfPositive = null) => $"{(plusIfPositive is true ? value.PlusIfPositive() : string.Empty)}{value.Round(digits)}€";
    public static string PlusIfPositive(this double value) => value >= 0 ? "+" : string.Empty;
    public static string MillisecondsToFormattedTime(this int milliseconds) => MillisecondsToFormattedTime((long)milliseconds);
    public static string MillisecondsToFormattedTime(this double milliseconds) => MillisecondsToFormattedTime((long)milliseconds);
    public static string MillisecondsToFormattedTime(this long milliseconds)
    {
        var seconds = ((double)milliseconds / 1000).Round(0);

        return milliseconds switch
        {
            _ when milliseconds < 1000 => $"{milliseconds}ms",
            _ when milliseconds < 60000 => $"{seconds}s",
            _ when milliseconds < 3600000 => $"{Math.Floor(seconds / 60).Round(0)}m {seconds % 60}s",
            _ => $"{Math.Floor(seconds / 3600).Round(0)}h {Math.Floor(Math.Floor(seconds / 3600).Round(0) / 60).Round(0)}m {seconds % 60}s",
        };
    }
}

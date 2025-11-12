namespace Cryptobot.ConsoleApp.Extensions;

public static class DateTimeExtensions
{
    public static long BuildDayKey(this DateTime date) => date.Date.Ticks;

    public static long BuildFifteenMinuteKey(this DateTime date) => (int)TimeSpan.FromTicks(date.Ticks).TotalMinutes / 15;
}
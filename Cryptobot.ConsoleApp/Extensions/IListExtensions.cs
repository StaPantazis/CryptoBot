namespace Cryptobot.ConsoleApp.Extensions;

public static class IListExtensions
{
    public static void ForEachDo<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
        }
    }
}

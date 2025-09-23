using Cryptobot.ConsoleApp.Bybit.Models;

namespace Cryptobot.ConsoleApp.Utils;

public class PathHelper
{
    public static string GetResourcesPath()
    {
        var resourcesDir = @"Cryptobot.ConsoleApp\Resources";

        var current = Directory.GetCurrentDirectory();
        var parentDir = Directory.GetParent(current);

        while (parentDir!.Name != "Cryptobot")
        {
            parentDir = parentDir.Parent;
        }

        var resourcesPath = Path.Combine(parentDir.FullName, resourcesDir);

        CreateDirectoryNonexistent(resourcesPath);
        return resourcesPath;
    }

    /// <summary>
    /// First Symbol --> Then Category --> Then Interval
    /// </summary>
    public static string GetHistoryPath(HistoryRequest historyRequest)
    {
        var resourcesPath = GetResourcesPath();
        var finalPath = Path.Combine(
            resourcesPath,
            historyRequest.MarketCategoryDescribed,
            historyRequest.SymbolDescribed,
            historyRequest.IntervalShortString);

        CreateDirectoryNonexistent(finalPath);
        return finalPath;
    }

    private static void CreateDirectoryNonexistent(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }
}

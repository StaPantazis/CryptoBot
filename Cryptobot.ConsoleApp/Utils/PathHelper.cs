using Cryptobot.ConsoleApp.Backtesting;

namespace Cryptobot.ConsoleApp.Utils;

public class PathHelper
{
    public static string GetBacktestingOutputPath()
    {
        var outputDir = @"Cryptobot.ConsoleApp\Backtesting\Output";
        var outputPath = Path.Combine(GetParentDir().FullName, outputDir);

        CreateDirectoryNonexistent(outputPath);
        return outputPath;
    }

    public static string GetResourcesPath()
    {
        var resourcesDir = @"Cryptobot.ConsoleApp\Resources";
        var resourcesPath = Path.Combine(GetParentDir().FullName, resourcesDir);

        CreateDirectoryNonexistent(resourcesPath);
        return resourcesPath;
    }

    /// <summary>
    /// First Symbol --> Then Category --> Then Interval
    /// </summary>
    public static string GetHistoryPath(BacktestingDetails details)
    {
        var resourcesPath = GetResourcesPath();
        var finalPath = Path.Combine(
            resourcesPath,
            details.MarketCategoryDescribed,
            details.SymbolDescribed,
            details.IntervalShortString);

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

    private static DirectoryInfo GetParentDir()
    {
        var current = Directory.GetCurrentDirectory();
        var parentDir = Directory.GetParent(current);

        while (parentDir!.Name != "Cryptobot")
        {
            parentDir = parentDir.Parent;
        }

        return parentDir;
    }
}

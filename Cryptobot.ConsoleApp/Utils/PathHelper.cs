using Cryptobot.ConsoleApp.Backtesting;

namespace Cryptobot.ConsoleApp.Utils;

public class PathHelper
{
    public static string GetBacktestingOutputPath()
    {
        var outputPath = Path.Combine(GetBasePath(), "Backtesting\\Output\\Backtest");
        CreateDirectoryNonexistent(outputPath);
        return outputPath;
    }

    public static string GetCsvOutputPath()
    {
        var outputPath = Path.Combine(GetBasePath(), "Backtesting\\Output\\Csv");
        CreateDirectoryNonexistent(outputPath);
        return outputPath;
    }

    public static string GetTrendProfilingOutputPath()
    {
        var outputPath = Path.Combine(GetBasePath(), "Backtesting\\Output\\TrendProfiling");
        CreateDirectoryNonexistent(outputPath);
        return outputPath;
    }

    public static string GetCachedIndicatorsOutputPath()
    {
        var outputPath = Path.Combine(GetResourcesPath(), "CachedIndicators");
        CreateDirectoryNonexistent(outputPath);
        return outputPath;
    }

    public static string GetResourcesPath()
    {
        var resourcesPath = Path.Combine(GetBasePath(), "Resources");
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

    public static void DeleteFileIfExisting(string filepath)
    {
        if (File.Exists(filepath))
        {
            File.Delete(filepath);
        }
    }

    public static void CheckFixFilepathExtensions(ref string filepath, string extension)
        => filepath = filepath.Contains(extension) ? filepath : $"{filepath}.{extension}";

    private static string GetBasePath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory.Split("Cryptobot.ConsoleApp")[0], "Cryptobot.ConsoleApp");
    }

    private static void CreateDirectoryNonexistent(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }
}

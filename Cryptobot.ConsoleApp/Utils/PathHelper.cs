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

    private static void CreateDirectoryNonexistent(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
    }
}

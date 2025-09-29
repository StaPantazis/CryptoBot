using System.IO.Compression;
using System.Text;

namespace Cryptobot.ConsoleApp.Utils;

public static class ZipHelper
{
    public static void CompressToGzip(string input, string outputFilePath)
    {
        PathHelper.CheckFixFilepathExtensions(ref outputFilePath, Constants.GZIP);

        var bytes = Encoding.UTF8.GetBytes(input);

        using var fileStream = File.Create(outputFilePath);
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        gzipStream.Write(bytes, 0, bytes.Length);
    }

    public static void BundleFiles(string zipFilepath, params string[] filespaths)
    {
        PathHelper.CheckFixFilepathExtensions(ref zipFilepath, Constants.ZIP);

        PathHelper.DeleteFileIfExisting(zipFilepath);

        using var zip = ZipFile.Open(zipFilepath, ZipArchiveMode.Create);

        foreach (var filepath in filespaths)
        {
            zip.CreateEntryFromFile(filepath, Path.GetFileName(filepath));
        }
    }
}

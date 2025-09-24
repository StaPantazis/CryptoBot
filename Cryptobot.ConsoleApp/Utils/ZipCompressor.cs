using System.IO.Compression;
using System.Text;

namespace Cryptobot.ConsoleApp.Utils;

public static class ZipCompressor
{
    public static void CompressToGzip(string input, string outputFilePath)
    {
        var bytes = Encoding.UTF8.GetBytes(input);

        outputFilePath += ".gz";

        using var fileStream = File.Create(outputFilePath);
        using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
        gzipStream.Write(bytes, 0, bytes.Length);
    }
}

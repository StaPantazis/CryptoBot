using System.IO.Compression;

namespace Cryptobot.ConsoleApp.Binance;

public static class BinanceHistory
{
    private const string _futuresUrl = @"https://data.binance.vision/data/futures/um/daily/klines/BTCUSDT/1m/BTCUSDT-1m-[date].zip";
    private const string _resourcesDir = @"C:\Users\stath\Documents\Coding\GIT\Cryptobot\Cryptobot\Cryptobot.Console\Resources\";

    public static async Task DownloadHistory()
    {
        Console.ForegroundColor = ConsoleColor.White;
        var date = Convert.ToDateTime("01/01/2020");

        while (date < DateTime.Now.AddDays(-1))
        {
            Console.ForegroundColor = ConsoleColor.White;
            var url = _futuresUrl.Replace("[date]", date.ToString("yyyy-MM-dd"));
            Console.Write("Checking ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{date:dd/MM/yyyy}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("... ");

            if (!FileExists(url, out var filePath))
            {
                Console.Write("Downloading... ");

                if (!await DownloadFile(url))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Could not download . ");
                    continue;
                }

                Console.Write("Validating file... ");

                if (IsDataValid(filePath))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Check!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nWrong data for file {filePath}!");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Already farmed!");
            }

            date = date.AddDays(1);
        }

        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void VerifyDataValidity()
    {
        var filePaths = Directory.EnumerateFiles(_resourcesDir, "*.csv").ToArray();
        var success = 0;
        var fails = new List<string>();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Validating data...");
        Console.ForegroundColor = ConsoleColor.Green;

        foreach (var filePath in filePaths)
        {
            if (IsDataValid(filePath))
            {
                Console.Write(".");
                success += 1;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(".");
                Console.ForegroundColor = ConsoleColor.Green;
                fails.Add(filePath);
            }
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(success);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"/{filePaths.Length}");

        if (success < filePaths.Length)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Fails:");

            foreach (var fail in fails)
            {
                Console.WriteLine(fail.Split(@"\", StringSplitOptions.RemoveEmptyEntries).Last());
            }
        }

        Console.ForegroundColor = ConsoleColor.White;
    }

    private static bool FileExists(string url, out string filePath)
    {
        var name = url.Split('/').Last().Replace("zip", "csv");
        filePath = ResourceFileName(name);

        if (!File.Exists(filePath))
        {
            filePath = filePath.Replace("csv", "json");
            return File.Exists(filePath);
        }

        return true;
    }

    private static async Task<bool> DownloadFile(string url)
    {
        var tempZip = ResourceFileName("temp.zip");

        using (var client = new HttpClient())
        using (var response = await client.GetAsync(url))
        using (var stream = await response.Content.ReadAsStreamAsync())
        using (var file = File.OpenWrite(tempZip))
        {
            stream.CopyTo(file);
        }

        try
        {
            ZipFile.ExtractToDirectory(tempZip, _resourcesDir);
            File.Delete(tempZip);

            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    private static bool IsDataValid(string filePath)
    {
        var lines = File.ReadAllLines(filePath);

        if (!char.IsDigit(lines[0][0]))
        {
            lines = lines[1..];
        }

        if (lines.Length != 1440)
        {
            return false;
        }

        try
        {
            _ = ReadRawData(lines);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static BinanceCandlestick[] ReadRawData(string[] rows)
    {
        return rows
            .Select(x => x.Split(',', StringSplitOptions.RemoveEmptyEntries) is string[] arr ? new BinanceCandlestick()
            {
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(arr[0])).UtcDateTime,
                OpenPrice = double.Parse(arr[1]),
                ClosePrice = double.Parse(arr[2]),
                HighPrice = double.Parse(arr[3]),
                LowPrice = double.Parse(arr[4]),
                Volume = double.Parse(arr[5]),
                CloseTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(arr[6])).UtcDateTime,
                QuoteVolume = double.Parse(arr[7]),
                Count = int.Parse(arr[8]),
                TakerBuyVolume = double.Parse(arr[9]),
                TakerBuyQuoteVolume = double.Parse(arr[10]),
                Ignore = arr[11] == "1",
            } : throw new NullReferenceException())
            .ToArray();
    }

    private static string ResourceFileName(string name) => @$"{_resourcesDir}\{name}".Replace(@"\\", @"\");
}

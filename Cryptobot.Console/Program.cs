using Cryptobot.ConsoleApp;

await BinanceHistory.DownloadHistory();
BinanceHistory.VerifyDataValidity();

Console.ReadLine();

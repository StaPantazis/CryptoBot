using Cryptobot.ConsoleApp;
using Cryptobot.ConsoleApp.Services;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var cacheManager = new CacheService();
await cacheManager.InitializeCache();

await Consoler.Run(cacheManager);

using Cryptobot.ConsoleApp;
using Cryptobot.ConsoleApp.Services;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var cache = new CacheService();
await cache.InitializeCache();

await Consoler.Run(cache);

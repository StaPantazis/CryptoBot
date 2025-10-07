using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.Utils;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var cacheManager = new CacheManager();
await cacheManager.InitializeCache();

await Consoler.Run(cacheManager);

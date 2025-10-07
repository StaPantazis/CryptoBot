using Cryptobot.ConsoleApp.EngineDir;
using Cryptobot.ConsoleApp.Utils;

var cacheManager = new CacheManager();
await cacheManager.InitializeAsync();

await Consoler.Run(cacheManager);

using Cryptobot.ConsoleApp;
using Cryptobot.ConsoleApp.Services;
using System.Text;

var budget = 10000d;
var totalTrades = 2245;
var winRate = 58.51;

var wonTrades = (int)(totalTrades * winRate / 100);
var lostTrades = totalTrades - wonTrades;
var diffTrades = wonTrades - lostTrades; // 381 = net wins

for (var i = 0; i < diffTrades; i++)
{
    budget *= 1.03;
}

var fees = 6391058;

var profit = budget - fees;

Console.OutputEncoding = Encoding.UTF8;

var cacheManager = new CacheService();
await cacheManager.InitializeCache();

await Consoler.Run(cacheManager);

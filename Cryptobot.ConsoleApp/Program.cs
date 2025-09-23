using Cryptobot.ConsoleApp.Bybit;
using Cryptobot.ConsoleApp.Bybit.Enums;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Utils;

var request = new HistoryRequest(CandlestickInterval.Five_Minutes, Constants.SYMBOL_BTCUSDT, Constants.MARKET_PERPETUAL_FUTURES);
await BybitHistory.Download(request);

Console.ReadLine();

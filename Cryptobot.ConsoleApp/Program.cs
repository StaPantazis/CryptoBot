using Cryptobot.ConsoleApp.Backtesting;
using Cryptobot.ConsoleApp.Bybit.Enums;
using Cryptobot.ConsoleApp.Bybit.Models;
using Cryptobot.ConsoleApp.Utils;

var request = new HistoryRequest(CandleInterval.Five_Minutes, Constants.SYMBOL_BTCUSDT, Constants.MARKET_PERPETUAL_FUTURES);
//await BybitHistory.Download(request);
Backtester.Test(request);

Console.ReadLine();

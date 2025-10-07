using Cryptobot.ConsoleApp.EngineDir.Models.Enums;

namespace Cryptobot.ConsoleApp.Resources.CachedIndicators;

public record CachedMacroTrend(DateTime OpenTime, double? MA11520, Trend Trend);
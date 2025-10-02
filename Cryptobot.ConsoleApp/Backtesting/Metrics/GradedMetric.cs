using Cryptobot.ConsoleApp.Extensions;

namespace Cryptobot.ConsoleApp.Backtesting.Metrics;

public class GradedMetric<T>(T value)
{
    public T Value { get; private set; } = value;
    public Grade Grade { get; set; }

    public static implicit operator GradedMetric<T>(T value) => new(value);

    public static implicit operator T(GradedMetric<T> graded) => graded.Value;
    public static implicit operator Grade(GradedMetric<T> graded) => graded.Grade;

    public override string ToString() => $"{Value} {Grade.GetDisplayName()}";
}
using Cryptobot.Tests.Mocks;
using FluentAssertions;

namespace Cryptobot.Tests;

public class SpotTests
{
    [Fact]
    public void Test1()
    {
        var spot = Factory.Spot;
        spot.Should().NotBeNull();
    }
}

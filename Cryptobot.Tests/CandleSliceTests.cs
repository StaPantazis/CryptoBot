using Cryptobot.ConsoleApp.EngineDir.Models;
using Cryptobot.Tests.Builders;
using FluentAssertions;

namespace Cryptobot.Tests;

public class CandleSliceTests
{
    [Fact]
    public void Should_initialize_with_existing_candles()
    {
        var candles = A.Candles(3);
        var slice = new CandleSlice<Candle>(candles);

        slice.Candles.Should().BeSameAs(candles);
        slice.LiveCandle.Should().BeNull();
        slice.LastCandle.Should().BeNull();
    }

    [Fact]
    public void Should_add_first_live_candle()
    {
        var slice = new CandleSlice<Candle>(3);
        var c1 = A.Candle(1);

        slice.AddLiveCandle(c1);

        slice.LiveCandle.Should().Be(c1);
        slice.LastCandle.Should().BeNull();
        slice.Candles.Should().BeEmpty();
    }

    [Fact]
    public void Should_move_live_to_history_and_update_last()
    {
        var slice = new CandleSlice<Candle>(3);

        var c1 = A.Candle(1);
        var c2 = A.Candle(2);

        slice.AddLiveCandle(c1);
        slice.AddLiveCandle(c2);

        slice.LastCandle.Should().Be(c1);
        slice.LiveCandle.Should().Be(c2);
        slice.Candles.Should().ContainSingle().Which.Should().Be(c1);
    }

    [Fact]
    public void Should_trim_history_when_capacity_exceeded()
    {
        var slice = new CandleSlice<Candle>(2);

        var c1 = A.Candle(1);
        var c2 = A.Candle(2);
        var c3 = A.Candle(3);

        slice.AddLiveCandle(c1);
        slice.AddLiveCandle(c2);
        slice.AddLiveCandle(c3);

        slice.Candles.Should().HaveCount(2);
        slice.Candles[0].Id.Should().Be(c1.Id);
        slice.Candles[1].Id.Should().Be(c2.Id);
        slice.LiveCandle.Should().Be(c3);
    }

    [Fact]
    public void Should_keep_last_and_live_correct_when_many_updates()
    {
        var slice = new CandleSlice<Candle>(3);

        var c1 = A.Candle(1);
        var c2 = A.Candle(2);
        var c3 = A.Candle(3);
        var c4 = A.Candle(4);

        slice.AddLiveCandle(c1);
        slice.AddLiveCandle(c2);
        slice.AddLiveCandle(c3);
        slice.AddLiveCandle(c4);

        slice.LastCandle.Should().Be(c3);
        slice.LiveCandle.Should().Be(c4);
        slice.Candles.Should().ContainInOrder(c1, c2, c3).And.HaveCount(3);
    }

    [Fact]
    public void Should_return_slice_of_correct_size()
    {
        var slice = new CandleSlice<Candle>(5);

        slice.AddLiveCandle(A.Candle(1));
        slice.AddLiveCandle(A.Candle(2));
        slice.AddLiveCandle(A.Candle(3));
        slice.AddLiveCandle(A.Candle(4));
        slice.AddLiveCandle(A.Candle(5));

        var s = slice.GetSlice(2);

        s.Candles.Should().HaveCount(2);
        s.Candles[0].Id.Should().Be("3");
        s.Candles[1].Id.Should().Be("4");
    }

    [Fact]
    public void Should_throw_when_requested_slice_too_large()
    {
        var slice = new CandleSlice<Candle>(3);
        slice.AddLiveCandle(A.Candle(1));

        Action act = () => slice.GetSlice(10);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}

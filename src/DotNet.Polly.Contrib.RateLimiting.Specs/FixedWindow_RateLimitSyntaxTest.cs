using System.Threading.RateLimiting;
using Polly.RateLimit;
using FluentAssertions;
using Polly;

namespace DotNet.Polly.Contrib.RateLimiting.Tests;

public class FixedWindow_RateLimitSyntaxTest : RateLimitSyntaxBaseTest
{
    [Fact]
    public override void Should_throw_when_option_is_null()
    {
        // Arrange
        var invalidSyntax = () => RateLimit.FixedWindowRateLimit(options: null!);

        // Act and Assert
        invalidSyntax.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("options");
    }

    [Fact]
    public override void Should_throw_when_configure_option_is_null()
    {
        // Arrange
        var invalidSyntax = () => RateLimit.FixedWindowRateLimit(configureOptions: null!);

        // Act and Assert
        invalidSyntax.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("configureOptions");
    }

    [Fact]
    public override void Given_limiter_with_one_permit_should_acquire_lease()
    {
        // Arrange
        var rateLimiter = RateLimit.FixedWindowRateLimit(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1,
            AutoReplenishment = false,
            Window = TimeSpan.FromSeconds(2)
        });

        // Act
        var result = TryExecutePolicy(rateLimiter);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public override void Given_limiter_with_one_permit_throw_rate_limit_exception_for_second_request()
    {
        // Arrange
        var rateLimiter = RateLimit.FixedWindowRateLimit(new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1,
            AutoReplenishment = false,
            Window = TimeSpan.FromSeconds(2)
        });

        // Act
        var result1 = TryExecutePolicy(rateLimiter);
        var exceededRequest = () => TryExecutePolicy(rateLimiter);

        // Assert
        result1.Should().Be(true);
        exceededRequest.Should().Throw<RateLimitRejectedException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(50)]
    public override void Given_limiter_with_N_permit_throw_rate_limit_exception_for_N_plus_1_th_request(int permitLimit)
    {
        // Arrange
        var rateLimiter = RateLimit.FixedWindowRateLimit(new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            AutoReplenishment = false,
            Window = TimeSpan.FromSeconds(2)
        });

        // Act
        var results = new bool[permitLimit];
        for (int index = 0; index < permitLimit; index++)
        {
            results[index] = TryExecutePolicy(rateLimiter);
        }
        var exceededRequest = () => TryExecutePolicy(rateLimiter);

        // Assert
        results.Should().AllBeEquivalentTo(true);
        exceededRequest.Should().Throw<RateLimitRejectedException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(50)]
    public override void Given_limiter_with_N_permit_throw_rate_limit_exception_for_N_plus_1_th_request_and_acquire_for_next_N_th_after_replenishment(int permitLimit)
    {
        // Arrange
        ReplenishingRateLimiter rateLimiter = null!;
        var rateLimiterPolicy = RateLimit.FixedWindowRateLimit(
            option =>
            {
                option.PermitLimit = permitLimit;
                option.QueueLimit = 0;
                option.AutoReplenishment = false;
                option.Window = TimeSpan.FromMilliseconds(1);
            },
            limiter =>
            {
                rateLimiter = limiter;
            });

        // Act
        var results = new bool[permitLimit];
        for (int index = 0; index < permitLimit; index++)
        {
            results[index] = TryExecutePolicy(rateLimiterPolicy);
        }
        var exceededRequest = () => TryExecutePolicy(rateLimiterPolicy);

        // Assert
        results.Should().AllBeEquivalentTo(true);
        exceededRequest.Should().Throw<RateLimitRejectedException>();

        Task.Delay(2).GetAwaiter().GetResult();
        rateLimiter!.TryReplenish();

        // Act
        var nextResults = new bool[permitLimit];
        for (int index = 0; index < permitLimit; index++)
        {
            nextResults[index] = TryExecutePolicy(rateLimiterPolicy);
        }

        // Assert
        nextResults.Should().AllBeEquivalentTo(true);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(100)]
    public override void Given_immediate_parallel_contention_limiter_still_only_permits_one(int parallelContention)
    {
        // Arrange
        var rateLimiterPolicy = RateLimit.FixedWindowRateLimit(
            option =>
            {
                option.PermitLimit = 1;
                option.QueueLimit = 0;
                option.AutoReplenishment = false;
                option.Window = TimeSpan.FromMilliseconds(1);
            });

        // Act
        var tasks = new Task<bool>[parallelContention];
        ManualResetEventSlim gate = new();
        for (int i = 0; i < tasks.Length; i++)
        {
            int index = i;
            tasks[index] = Task.Run<bool>(() =>
            {
                try
                {
                    gate.Wait();
                    return TryExecutePolicy(rateLimiterPolicy);
                }
                catch (RateLimitRejectedException exception)
                {
                    return false;
                }
            });
        }
        gate.Set();
        Task.WhenAll(tasks);

        // Assert
        var results = tasks.Select(t => t.Result).ToList();
        results.Count(x => x).Should().Be(1);
        results.Count(x => !x).Should().Be(parallelContention - 1);
    }
}
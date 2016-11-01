namespace LimitsMiddleware.Tests.RateLimiters
{
    using System;
    using System.Threading.Tasks;
    using LimitsMiddleware.RateLimiters;
    using Shouldly;
    using Xunit;

    public class FixedTokenBucketTests
    {
        private const int MAX_TOKENS = 10;
        private const long REFILL_INTERVAL = 10;
        private const int N_LESS_THAN_MAX = 2;
        private const int N_GREATER_THAN_MAX = 12;
        private const int CUMULATIVE = 2;
        private readonly FixedTokenBucket _bucket;
        private GetUtcNow _getUtcNow = () => SystemClock.GetUtcNow();

        public FixedTokenBucketTests()
        {
            _bucket = new FixedTokenBucket(() => MAX_TOKENS, () => _getUtcNow());
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNTokensLessThanMax_ReturnsFalse()
        {
            TimeSpan waitTime;
            bool shouldThrottle = _bucket.ShouldThrottle(N_LESS_THAN_MAX, out waitTime);

            shouldThrottle.ShouldBeFalse();
            _bucket.CurrentTokenCount.ShouldBe(MAX_TOKENS - N_LESS_THAN_MAX);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledCumulativeNTimesIsLessThanMaxTokens_ReturnsFalse()
        {
            for (int i = 0; i < CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_LESS_THAN_MAX).ShouldBeFalse();
            }

            long tokens = _bucket.CurrentTokenCount;

            tokens.ShouldBe(MAX_TOKENS - (CUMULATIVE*N_LESS_THAN_MAX));
        }

        [Fact]
        public void ShouldThrottle_WhenCalledCumulativeNTimesIsGreaterThanMaxTokens_ReturnsTrue()
        {
            for (int i = 0; i < CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_GREATER_THAN_MAX).ShouldBeTrue();
            }

            long tokens = _bucket.CurrentTokenCount;

            tokens.ShouldBe(MAX_TOKENS);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNLessThanMaxSleepNLessThanMax_ReturnsFalse()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            bool before = _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            long tokensBefore = _bucket.CurrentTokenCount;
            before.ShouldBeFalse();
            tokensBefore.ShouldBe(MAX_TOKENS - N_LESS_THAN_MAX);

            _getUtcNow = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            bool after = _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            long tokensAfter = _bucket.CurrentTokenCount;

            after.ShouldBeFalse();
            tokensAfter.ShouldBe(MAX_TOKENS - N_LESS_THAN_MAX);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNGreaterThanMaxSleepNGreaterThanMax_ReturnsTrue()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            bool before = _bucket.ShouldThrottle(N_GREATER_THAN_MAX);
            long tokensBefore = _bucket.CurrentTokenCount;

            before.ShouldBeTrue();
            tokensBefore.ShouldBe(MAX_TOKENS);

            _getUtcNow = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            bool after = _bucket.ShouldThrottle(N_GREATER_THAN_MAX);
            long tokensAfter = _bucket.CurrentTokenCount;
            after.ShouldBeTrue();
            tokensAfter.ShouldBe(MAX_TOKENS);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNLessThanMaxSleepCumulativeNLessThanMax()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            long sum = 0;
            for (int i = 0; i < CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_LESS_THAN_MAX).ShouldBeFalse();
                sum += N_LESS_THAN_MAX;
            }
            long tokensBefore = _bucket.CurrentTokenCount;
            tokensBefore.ShouldBe(MAX_TOKENS - sum);

            _getUtcNow = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            for (int i = 0; i < CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_LESS_THAN_MAX).ShouldBeFalse();
            }
            long tokensAfter = _bucket.CurrentTokenCount;
            tokensAfter.ShouldBe(MAX_TOKENS - sum);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithCumulativeNLessThanMaxSleepCumulativeNGreaterThanMax()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            long sum = 0;
            for (int i = 0; i < CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_LESS_THAN_MAX).ShouldBeFalse();
                sum += N_LESS_THAN_MAX;
            }
            long tokensBefore = _bucket.CurrentTokenCount;
            tokensBefore.ShouldBe(MAX_TOKENS - sum);

            _getUtcNow = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            for (int i = 0; i < 3*CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            }

            bool after = _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            long tokensAfter = _bucket.CurrentTokenCount;

            after.ShouldBeTrue();
            tokensAfter.ShouldBeLessThan(N_LESS_THAN_MAX);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithCumulativeNGreaterThanMaxSleepCumulativeNLessThanMax()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            for (int i = 0; i < 3*CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            }

            bool before = _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            long tokensBefore = _bucket.CurrentTokenCount;

            before.ShouldBeTrue();
            tokensBefore.ShouldBeLessThan(N_LESS_THAN_MAX);

            _getUtcNow = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            long sum = 0;
            for (int i = 0; i < CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_LESS_THAN_MAX).ShouldBeFalse();
                sum += N_LESS_THAN_MAX;
            }

            long tokensAfter = _bucket.CurrentTokenCount;
            tokensAfter.ShouldBe(MAX_TOKENS - sum);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithCumulativeNGreaterThanMaxSleepCumulativeNGreaterThanMax()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            for (int i = 0; i < 3*CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            }

            bool before = _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            long tokensBefore = _bucket.CurrentTokenCount;

            before.ShouldBeTrue();
            tokensBefore.ShouldBeLessThan(N_LESS_THAN_MAX);

            _getUtcNow = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            for (int i = 0; i < 3*CUMULATIVE; i++)
            {
                _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            }
            bool after = _bucket.ShouldThrottle(N_LESS_THAN_MAX);
            long tokensAfter = _bucket.CurrentTokenCount;

            after.ShouldBeTrue();
            tokensAfter.ShouldBeLessThan(N_LESS_THAN_MAX);
        }

        [Fact]
        public async Task ShouldThrottle_WhenThread1NLessThanMaxAndThread2NLessThanMax()
        {
            var task1 = Task.Run(() => 
            {
                bool throttle = _bucket.ShouldThrottle(N_LESS_THAN_MAX);
                throttle.ShouldBeFalse();
            });

            var task2 = Task.Run(() =>
            {
                bool throttle = _bucket.ShouldThrottle(N_LESS_THAN_MAX);
                throttle.ShouldBeFalse();
            });

            await Task.WhenAll(task1, task2);

            _bucket.CurrentTokenCount.ShouldBe(MAX_TOKENS - 2*N_LESS_THAN_MAX);
        }

        [Fact]
        public async Task ShouldThrottle_Thread1NGreaterThanMaxAndThread2NGreaterThanMax()
        {
            bool shouldThrottle = _bucket.ShouldThrottle(N_GREATER_THAN_MAX);
            shouldThrottle.ShouldBeTrue();

            var task1 = Task.Run(() =>
            {
                bool throttle = _bucket.ShouldThrottle(N_GREATER_THAN_MAX);
                throttle.ShouldBeTrue();
            });

            var task2 = Task.Run(() =>
            {
                bool throttle = _bucket.ShouldThrottle(N_GREATER_THAN_MAX);
                throttle.ShouldBeTrue();
            });

            await Task.WhenAll(task1, task2);

            _bucket.CurrentTokenCount.ShouldBe(MAX_TOKENS);
        }
    }
}
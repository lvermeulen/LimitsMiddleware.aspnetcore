using LimitsMiddleware.Logging.LogProviders;

namespace LimitsMiddleware.RateLimiters
{
    using System;
    using System.Threading;

    internal class FixedTokenBucket
    {
        private readonly Func<int> _getBucketTokenCapacty;
        private readonly long _refillIntervalTicks;
        private readonly TimeSpan _refillInterval;
        private readonly GetUtcNow _getUtcNow;
        private long _nextRefillTime;
        private long _tokens;
        private readonly InterlockedBoolean _updatingTokens = new InterlockedBoolean();
        private int _concurrentRequestCount;

        public FixedTokenBucket(Func<int> getBucketTokenCapacty, GetUtcNow getUtcNow = null)
        {
            _getBucketTokenCapacty = getBucketTokenCapacty;
            _refillInterval = TimeSpan.FromSeconds(1);
            _refillIntervalTicks = TimeSpan.FromSeconds(1).Ticks;
            _getUtcNow = getUtcNow ?? SystemClock.GetUtcNow;
        }

        public bool ShouldThrottle(int tokenCount)
        {
            TimeSpan _;
            return ShouldThrottle(tokenCount, out _);
        }

        public bool ShouldThrottle(int tokenCount, out TimeSpan waitTimeSpan)
        {
            waitTimeSpan = TimeSpan.Zero;
            UpdateTokens();
            long tokens = Interlocked.Read(ref _tokens);
            if (tokens < tokenCount)
            {
                long currentTime = _getUtcNow().Ticks;
                long waitTicks = _nextRefillTime - currentTime;
                if (waitTicks <= 0)
                {
                    return false;
                }
                waitTimeSpan = TimeSpan.FromTicks(waitTicks * _concurrentRequestCount);
                return true;
            }
            Interlocked.Add(ref _tokens, -tokenCount);
            return false;
        }

        public long CurrentTokenCount
        {
            get
            {
                UpdateTokens();
                return Interlocked.Read(ref _tokens);
            }
        }

        public int Capacity => _getBucketTokenCapacty();

        public double Rate => Capacity/_refillInterval.TotalSeconds;

        public IDisposable RegisterRequest()
        {
            Interlocked.Increment(ref _concurrentRequestCount);
            return new DisposableAction(() =>
            {
                Interlocked.Decrement(ref _concurrentRequestCount);
            });
        }

        private void UpdateTokens()
        {
            if (_updatingTokens.EnsureCalledOnce())
            {
                return;
            }
            long currentTime = _getUtcNow().Ticks;

            if (currentTime >= _nextRefillTime)
            {
                Interlocked.Exchange(ref _tokens, _getBucketTokenCapacty());
                _nextRefillTime = currentTime + _refillIntervalTicks;
            }

            _updatingTokens.Set(false);
        }
    }
}
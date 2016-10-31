// https://github.com/robertmircea/RateLimiters

namespace LimitsMiddleware.RateLimiters
{
    using System;

    internal static class SystemClock
    {
        internal static readonly GetUtcNow GetUtcNow = () => DateTimeOffset.UtcNow;
    }
}
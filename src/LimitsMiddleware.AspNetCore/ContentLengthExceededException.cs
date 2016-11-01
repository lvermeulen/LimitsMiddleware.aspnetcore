// https://github.com/robertmircea/RateLimiters

namespace LimitsMiddleware
{
    using System;

    internal class ContentLengthExceededException : Exception
    {
        public ContentLengthExceededException(string message) : base(message)
        {}
    }
}
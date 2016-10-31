using System;
using System.IO;
using LimitsMiddleware.Logging;
using LimitsMiddleware.RateLimiters;
using Microsoft.AspNetCore.Http;

namespace LimitsMiddleware
{
    using MidFunc = Func<RequestDelegate, RequestDelegate>;

    public static partial class Limits
    {
        /// <summary>
        ///     Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="maxBytesPerSecond">
        ///     The maximum number of bytes per second to be transferred. Use 0 or a negative
        ///     number to specify infinite bandwidth.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxBandwidthGlobal(int maxBytesPerSecond, string loggerName)
        {
            return MaxBandwidthGlobal(() => maxBytesPerSecond, loggerName);
        }

        /// <summary>
        ///     Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getBytesPerSecond">
        ///     A delegate to retrieve the maximum number of bytes per second to be transferred.
        ///     Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesToWrite</exception>
        public static MidFunc MaxBandwidthGlobal(Func<int> getBytesPerSecond, string loggerName = null)
        {
            if (getBytesPerSecond == null)
            {
                throw new ArgumentNullException(nameof(getBytesPerSecond));
            }

            loggerName = string.IsNullOrWhiteSpace(loggerName)
                ? $"{nameof(LimitsMiddleware)}.{nameof(MaxBandwidthGlobal)}"
                : loggerName;
            var logger = LogProvider.GetLogger(loggerName);

            var requestTokenBucket = new FixedTokenBucket(getBytesPerSecond);
            var responseTokenBucket = new FixedTokenBucket(getBytesPerSecond);
            logger.Debug("Configure streams to be globally limited to.");

            return
                next =>
                async context =>
                {
                    // ReSharper disable once ArrangeBraces_using
                    using (requestTokenBucket.RegisterRequest())
                    using (responseTokenBucket.RegisterRequest())
                    {
                        var requestBodyStream = context.Request.Body ?? Stream.Null;
                        var responseBodyStream = context.Response.Body;

                        context.Request.Body = new ThrottledStream(requestBodyStream, requestTokenBucket);
                        context.Response.Body = new ThrottledStream(responseBodyStream, responseTokenBucket);

                        //TODO consider SendFile interception
                        await next(context).ConfigureAwait(false);
                    }
                };
        }
    }
}

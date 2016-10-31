using System;
using System.Threading;
using LimitsMiddleware.Logging;
using Microsoft.AspNetCore.Http;

namespace LimitsMiddleware
{
    using MidFunc = Func<RequestDelegate, RequestDelegate>;

    public static partial class Limits
    {
        /// <summary>
        ///     Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="maxConcurrentRequests">
        ///     The maximum number of concurrent requests. Use 0 or a negative
        ///     number to specify unlimited number of concurrent requests.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxConcurrentRequests(int maxConcurrentRequests, string loggerName = null)
        {
            return MaxConcurrentRequests(() => maxConcurrentRequests, loggerName);
        }

        /// <summary>
        ///     Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxConcurrentRequests">
        ///     A delegate to retrieve the maximum number of concurrent requests. Allows you
        ///     to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent
        ///     requests.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxConcurrentRequests</exception>
        public static MidFunc MaxConcurrentRequests(Func<int> getMaxConcurrentRequests, string loggerName = null)
        {
            if (getMaxConcurrentRequests == null)
            {
                throw new ArgumentNullException(nameof(getMaxConcurrentRequests));
            }

            return MaxConcurrentRequests(_ => getMaxConcurrentRequests(), loggerName);
        }

        /// <summary>
        ///     Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxConcurrentRequests">
        ///     A delegate to retrieve the maximum number of concurrent requests. Allows you
        ///     to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent
        ///     requests.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxConcurrentRequests</exception>
        public static MidFunc MaxConcurrentRequests(Func<RequestContext, int> getMaxConcurrentRequests, string loggerName = null)
        {
            if (getMaxConcurrentRequests == null)
            {
                throw new ArgumentNullException(nameof(getMaxConcurrentRequests));
            }

            loggerName = string.IsNullOrWhiteSpace(loggerName)
                ? $"{nameof(LimitsMiddleware)}.{nameof(MaxConcurrentRequests)}"
                : loggerName;
            var logger = LogProvider.GetLogger(loggerName);
            var concurrentRequestCounter = 0;

            return
                next =>
                async context =>
                {
                    var limitsRequestContext = new RequestContext(context.Request);
                    int maxConcurrentRequests = getMaxConcurrentRequests(limitsRequestContext);
                    if (maxConcurrentRequests <= 0)
                    {
                        maxConcurrentRequests = int.MaxValue;
                    }
                    try
                    {
                        int concurrentRequests = Interlocked.Increment(ref concurrentRequestCounter);
                        logger.Debug($"Concurrent request {concurrentRequests}/{maxConcurrentRequests}.");
                        if (concurrentRequests > maxConcurrentRequests)
                        {
                            logger.Info($"Limit ({maxConcurrentRequests}). Request rejected.");
                            var response = context.Response;
                            response.StatusCode = 503;
                            return;
                        }
                        await next(context);
                    }
                    finally
                    {
                        int concurrentRequests = Interlocked.Decrement(ref concurrentRequestCounter);
                        logger.Debug($"Concurrent request {concurrentRequests}/{maxConcurrentRequests}.");
                    }
                };
        }
    }
}

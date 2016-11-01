using System;
using System.IO;
using LimitsMiddleware.RateLimiters;
using Microsoft.AspNetCore.Http;

namespace LimitsMiddleware
{
    using MidFunc = Func<RequestDelegate, RequestDelegate>;

    public static partial class Limits
    {
        /// <summary>
        ///     Limits the bandwith used by the subsequent stages in the aspnetcore pipeline.
        /// </summary>
        /// <param name="maxBytesPerSecond">
        ///     The maximum number of bytes per second to be transferred. Use 0 or a negative
        ///     number to specify infinite bandwidth.
        /// </param>
        /// <returns>A middleware delegate.</returns>
        public static MidFunc MaxBandwidthPerRequest(int maxBytesPerSecond) => MaxBandwidthPerRequest(() => maxBytesPerSecond);

        /// <summary>
        ///     Limits the bandwith used by the subsequent stages in the aspnetcore pipeline.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">
        ///     A delegate to retrieve the maximum number of bytes per second to be transferred.
        ///     Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.
        /// </param>
        /// <returns>A middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static MidFunc MaxBandwidthPerRequest(Func<int> getMaxBytesPerSecond)
        {
            if (getMaxBytesPerSecond == null)
            {
                throw new ArgumentNullException(nameof(getMaxBytesPerSecond));
            }

            return MaxBandwidthPerRequest(_ => getMaxBytesPerSecond());
        }

        /// <summary>
        ///     Limits the bandwith used by the subsequent stages in the aspnetcore pipeline.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">
        ///     A delegate to retrieve the maximum number of bytes per second to be transferred.
        ///     Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.
        /// </param>
        /// <returns>A middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static MidFunc MaxBandwidthPerRequest(Func<RequestContext, int> getMaxBytesPerSecond)
        {
            if (getMaxBytesPerSecond == null)
            {
                throw new ArgumentNullException(nameof(getMaxBytesPerSecond));
            }

            return
                next =>
                async context =>
                {
                    var requestBodyStream = context.Request.Body ?? Stream.Null;
                    var responseBodyStream = context.Response.Body;

                    var limitsRequestContext = new RequestContext(context.Request);

                    var requestTokenBucket = new FixedTokenBucket(
                        () => getMaxBytesPerSecond(limitsRequestContext));
                    var responseTokenBucket = new FixedTokenBucket(
                        () => getMaxBytesPerSecond(limitsRequestContext));

                    // ReSharper disable once ArrangeBraces_using
                    using (requestTokenBucket.RegisterRequest())
                    using (responseTokenBucket.RegisterRequest())
                    {

                        context.Request.Body = new ThrottledStream(requestBodyStream, requestTokenBucket);
                        context.Response.Body = new ThrottledStream(responseBodyStream, responseTokenBucket);

                        //TODO consider SendFile interception
                        await next(context).ConfigureAwait(false);
                    }
                };
        }
    }
}

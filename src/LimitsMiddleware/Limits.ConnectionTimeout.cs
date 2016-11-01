using System;
using System.IO;
using LimitsMiddleware.Logging;
using Microsoft.AspNetCore.Http;

namespace LimitsMiddleware
{
    using MidFunc = Func<RequestDelegate, RequestDelegate>;

    public static partial class Limits
    {
        /// <summary>
        ///     Timeouts the connection if there hasn't been an read activity on the request body stream or any
        ///     write activity on the response body stream.
        /// </summary>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>A middleware delegate.</returns>
        public static MidFunc ConnectionTimeout(TimeSpan timeout, string loggerName = null) => ConnectionTimeout(() => timeout, loggerName);

        /// <summary>
        ///     Timeouts the connection if there hasn't been an read activity on the request body stream or any
        ///     write activity on the response body stream.
        /// </summary>
        /// <param name="getTimeout">
        ///     A delegate to retrieve the timeout timespan. Allows you
        ///     to supply different values at runtime.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>A middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getTimeout</exception>
        public static MidFunc ConnectionTimeout(Func<TimeSpan> getTimeout, string loggerName = null)
        {
            if (getTimeout == null)
            {
                throw new ArgumentNullException(nameof(getTimeout));
            }

            return ConnectionTimeout(_ => getTimeout(), loggerName);
        }

        /// <summary>
        ///     Timeouts the connection if there hasn't been an read activity on the request body stream or any
        ///     write activity on the response body stream.
        /// </summary>
        /// <param name="getTimeout">
        ///     A delegate to retrieve the timeout timespan. Allows you
        ///     to supply different values at runtime.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>A middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getTimeout</exception>
        public static MidFunc ConnectionTimeout(Func<RequestContext, TimeSpan> getTimeout, string loggerName = null)
        {
            if (getTimeout == null)
            {
                throw new ArgumentNullException(nameof(getTimeout));
            }

            loggerName = string.IsNullOrWhiteSpace(loggerName)
                ? $"{nameof(LimitsMiddleware)}.{nameof(ConnectionTimeout)}"
                : loggerName;

            var logger = LogProvider.GetLogger(loggerName);

            return
                next =>
                context =>
                {
                    var limitsRequestContext = new RequestContext(context.Request);

                    var requestBodyStream = context.Request.Body ?? Stream.Null;
                    var responseBodyStream = context.Response.Body;

                    var connectionTimeout = getTimeout(limitsRequestContext);
                    context.Request.Body = new TimeoutStream(requestBodyStream, connectionTimeout, logger);
                    context.Response.Body = new TimeoutStream(responseBodyStream, connectionTimeout, logger);
                    return next(context);
                };
        }
    }
}

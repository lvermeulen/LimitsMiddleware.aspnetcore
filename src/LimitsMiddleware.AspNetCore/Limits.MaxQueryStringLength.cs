using System;
using System.Net;
using LimitsMiddleware.Logging;
using Microsoft.AspNetCore.Http;

namespace LimitsMiddleware
{
    using MidFunc = Func<RequestDelegate, RequestDelegate>;

    public static partial class Limits
    {
        /// <summary>
        ///     Limits the length of the query string.
        /// </summary>
        /// <param name="maxQueryStringLength">Maximum length of the query string.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>A middleware delegate.</returns>
        public static MidFunc MaxQueryStringLength(int maxQueryStringLength, string loggerName = null) => 
            MaxQueryStringLength(_ => maxQueryStringLength, loggerName);

        /// <summary>
        ///     Limits the length of the query string.
        /// </summary>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>A middleware delegate.</returns>
        public static MidFunc MaxQueryStringLength(Func<int> getMaxQueryStringLength, string loggerName = null) => 
            MaxQueryStringLength(_ => getMaxQueryStringLength(), loggerName);

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>A middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxQueryStringLength</exception>
        public static MidFunc MaxQueryStringLength(Func<RequestContext, int> getMaxQueryStringLength, string loggerName = null)
        {
            if (getMaxQueryStringLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxQueryStringLength));
            }

            loggerName = string.IsNullOrWhiteSpace(loggerName)
                ? $"{nameof(LimitsMiddleware)}.{nameof(MaxQueryStringLength)}"
                : loggerName;
            var logger = LogProvider.GetLogger(loggerName);

            return
                next =>
                async context =>
                {
                    var requestContext = new RequestContext(context.Request);

                    var queryString = context.Request.QueryString;
                    if (queryString.HasValue)
                    {
                        int maxQueryStringLength = getMaxQueryStringLength(requestContext);
                        string unescapedQueryString = Uri.UnescapeDataString(queryString.Value);
                        logger.Debug($"Querystring of request with an unescaped length of {unescapedQueryString.Length}");
                        if (unescapedQueryString.Length > maxQueryStringLength)
                        {
                            logger.Info($"Querystring (Length {unescapedQueryString.Length}) too long (allowed {maxQueryStringLength}). Request rejected.");
                            context.Response.StatusCode = (int)HttpStatusCode.RequestUriTooLong;
                            return;
                        }
                    }
                    else
                    {
                        logger.Debug("No querystring.");
                    }
                    await next(context);
                };
        }
    }
}

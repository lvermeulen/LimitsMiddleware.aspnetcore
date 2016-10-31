using System;
using LimitsMiddleware.Logging;
using Microsoft.AspNetCore.Http;

namespace LimitsMiddleware
{
    using MidFunc = Func<RequestDelegate, RequestDelegate>;

    public static partial class Limits
    {
        /// <summary>
        ///     Limits the length of the request content.
        /// </summary>
        /// <param name="maxContentLength">Maximum length of the content.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxRequestContentLength(int maxContentLength, string loggerName = null)
        {
            return MaxRequestContentLength(() => maxContentLength, loggerName);
        }

        /// <summary>
        ///     Limits the length of the request content.
        /// </summary>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxContentLength</exception>
        public static MidFunc MaxRequestContentLength(Func<int> getMaxContentLength, string loggerName = null)
        {
            if (getMaxContentLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxContentLength));
            }

            return MaxRequestContentLength(_ => getMaxContentLength(), loggerName);
        }

        /// <summary>
        ///     Limits the length of the request content.
        /// </summary>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxContentLength</exception>
        public static MidFunc MaxRequestContentLength(Func<RequestContext, int> getMaxContentLength, string loggerName = null)
        {
            if (getMaxContentLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxContentLength));
            }

            loggerName = string.IsNullOrWhiteSpace(loggerName)
                ? $"{nameof(LimitsMiddleware)}.{nameof(MaxRequestContentLength)}"
                : loggerName;
            var logger = LogProvider.GetLogger(loggerName);

            return
                next =>
                async context =>
                {
                    var request = context.Request;
                    string requestMethod = request.Method.Trim().ToUpperInvariant();

                    if (requestMethod == "HEAD")
                    {
                        logger.Debug("HEAD request forwarded without check.");
                        await next(context);
                    }
                    int maxContentLength = getMaxContentLength(new RequestContext(request));
                    logger.Debug($"Max valid content length is {maxContentLength}.");
                    if (!IsChunkedRequest(request))
                    {
                        logger.Debug("Not a chunked request. Checking content length header.");
                        var contentLengthHeaderValue = request.ContentLength;
                        if (!contentLengthHeaderValue.HasValue)
                        {
                            if (requestMethod == "PUT" || requestMethod == "POST")
                            {
                                SetResponseStatusCode(context, 411);
                                return;
                            }
                            request.Body = new ContentLengthLimitingStream(request.Body, 0);
                        }
                        else
                        {
                            if (contentLengthHeaderValue > maxContentLength)
                            {
                                logger.Info($"Content length of {contentLengthHeaderValue} exceeds maximum of {maxContentLength}. Request rejected.");
                                SetResponseStatusCode(context, 413);
                                return;
                            }
                            logger.Debug("Content length header check passed.");

                            request.Body = new ContentLengthLimitingStream(request.Body, maxContentLength);
                            logger.Debug($"Request body stream configured with length limiting stream of {maxContentLength}.");
                        }
                    }
                    else
                    {
                        request.Body = new ContentLengthLimitingStream(request.Body, maxContentLength);
                        logger.Debug($"Request body stream configured with length limiting stream of {maxContentLength}.");
                        logger.Debug("Chunked request. Content length header not checked.");
                    }

                    try
                    {
                        logger.Debug("Request forwarded.");
                        await next(context);
                        logger.Debug("Processing finished.");
                    }
                    catch (ContentLengthRequiredException)
                    {
                        logger.Info("Content length required. Request canceled and rejected.");
                        SetResponseStatusCode(context, 411);
                    }
                    catch (ContentLengthExceededException)
                    {
                        logger.Info($"Content length of {maxContentLength} exceeded. Request canceled and rejected.");
                        SetResponseStatusCode(context, 413);
                    }
                };
        }

        private static bool IsChunkedRequest(HttpRequest request)
        {
            string header = request.Headers["Transfer-Encoding"];
            return header != null && header.Equals("chunked", StringComparison.OrdinalIgnoreCase);
        }

        private static void SetResponseStatusCode(HttpContext context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
        }
    }
}

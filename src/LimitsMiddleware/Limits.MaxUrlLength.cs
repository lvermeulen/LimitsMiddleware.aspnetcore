using System;
using LimitsMiddleware.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace LimitsMiddleware
{
    using MidFunc = Func<RequestDelegate, RequestDelegate>;

    public static partial class Limits
    {
        public static MidFunc MaxUrlLength(int maxUrlLength, string loggerName = null) => MaxUrlLength(() => maxUrlLength, loggerName);

        public static MidFunc MaxUrlLength(Func<int> getMaxUrlLength, string loggerName = null) => MaxUrlLength(_ => getMaxUrlLength(), loggerName);

        public static MidFunc MaxUrlLength(Func<RequestContext, int> getMaxUrlLength, string loggerName = null)
        {
            if (getMaxUrlLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxUrlLength));
            }

            loggerName = string.IsNullOrWhiteSpace(loggerName)
                ? $"{nameof(LimitsMiddleware)}.{nameof(MaxUrlLength)}"
                : loggerName;

            var logger = LogProvider.GetLogger(loggerName);

            return
                next =>
                async context =>
                {
                    int maxUrlLength = getMaxUrlLength(new RequestContext(context.Request));
                    string unescapedUri = Uri.UnescapeDataString(new Uri(context.Request.GetEncodedUrl()).AbsoluteUri);

                    logger.Debug("Checking request url length.");
                    if (unescapedUri.Length > maxUrlLength)
                    {
                        logger.Info($"Url \"{unescapedUri}\"(Length: {unescapedUri.Length}) exceeds allowed length of {maxUrlLength}. Request rejected.");
                        context.Response.StatusCode = 414;
                    }
                    logger.Debug("Check passed. Request forwarded.");
                    await next(context);
                };
        }
    }
}

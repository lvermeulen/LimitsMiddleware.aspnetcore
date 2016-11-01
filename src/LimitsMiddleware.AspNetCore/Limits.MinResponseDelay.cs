using System;
using System.Threading.Tasks;
using LimitsMiddleware.Logging;
using Microsoft.AspNetCore.Http;

namespace LimitsMiddleware
{
    using MidFunc = Func<RequestDelegate, RequestDelegate>;

    public static partial class Limits
    {
        /// <summary>
        ///     Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="minDelay">The min response delay, in milliseconds.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>A midfunc.</returns>
        public static MidFunc MinResponseDelay(int minDelay, string loggerName = null) => MinResponseDelay(_ => minDelay, loggerName);

        /// <summary>
        ///     Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="getMinDelay">A delegate to return the min response delay.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The aspnetcore builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">getMinDelay</exception>
        public static MidFunc MinResponseDelay(Func<int> getMinDelay, string loggerName = null)
        {
            if (getMinDelay == null)
            {
                throw new ArgumentNullException(nameof(getMinDelay));
            }

            return MinResponseDelay(_ => getMinDelay(), loggerName);
        }

        /// <summary>
        ///     Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="getMinDelay">A delegate to return the min response delay.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The aspnetcore builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">getMinDelay</exception>
        public static MidFunc MinResponseDelay(Func<RequestContext, int> getMinDelay, string loggerName = null)
        {
            if (getMinDelay == null)
            {
                throw new ArgumentNullException(nameof(getMinDelay));
            }

            return MinResponseDelay(context => TimeSpan.FromMilliseconds(getMinDelay(context)), loggerName);
        }

        /// <summary>
        ///     Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="minDelay">The min response delay.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>A midfunc.</returns>
        public static MidFunc MinResponseDelay(TimeSpan minDelay, string loggerName = null) => MinResponseDelay(_ => minDelay, loggerName);

        /// <summary>
        ///     Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="getMinDelay">A delegate to return the min response delay.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The aspnetcore builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">getMinDelay</exception>
        public static MidFunc MinResponseDelay(Func<TimeSpan> getMinDelay, string loggerName = null)
        {
            if (getMinDelay == null)
            {
                throw new ArgumentNullException(nameof(getMinDelay));
            }

            return MinResponseDelay(_ => getMinDelay(), loggerName);
        }

        /// <summary>
        ///     Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="getMinDelay">A delegate to return the min response delay.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The aspnetcore builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">getMinDelay</exception>
        public static MidFunc MinResponseDelay(Func<RequestContext, TimeSpan> getMinDelay, string loggerName = null)
        {
            if (getMinDelay == null)
            {
                throw new ArgumentNullException(nameof(getMinDelay));
            }

            loggerName = string.IsNullOrWhiteSpace(loggerName)
                ? $"{nameof(LimitsMiddleware)}.{nameof(MinResponseDelay)}"
                : loggerName;

            var logger = LogProvider.GetLogger(loggerName);

            return
                next =>
                async context =>
                {
                    var limitsRequestContext = new RequestContext(context.Request);
                    var delay = getMinDelay(limitsRequestContext);

                    if (delay <= TimeSpan.Zero)
                    {
                        await next(context);
                        return;
                    }

                    logger.Debug($"Delaying response by {delay}");
                    await Task.Delay(delay, context.RequestAborted);
                    await next(context);
                };
        }
    }
}

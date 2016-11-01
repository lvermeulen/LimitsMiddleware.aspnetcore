using System;
using Microsoft.AspNetCore.Builder;

namespace LimitsMiddleware.Extensions
{
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Sets a minimum delay in miliseconds before sending the response.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="minDelay">
        ///     The minimum delay to wait before sending the response.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MinResponseDelay(this IApplicationBuilder app, int minDelay, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return MinResponseDelay(app, () => minDelay, loggerName);
        }

        /// <summary>
        ///     Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMinDelay">
        ///     A delegate to retrieve the maximum number of bytes per second to be transferred.
        ///     Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MinResponseDelay(this IApplicationBuilder app, Func<int> getMinDelay, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMinDelay == null)
            {
                throw new ArgumentNullException(nameof(getMinDelay));
            }

            app.Use(Limits.MinResponseDelay(getMinDelay, loggerName));
            return app;
        }

        /// <summary>
        ///     Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMinDelay">
        ///     A delegate to retrieve the minimum delay before calling the next stage in the pipeline. Note:
        ///     the delegate should return quickly.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MinResponseDelay(this IApplicationBuilder app, Func<TimeSpan> getMinDelay, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMinDelay == null)
            {
                throw new ArgumentNullException(nameof(getMinDelay));
            }

            app.Use(Limits.MinResponseDelay(getMinDelay, loggerName));
            return app;
        }

        /// <summary>
        ///     Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMinDelay">
        ///     A delegate to retrieve the minimum delay before calling the next stage in the pipeline. Note:
        ///     the delegate should return quickly.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MinResponseDelay(this IApplicationBuilder app, Func<RequestContext, TimeSpan> getMinDelay, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMinDelay == null)
            {
                throw new ArgumentNullException(nameof(getMinDelay));
            }

            app.Use(Limits.MinResponseDelay(getMinDelay, loggerName));
            return app;
        }
    }
}

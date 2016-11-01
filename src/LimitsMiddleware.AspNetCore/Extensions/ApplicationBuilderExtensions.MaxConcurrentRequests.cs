using System;
using Microsoft.AspNetCore.Builder;

namespace LimitsMiddleware.Extensions
{
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Limits the number of concurrent requests that can be handled used by the subsequent stages in the aspnetcore pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="maxConcurrentRequests">
        ///     The maximum number of concurrent requests. Use 0 or a negative
        ///     number to specify unlimited number of concurrent requests.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxConcurrentRequests(this IApplicationBuilder app, int maxConcurrentRequests, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return MaxConcurrentRequests(app, () => maxConcurrentRequests, loggerName);
        }

        /// <summary>
        ///     Limits the number of concurrent requests that can be handled used by the subsequent stages in the aspnetcore pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxConcurrentRequests">
        ///     A delegate to retrieve the maximum number of concurrent requests. Allows you
        ///     to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent
        ///     requests.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxConcurrentRequests(this IApplicationBuilder app, Func<int> getMaxConcurrentRequests, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxConcurrentRequests == null)
            {
                throw new ArgumentNullException(nameof(getMaxConcurrentRequests));
            }

            app.Use(Limits.MaxConcurrentRequests(getMaxConcurrentRequests, loggerName));

            return app;
        }

        /// <summary>
        ///     Limits the number of concurrent requests that can be handled used by the subsequent stages in the aspnetcore pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxConcurrentRequests">
        ///     A delegate to retrieve the maximum number of concurrent requests. Allows you
        ///     to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent
        ///     requests.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxConcurrentRequests(this IApplicationBuilder app, Func<RequestContext, int> getMaxConcurrentRequests, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxConcurrentRequests == null)
            {
                throw new ArgumentNullException(nameof(getMaxConcurrentRequests));
            }

            app.Use(Limits.MaxConcurrentRequests(getMaxConcurrentRequests, loggerName));

            return app;
        }
    }
}
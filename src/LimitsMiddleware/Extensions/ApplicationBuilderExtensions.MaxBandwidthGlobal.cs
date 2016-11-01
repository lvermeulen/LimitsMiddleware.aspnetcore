using System;
using Microsoft.AspNetCore.Builder;

namespace LimitsMiddleware.Extensions
{
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Limits the bandwith used globally by the subsequent stages in the aspnetcore pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="maxBytesPerSecond">
        ///     The maximum number of bytes per second to be transferred. Use 0 or a negative
        ///     number to specify infinite bandwidth.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxBandwidthGlobal(this IApplicationBuilder app, int maxBytesPerSecond,
            string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return MaxBandwidthGlobal(app, () => maxBytesPerSecond, loggerName);
        }

        /// <summary>
        ///     Limits the bandwith used globally by the subsequent stages in the aspnetcore pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxBytesPerSecond">
        ///     A delegate to retrieve the maximum number of bytes per second to be transferred.
        ///     Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The app instance.</returns>
        public static IApplicationBuilder MaxBandwidthGlobal(this IApplicationBuilder app, Func<int> getMaxBytesPerSecond, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxBytesPerSecond == null)
            {
                throw new ArgumentNullException(nameof(getMaxBytesPerSecond));
            }

            app.Use(Limits.MaxBandwidthGlobal(getMaxBytesPerSecond, loggerName));
            return app;
        }
    }
}

using System;
using Microsoft.AspNetCore.Builder;

namespace LimitsMiddleware.Extensions
{
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="maxBytesPerSecond">
        ///     The maximum number of bytes per second to be transferred. Use 0 or a negative
        ///     number to specify infinite bandwidth.
        /// </param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxBandwidthPerRequest(this IApplicationBuilder app, int maxBytesPerSecond)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return MaxBandwidthPerRequest(app, () => maxBytesPerSecond);
        }

        /// <summary>
        ///     Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxBytesPerSecond">
        ///     A delegate to retrieve the maximum number of bytes per second to be transferred.
        ///     Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.
        /// </param>
        /// <returns>The app instance.</returns>
        public static IApplicationBuilder MaxBandwidthPerRequest(this IApplicationBuilder app, Func<int> getMaxBytesPerSecond)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxBytesPerSecond == null)
            {
                throw new ArgumentNullException(nameof(getMaxBytesPerSecond));
            }

            app.Use(Limits.MaxBandwidthPerRequest(getMaxBytesPerSecond));
            return app;
        }

        /// <summary>
        ///     Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">
        ///     A delegate to retrieve the maximum number of bytes per second to be transferred.
        ///     Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.
        /// </param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <exception cref="System.ArgumentNullException">app</exception>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static IApplicationBuilder MaxBandwidthPerRequest(this IApplicationBuilder app, Func<RequestContext, int> getMaxBytesPerSecond)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxBytesPerSecond == null)
            {
                throw new ArgumentNullException(nameof(getMaxBytesPerSecond));
            }

            app.Use(Limits.MaxBandwidthPerRequest(getMaxBytesPerSecond));
            return app;
        }
    }
}
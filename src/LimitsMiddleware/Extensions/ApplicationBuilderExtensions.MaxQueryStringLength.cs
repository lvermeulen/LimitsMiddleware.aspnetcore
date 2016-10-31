using System;
using Microsoft.AspNetCore.Builder;

namespace LimitsMiddleware.Extensions
{
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="maxQueryStringLength">Maximum length of the query string.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxQueryStringLength(this IApplicationBuilder app, int maxQueryStringLength,
            string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return MaxQueryStringLength(app, () => maxQueryStringLength, loggerName);
        }

        /// <summary>
        ///     Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxQueryStringLength(this IApplicationBuilder app, Func<int> getMaxQueryStringLength,
            string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxQueryStringLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxQueryStringLength));
            }

            app.Use(Limits.MaxQueryStringLength(getMaxQueryStringLength, loggerName));

            return app;
        }


        /// <summary>
        ///     Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxQueryStringLength(this IApplicationBuilder app,
            Func<RequestContext, int> getMaxQueryStringLength, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxQueryStringLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxQueryStringLength));
            }

            app.Use(Limits.MaxQueryStringLength(getMaxQueryStringLength, loggerName));

            return app;
        }
    }
}
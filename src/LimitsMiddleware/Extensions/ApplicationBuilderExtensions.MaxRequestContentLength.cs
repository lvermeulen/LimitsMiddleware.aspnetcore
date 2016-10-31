using System;
using Microsoft.AspNetCore.Builder;

namespace LimitsMiddleware.Extensions
{
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="maxContentLength">Maximum length of the content.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxRequestContentLength(this IApplicationBuilder app, int maxContentLength, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return MaxRequestContentLength(app, () => maxContentLength, loggerName);
        }

        /// <summary>
        ///     Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxRequestContentLength(this IApplicationBuilder app, Func<int> getMaxContentLength, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxContentLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxContentLength));
            }

            app.Use(Limits.MaxRequestContentLength(getMaxContentLength, loggerName));

            return app;
        }

        /// <summary>
        ///     Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxRequestContentLength(this IApplicationBuilder app, Func<RequestContext, int> getMaxContentLength, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxContentLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxContentLength));
            }

            app.Use(Limits.MaxRequestContentLength(getMaxContentLength, loggerName));

            return app;
        }
    }
}
using System;
using Microsoft.AspNetCore.Builder;

namespace LimitsMiddleware.Extensions
{
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="maxUrlLength">Maximum length of the URL.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxUrlLength(this IApplicationBuilder app, int maxUrlLength, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return MaxUrlLength(app, () => maxUrlLength, loggerName);
        }

        /// <summary>
        ///     Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxUrlLength(this IApplicationBuilder app, Func<int> getMaxUrlLength, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxUrlLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxUrlLength));
            }

            app.Use(Limits.MaxUrlLength(getMaxUrlLength, loggerName));
            return app;
        }

        /// <summary>
        ///     Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxUrlLength(this IApplicationBuilder app, Func<RequestContext, int> getMaxUrlLength, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getMaxUrlLength == null)
            {
                throw new ArgumentNullException(nameof(getMaxUrlLength));
            }

            app.Use(Limits.MaxUrlLength(getMaxUrlLength, loggerName));
            return app;
        }
    }
}

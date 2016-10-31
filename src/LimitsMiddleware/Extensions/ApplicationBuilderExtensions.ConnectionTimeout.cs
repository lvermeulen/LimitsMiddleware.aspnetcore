using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LimitsMiddleware.Extensions
{
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Timeouts the connection if there hasn't been an read activity on the request body stream or any
        ///     write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder ConnectionTimeout(this IApplicationBuilder app, TimeSpan timeout, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.Use(Limits.ConnectionTimeout(timeout, loggerName));

            return app;
        }

        /// <summary>
        ///     Timeouts the connection if there hasn't been an read activity on the request body stream or any
        ///     write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getTimeout">
        ///     A delegate to retrieve the timeout timespan. Allows you
        ///     to supply different values at runtime.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder ConnectionTimeout(this IApplicationBuilder app, Func<TimeSpan> getTimeout, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getTimeout == null)
            {
                throw new ArgumentNullException(nameof(getTimeout));
            }

            app.Use(Limits.ConnectionTimeout(getTimeout, loggerName));

            return app;
        }


        /// <summary>
        ///     Timeouts the connection if there hasn't been an read activity on the request body stream or any
        ///     write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getTimeout">
        ///     A delegate to retrieve the timeout timespan. Allows you
        ///     to supply different values at runtime.
        /// </param>
        /// <param name="loggerName">(Optional) The name of the logger log messages are written to.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getTimeout</exception>
        public static IApplicationBuilder ConnectionTimeout(this IApplicationBuilder app, Func<RequestContext, TimeSpan> getTimeout, string loggerName = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (getTimeout == null)
            {
                throw new ArgumentNullException(nameof(getTimeout));
            }

            app.Use(async (context, next) =>
            {
                Limits.ConnectionTimeout(getTimeout, loggerName);
                await next.Invoke();
            });
            Func<RequestDelegate, RequestDelegate> connectionTimeout = Limits.ConnectionTimeout(getTimeout, loggerName);
            app.Use(connectionTimeout);

            return app;
        }
    }
}

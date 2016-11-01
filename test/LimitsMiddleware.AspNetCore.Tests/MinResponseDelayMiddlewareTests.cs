
namespace LimitsMiddleware.Tests
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Shouldly;
    using Xunit;

    public class MinResponseDelayMiddlewareTests
    {
        [Fact]
        public async Task When_response_delay_is_applied_then_time_to_receive_data_should_be_longer()
        {
            var stopwatch = new Stopwatch();

            using (var client = CreateHttpClient(() => TimeSpan.Zero))
            {
                stopwatch.Start();

                await client.GetAsync("http://example.com");

                stopwatch.Stop();
            }

            TimeSpan noLimitTimespan = stopwatch.Elapsed;

            using (var client = CreateHttpClient(() => TimeSpan.FromMilliseconds(10)))
            {
                stopwatch.Start();

                await client.GetAsync("http://example.com");

                stopwatch.Stop();
            }

            var limitTimespan = stopwatch.Elapsed;

            limitTimespan.ShouldBeGreaterThan(noLimitTimespan);
        }

        private static HttpClient CreateHttpClient(Func<TimeSpan> getMinDelay)
        {
            var builder = new WebHostBuilder().Configure(app => app
                .MinResponseDelay(getMinDelay)
                .Use((context, _) =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.OK;

                    return Task.FromResult(0);
                }));

            var server = new TestServer(builder);
            return server.CreateClient();
        }
    }
}
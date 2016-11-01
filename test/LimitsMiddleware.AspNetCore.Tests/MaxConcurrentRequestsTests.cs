namespace LimitsMiddleware.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Shouldly;
    using Xunit;

    public class MaxConcurrentRequestsTests
    {
        [Fact]
        public async Task When_max_concurrent_request_is_1_then_second_request_should_get_service_unavailable()
        {
            var tcs = new TaskCompletionSource<int>();
            var client = CreateHttpClient(1, tcs.Task);
            var response1 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);
            var response2 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);

            tcs.SetResult(0);

            response1.StatusCode.ShouldBe(HttpStatusCode.OK);
            response2.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task When_max_concurrent_request_is_2_then_second_request_should_get_ok()
        {
            var tcs = new TaskCompletionSource<int>();
            var client = CreateHttpClient(2, tcs.Task);
            var response1 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);
            var response2 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);

            tcs.SetResult(0);

            response1.StatusCode.ShouldBe(HttpStatusCode.OK);
            response2.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_max_concurrent_request_is_0_then_second_request_should_get_ok()
        {
            var tcs = new TaskCompletionSource<int>();
            var client = CreateHttpClient(0, tcs.Task);
            var response1 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);
            var response2 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);

            tcs.SetResult(0);

            response1.StatusCode.ShouldBe(HttpStatusCode.OK);
            response2.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        private static HttpClient CreateHttpClient(int maxConcurrentRequests, Task waitHandle) => CreateHttpClient(_ => maxConcurrentRequests, waitHandle);

        private static HttpClient CreateHttpClient(Func<RequestContext, int> maxConcurrentRequests, Task waitHandle)
        {
            var builder = new WebHostBuilder().Configure(app => app
                .MaxConcurrentRequests(maxConcurrentRequests)
                .Use(async (context, _) =>
                {
                    byte[] bytes = Enumerable.Repeat((byte)0x1, 2).ToArray();
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.ContentLength = bytes.Length * 2;
                    context.Response.ContentType = "application/octet-stream";

                    // writing the response body flushes the headers
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    await waitHandle;
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                }));

            var server = new TestServer(builder);
            return server.CreateClient();
        }
    }
}
namespace LimitsMiddleware.Tests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Shouldly;
    using Xunit;

    public class MaxUrlLengthTests
    {
        [Fact]
        public async Task When_max_urlLength_is_20_and_a_url_with_length_18_should_be_served()
        {
            var client = CreateClient(20);
            var response = await client.GetAsync("http://example.com");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_max_urlLength_is_20_and_a_url_with_length_39_is_coming_it_should_be_rejected()
        {
            var client = CreateClient(20);
            var response = await client.GetAsync("http://example.com/example/example.html");

            response.StatusCode.ShouldBe(HttpStatusCode.RequestUriTooLong);
        }

        [Fact]
        public async Task When_max_urlLength_is_30_and_a_url_with_escaped_length_42_but_unescaped_28_it_should_be_served()
        {
            var client = CreateClient(30);
            var response = await client.GetAsync("http://example.com?q=%48%49%50%51%52%53%54");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        private static HttpClient CreateClient(int length)
        {
            var builder = new WebHostBuilder().Configure(app => app
                .MaxQueryStringLength(length)
                .MaxUrlLength(length)
                .Use((context, next) =>
                {
                    if (context.Response.IsSuccessStatusCode())
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    return Task.FromResult(0);
                }));

            var server = new TestServer(builder);
            return server.CreateClient();
        }
    }
}
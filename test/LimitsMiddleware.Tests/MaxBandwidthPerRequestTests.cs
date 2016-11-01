namespace LimitsMiddleware.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.FileProviders;
    using Shouldly;
    using Xunit;

    public class MaxBandwidthPerRequestTests
    {
        [Theory]
        [InlineData("file_64KB.txt", 8000, 15)]
        [InlineData("file_64KB.txt", 16000, 4)]
        [InlineData("file_512KB.txt", 100000, 5)]
        [InlineData("file_512KB.txt", 200000, 2)]
        public async Task When_bandwidth_is_applied_then_time_to_receive_data_should_be_longer(string file, int maxKiloBytesPerSeconds, int approximateSeconds)
        {
            var stopwatch = new Stopwatch();
            using (var httpClient = CreateHttpClient())
            {
                stopwatch.Start();

                var response = await httpClient.GetAsync(file);
                response.StatusCode.ShouldBe(HttpStatusCode.OK);

                stopwatch.Stop();
            }
            TimeSpan nolimitTimeSpan = stopwatch.Elapsed;

            using (var httpClient = CreateHttpClient(maxKiloBytesPerSeconds))
            {
                stopwatch.Restart();

                var response = await httpClient.GetAsync(file);
                response.StatusCode.ShouldBe(HttpStatusCode.OK);

                stopwatch.Stop();
            }
            TimeSpan limitedTimeSpan = stopwatch.Elapsed;

            Debug.WriteLine("No limits: {0}", nolimitTimeSpan);
            Debug.WriteLine("Limited  : {0}", limitedTimeSpan);

            limitedTimeSpan.ShouldBeGreaterThan(nolimitTimeSpan);

            double abs = Math.Abs((limitedTimeSpan.TotalSeconds - nolimitTimeSpan.TotalSeconds) - approximateSeconds);
            (abs < 1).ShouldBeTrue($"value {abs} >= 1");
        }

        private static HttpClient CreateHttpClient(int maxKiloBytesPerSecond = -1)
        {
            var builder = new WebHostBuilder().Configure(app => app
                .MaxBandwidthPerRequest(maxKiloBytesPerSecond)
                .UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Files")),
                    RequestPath = new PathString("")
                }));

            var server = new TestServer(builder);
            return server.CreateClient();
        }
    }
}
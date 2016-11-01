using Microsoft.AspNetCore.Http;

namespace LimitsMiddleware.Tests
{
    public static class HttpResponseExtensions
    {
        public static bool IsSuccessStatusCode(this HttpResponse response) => (response.StatusCode >= 200) && (response.StatusCode <= 299);
    }
}

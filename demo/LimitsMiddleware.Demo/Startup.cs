using System;
using System.Linq;
using LimitsMiddleware.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace LimitsMiddleware.Demo
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app
                .MaxUrlLength(100)
                .MaxQueryStringLength(80)
                .MaxConcurrentRequests(4)

                .MinResponseDelay(context =>
                {
                    var queryParams = QueryHelpers.ParseQuery(context.Uri.Query);
                    StringValues minResponseDelayParamValues;
                    string minResponseDelayParam = queryParams.TryGetValue("minresponsedelay", out minResponseDelayParamValues)
                        ? minResponseDelayParamValues.First()
                        : null;
                    int minResponseDelay;
                    return int.TryParse(minResponseDelayParam, out minResponseDelay)
                        ? TimeSpan.FromSeconds(minResponseDelay)
                        : TimeSpan.Zero;
                })

                .MaxBandwidthPerRequest(context =>
                {
                    var queryParams = QueryHelpers.ParseQuery(context.Uri.Query);
                    StringValues maxBandWidthParamValues;
                    string maxBandwidthParam = queryParams.TryGetValue("maxbandwidthperrequest", out maxBandWidthParamValues) 
                        ? maxBandWidthParamValues.First() 
                        : null;
                    int maxBandwidth;
                    return int.TryParse(maxBandwidthParam, out maxBandwidth) 
                        ? maxBandwidth 
                        : -1;
                })

                .MaxBandwidthGlobal(10 * 1024 * 1024);
        }
    }
}

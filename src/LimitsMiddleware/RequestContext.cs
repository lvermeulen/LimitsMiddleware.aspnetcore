namespace LimitsMiddleware
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;

    public class RequestContext
    {
        private readonly HttpRequest _request;

        internal RequestContext(HttpRequest request)
        {
            _request = request;
        }

        public string Method => _request.Method;

        public Uri Uri => new Uri(_request.GetEncodedUrl());

        public IHeaderDictionary Headers => _request.Headers;

        public string Host => _request.Host.ToString();

        //public string LocalIpAddress => _request.LocalIpAddress;

        //public int? LocalPort => _request.LocalPort;

        //public string RemoteIpAddress => _request.RemoteIpAddress;

        //public int? RemotePort => _request.RemotePort;

        //public ClaimsPrincipal User => _request.User;
    }
}
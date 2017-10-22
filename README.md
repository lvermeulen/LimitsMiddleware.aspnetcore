![Icon](http://i.imgur.com/D7PUb42.png?1) 
# Limits Middleware for ASP.NET Core [![Build status](https://ci.appveyor.com/api/projects/status/5oudipjv2e65bl1w?svg=true)](https://ci.appveyor.com/project/lvermeulen/limitsmiddleware-aspnetcore) [![license](https://img.shields.io/github/license/lvermeulen/LimitsMiddleware.aspnetcore.svg?maxAge=2592000)](https://github.com/lvermeulen/LimitsMiddleware.aspnetcore/blob/master/LICENSE) [![NuGet](https://img.shields.io/nuget/vpre/LimitsMiddleware.aspnetcore.svg?maxAge=2592000)](https://www.nuget.org/packages/LimitsMiddleware.aspnetcore/) [![Coverage Status](https://coveralls.io/repos/github/lvermeulen/LimitsMiddleware.aspnetcore/badge.svg?branch=master)](https://coveralls.io/github/lvermeulen/LimitsMiddleware.aspnetcore?branch=master) [![Join the chat at https://gitter.im/lvermeulen/LimitsMiddleware.aspnetcore](https://badges.gitter.im/lvermeulen/LimitsMiddleware.aspnetcore.svg)](https://gitter.im/lvermeulen/LimitsMiddleware.aspnetcore?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge) ![](https://img.shields.io/badge/.net-4.5.1-yellowgreen.svg) ![](https://img.shields.io/badge/netstandard-1.6-yellowgreen.svg)
Middleware to apply limits to an ASP.NET Core pipeline. This code was ported from [Damian Hickey's Limits Middleware for Owin](https://github.com/damianh/LimitsMiddleware).

## Features:

 - Max bandwidth
 - Max concurrent requests
 - Connection timeout
 - Max query string
 - Max request content length
 - Max url length
 - Min response delay
 

## Usage:

Configuration values can be supplied as constants or with a delegate for runtime values.

```csharp
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            // static settings
            app
	            .MaxBandwidth(10000) //bps
	            .MaxConcurrentRequests(10)
	            .ConnectionTimeout(TimeSpan.FromSeconds(10))
	            .MaxQueryStringLength(15) //Unescaped QueryString
	            .MaxRequestContentLength(15)
	            .MaxUrlLength(20)
	            .MinResponseDelay(200ms)
	            .Use(..);

        	// dynamic settings
	        app
	            .MaxBandwidth(() => 10000) //bps
	            .MaxConcurrentRequests(() => 10)
	            .ConnectionTimeout(() => TimeSpan.FromSeconds(10))
	            .MaxQueryStringLength(() => 15)
	            .MaxRequestContentLength(() => 15)
	            .MaxUrlLength(() => 20)
	            .Use(..);
		}
    }
}
```

## Thanks
* [Funnel](https://thenounproject.com/term/funnel/515072/) icon by [Arthur Shlain](https://thenounproject.com/ArtZ91/) from [The Noun Project](https://thenounproject.com)

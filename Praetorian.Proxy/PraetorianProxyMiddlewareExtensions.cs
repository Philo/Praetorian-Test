using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Praetorian.Proxy
{
    internal static class PraetorianProxyMiddlewareExtensions
    {
        internal static IApplicationBuilder UsePraetorianProxy(this IApplicationBuilder appBuilder)
        {
            return appBuilder.UseMiddleware<PraetorianProxyMiddleware>();
        }
    }
}
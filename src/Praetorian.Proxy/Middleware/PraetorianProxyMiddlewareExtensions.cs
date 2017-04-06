using Microsoft.AspNetCore.Builder;

namespace Praetorian.Proxy.Middleware
{
    internal static class PraetorianProxyMiddlewareExtensions
    {
        internal static IApplicationBuilder UsePraetorianProxy(this IApplicationBuilder appBuilder)
        {
            return appBuilder.UseMiddleware<PraetorianProxyMiddleware>();
        }
    }
}
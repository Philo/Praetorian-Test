using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Praetorian.Proxy.Middleware
{
    internal class PraetorianProxyMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public PraetorianProxyMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            logger = loggerFactory.CreateLogger<PraetorianProxyMiddleware>();
        }

        private bool IsPraetorianRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/_praetorian");
        }

        public async Task Invoke(HttpContext context, IPraetorianFileProviderFactory praetorianFileProviderFactory)
        {
            if (IsPraetorianRequest(context))
            {
                await next(context);
            }
            else
            {
                var provider = await praetorianFileProviderFactory.GetProviderAsync();
                if (provider == null)
                {
                    context.Response.Redirect("_praetorian");
                }
                else
                {
                    if (await provider.FileExistsAsync(context.Request.Path))
                    {
                        await provider.WriteToStreamAsync(context.Request.Path, context.Response);
                    }
                    else
                    {
                        await next(context);
                    }

                }
            }
        }
    }
}
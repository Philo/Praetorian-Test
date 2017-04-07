using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Praetorian.Proxy.Middleware
{
    internal class PraetorianProtectMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public PraetorianProtectMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            logger = loggerFactory.CreateLogger<PraetorianProtectMiddleware>();
        }

        private bool IsPraetorianRequest(HttpContext context)
        {
            var host = context.Items["_phost"]?.ToString();
            return
                !context.Request.Host.Host.Equals(host);
        }

        public async Task Invoke(HttpContext context, IPraetorianFileProviderFactory praetorianFileProviderFactory)
        {
            var provider = await praetorianFileProviderFactory.GetProviderAsync();
            if (provider != null)
            {
                if (await provider.FileExistsAsync(context.Request.Path))
                {
                    await provider.WriteToStreamAsync(context.Request.Path, context.Response);
                    return;
                }
            }
            await next(context);
        }
    }
}
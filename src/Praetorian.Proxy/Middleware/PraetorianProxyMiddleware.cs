using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Praetorian.Proxy.Middleware
{
    internal class PraetorianProxyMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<PraetorianOptions> options;
        private readonly ILogger logger;

        public PraetorianProxyMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<PraetorianOptions> options)
        {
            this.next = next;
            this.options = options;
            logger = loggerFactory.CreateLogger<PraetorianProxyMiddleware>();
        }

        private bool IsPraetorianRequest(HttpContext context)
        {
            var host = context.Items["_phost"]?.ToString();
            return
                !context.Request.Host.Host.Equals(host);
        }

        public async Task Invoke(HttpContext context, IPraetorianFileProviderFactory praetorianFileProviderFactory)
        {
            if (IsPraetorianRequest(context))
            {
                var provider = await praetorianFileProviderFactory.GetProviderAsync();
                if (provider == null)
                {
                    var host = context.Items["_phost"]?.ToString();
                    context.Response.Redirect($"{context.Request.Scheme}://{host}:{context.Request.Host.Port}");
                }
                else
                {
                    if (await provider.FileExistsAsync(context.Request.Path))
                    {
                        await provider.WriteToStreamAsync(context.Request.Path, context.Response);
                        return;
                    }
                }
            }
            await next(context);
        }
    }
}
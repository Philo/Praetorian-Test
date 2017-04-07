using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Praetorian.Proxy.Middleware
{
    internal class PraetorianProtectMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IOptions<PraetorianOptions> options;
        private readonly ILogger logger;

        public PraetorianProtectMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<PraetorianOptions> options)
        {
            this.next = next;
            this.options = options;
            logger = loggerFactory.CreateLogger<PraetorianProtectMiddleware>();
        }

        private bool IsPraetorianRequest(HttpContext context)
        {
            var host = options.Value.Host;
            return
                !context.Request.Host.Value.Equals(host);
        }

        public async Task Invoke(HttpContext context, IPraetorianFileProviderFactory praetorianFileProviderFactory, IOptions<PraetorianOptions> options)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                await context.Authentication.ChallengeAsync("praetorian", new AuthenticationProperties());
                return;
            }
            /*
            await context.Authentication.ChallengeAsync("praetorian", new AuthenticationProperties()
            {
                RedirectUri = 
            }); */

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
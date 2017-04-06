using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Praetorian.Proxy.StorageProviders;

namespace Praetorian.Proxy
{
    internal class PraetorianProxyMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IAzureFileProvider azureFileProvider;
        private readonly ILogger logger;

        public PraetorianProxyMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IAzureFileProvider azureFileProvider)
        {
            this.next = next;
            this.azureFileProvider = azureFileProvider;
            logger = loggerFactory.CreateLogger<PraetorianProxyMiddleware>();
        }

        private bool IsPraetorianRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/_praetorian");
        }

        public async Task Invoke(HttpContext context, IDataProtectionProvider dataProtectionProvider)
        {
            logger.LogInformation($"Requesting {context.Request.Path} | {context.Request.Host}");

            var dataProtector = dataProtectionProvider.CreateProtector("_praetorian").ToTimeLimitedDataProtector();
            if (IsPraetorianRequest(context))
            {
                // this would direct into the administration area, likely via a login
                //var ticket = dataProtector.Protect("sli-portal", TimeSpan.FromMinutes(5));
                //context.Response.Cookies.Append("__praetorianTicket", ticket, new CookieOptions
                //{
                //    Expires = DateTimeOffset.UtcNow.AddMinutes(5)
                //});
                await next(context);
            }
            else
            {
                var options = GetOptions(context, dataProtector);
                if (options == null)
                {
                    context.Response.Redirect("_praetorian");
                }
                else
                {
                    if (await azureFileProvider.FileExistsAsync(context.Request.Path))
                    {
                        await azureFileProvider.WriteToStreamAsync(context.Request.Path, context.Response);
                    }
                    else
                    {
                        await next(context);
                    }
                }
            }
        }

        private bool HasSiteCookie(IRequestCookieCollection requestCookies)
        {
            return requestCookies.ContainsKey("__praetorianTicket");
        }

        private SiteOptions GetOptions(HttpContext context, ITimeLimitedDataProtector dataProtector)
        {
            if (HasSiteCookie(context.Request.Cookies))
            {
                var encryptedTicket = string.Empty;
                context.Request.Cookies.TryGetValue("__praetorianTicket", out encryptedTicket);
                var ticket = dataProtector.Unprotect(encryptedTicket);
                return new SiteOptions()
                {
                    ContainerName = ticket
                };
            }
            return null;
        }
    }

    public class SiteOptions
    {
        public string ContainerName { get; set; }
    }
}
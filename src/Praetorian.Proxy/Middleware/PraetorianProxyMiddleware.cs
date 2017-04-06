using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Praetorian.Proxy.Controllers;
using Praetorian.Proxy.StorageProviders;

namespace Praetorian.Proxy.Middleware
{
    public interface IPraetorianFileProviderFactory
    {
        Task<IPraetorianFileProvider> GetProviderAsync();
    }

    public class PraetorianFileProviderFactory : IPraetorianFileProviderFactory
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IPraetorianProjectService praetorianProjectService;

        public PraetorianFileProviderFactory(IHttpContextAccessor httpContextAccessor, IPraetorianProjectService praetorianProjectService)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.praetorianProjectService = praetorianProjectService;
        }

        public async Task<IPraetorianFileProvider> GetProviderAsync()
        {
            var request = httpContextAccessor.HttpContext.Request;
            var token = request.Cookies.GetPraetorianSiteCookieToken();
            var project = await praetorianProjectService.GetProjectFromSiteReferenceToken(token);
            if (project == null || !project.Active)
            {
                return null;
            }

            return new AzurePraetorianFileProvider(new Uri(project.SasUri), project.ContainerName, project.DefaultDocument);
        }
    }

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
            logger.LogInformation($"Requesting {context.Request.Path} | {context.Request.Host}");

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

        //private bool HasSiteCookie(IRequestCookieCollection requestCookies)
        //{
        //    return requestCookies.ContainsKey("__praetorianTicket");
        //}

        //private SiteOptions GetOptions(HttpContext context, ITimeLimitedDataProtector dataProtector)
        //{
        //    if (HasSiteCookie(context.Request.Cookies))
        //    {
        //        var encryptedTicket = string.Empty;
        //        context.Request.Cookies.TryGetValue("__praetorianTicket", out encryptedTicket);
        //        var ticket = dataProtector.Unprotect(encryptedTicket);
        //        return new SiteOptions()
        //        {
        //            ContainerName = ticket
        //        };
        //    }
        //    return null;
        //}
    }

    public class SiteOptions
    {
        public string ContainerName { get; set; }
    }
}
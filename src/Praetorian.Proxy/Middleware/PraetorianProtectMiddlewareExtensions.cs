using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Praetorian.Proxy.Services;

namespace Praetorian.Proxy.Middleware
{
    internal static class PraetorianProtectMiddlewareExtensions
    {
        private static readonly Regex PraetorianProtectPattern = new Regex(@"^(?<project>[^.]+)[.](?<client>[^.]+)[.](?<host>[^:]+)[:]?(?<port>\d+)?$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        internal static IApplicationBuilder UsePraetorianProtect(this IApplicationBuilder appBuilder, Action<PraetorianOptionsConfigurer> options = null)
        {
            var pOptions = appBuilder.ApplicationServices.GetRequiredService<IOptions<PraetorianOptions>>().Value;
            var configurer = new PraetorianOptionsConfigurer(appBuilder.ApplicationServices, pOptions);
            options?.Invoke(configurer);

            var projectService = appBuilder.ApplicationServices.GetRequiredService<IPraetorianProjectService>();

            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = async context =>
                    {
                        var clientName = context.HttpContext.Items["_pclient"].ToString();
                        var projectName = context.HttpContext.Items["_pproject"].ToString();

                        var project = await projectService.GetProject(clientName, projectName);
                        var returnUrl = project?.BuildProjectUri(context.HttpContext) ?? "/";
                        context.HttpContext.Response.Redirect($"{context.Request.Scheme}://{pOptions.Host.TrimEnd('/')}{context.Options.LoginPath}?returnUrl={WebUtility.UrlEncode(returnUrl)}");
                    }
                },
                LoginPath = new PathString("/Login"),
                AccessDeniedPath = new PathString("/Login/Denied"),
                CookieName = ".praetorian",
                CookieDomain = pOptions.CookieDomain,
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = "praetorian"
            });

            return appBuilder.UseWhen(c => IsPraetorianProtected(c, pOptions), app => app.UseMiddleware<PraetorianProtectMiddleware>());
        }

        private static bool IsPraetorianProtected(HttpContext context, PraetorianOptions pOptions)
        {
            if (context.Request.Host.Value.Equals(pOptions.Host, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            var result = PraetorianProtectPattern.Match(context.Request.Host.Value);
            if (result.Success)
            {
                //context.Items.Add("_phost", pOptions.Host);
                context.Items.Add("_pclient", result.Groups["client"]);
                context.Items.Add("_pproject", result.Groups["project"]);
            }
            return result.Success;
        }
    }
}
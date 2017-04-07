using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Praetorian.Proxy.Middleware
{
    internal class PraetorianOptions
    {
        public string Host { get; private set; }

        public PraetorianOptions WithHost(string host)
        {
            Host = host;
            return this;
        }
    }

    internal static class PraetorianProxyMiddlewareExtensions
    {
        private static readonly Regex PraetorianProtectPattern = new Regex(@"^(?<project>[^.]+)[.](?<client>[^.]+)[.](?<host>[^:]+)[:]?(?<port>\d+)?$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        internal static IApplicationBuilder UsePraetorianProxy(this IApplicationBuilder appBuilder, Action<PraetorianOptions> options = null)
        {
            var pOptions = appBuilder.ApplicationServices.GetRequiredService<IOptions<PraetorianOptions>>()?.Value;
            options?.Invoke(pOptions);
            return appBuilder.UseWhen(c => IsPraetorianProtected(c, pOptions), app => app.UseMiddleware<PraetorianProxyMiddleware>());
        }

        private static bool IsPraetorianProtected(HttpContext context, PraetorianOptions pOptions)
        {
            if (context.Request.Host.Host.Equals(pOptions.Host, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            var result = PraetorianProtectPattern.Match(context.Request.Host.Value);
            if (result.Success)
            {
                context.Items.Add("_phost", pOptions.Host);
                context.Items.Add("_pclient", result.Groups["client"]);
                context.Items.Add("_pproject", result.Groups["project"]);
            }
            return result.Success;
        }
    }
}
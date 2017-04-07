using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Praetorian.Proxy.Middleware
{
    internal class PraetorianOptionsConfigurer
    {
        private readonly IServiceProvider serviceProvider;
        private readonly PraetorianOptions options;

        public PraetorianOptionsConfigurer(IServiceProvider serviceProvider, PraetorianOptions options)
        {
            this.serviceProvider = serviceProvider;
            this.options = options;
        }

        public PraetorianOptionsConfigurer WithHost(string host)
        {
            options.Host = host;
            return this;
        }

        public PraetorianOptionsConfigurer WithHost<TOptions>(Func<TOptions, string> hostOptionFunc) where TOptions : class, new()
        {
            var hostOption = serviceProvider.GetRequiredService<IOptions<TOptions>>()?.Value;
            var host = hostOptionFunc(hostOption);
            return WithHost(host);
        }

        public PraetorianOptionsConfigurer WithAzureTableConnectionString(string connectionString)
        {
            options.AzureTableConnectionString = connectionString;
            return this;
        }
    }

    internal class PraetorianOptions
    {
        public string AzureTableConnectionString { get; set; }
        public string Host { get; set; }
    }

    internal static class PraetorianProtectMiddlewareExtensions
    {
        private static readonly Regex PraetorianProtectPattern = new Regex(@"^(?<project>[^.]+)[.](?<client>[^.]+)[.](?<host>[^:]+)[:]?(?<port>\d+)?$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        internal static IApplicationBuilder UsePraetorianProtect(this IApplicationBuilder appBuilder, Action<PraetorianOptionsConfigurer> options = null)
        {
            var pOptions = appBuilder.ApplicationServices.GetRequiredService<IOptions<PraetorianOptions>>()?.Value;
            var configurer = new PraetorianOptionsConfigurer(appBuilder.ApplicationServices, pOptions);
            options?.Invoke(configurer);
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
                context.Items.Add("_phost", pOptions.Host);
                context.Items.Add("_pclient", result.Groups["client"]);
                context.Items.Add("_pproject", result.Groups["project"]);
            }
            return result.Success;
        }
    }
}
using System;
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

        public PraetorianOptionsConfigurer WithCookieDomain(string domain)
        {
            options.CookieDomain = domain;
            return this;
        }

        public PraetorianOptionsConfigurer WithHost(string host)
        {
            options.Host = host;
            return this;
        }

        public PraetorianOptionsConfigurer WithCookieDomain<TOptions>(Func<TOptions, string> optionFunc) where TOptions : class, new()
        {
            var hostOption = serviceProvider.GetRequiredService<IOptions<TOptions>>()?.Value;
            var host = optionFunc(hostOption);
            return WithCookieDomain(host);
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
}
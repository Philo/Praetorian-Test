using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Praetorian.Proxy.Controllers;
using Praetorian.Proxy.Middleware;
using Praetorian.Proxy.Services;
using Praetorian.Proxy.StorageProviders;

namespace Praetorian.Proxy
{
    public class SiteSettings
    {
        public string Host { get; set; }
    }

    internal static class PraetorianServiceCollectionExtensions
    {
        internal static IServiceCollection AddPraetorianProtect(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            serviceCollection.AddScoped<IPraetorianFileProvider, AzurePraetorianFileProvider>();
            serviceCollection.AddScoped<IPraetorianProjectService, PraetorianProjectService>();
            serviceCollection.AddScoped<IPraetorianFileProviderFactory, PraetorianFileProviderFactory>();
            serviceCollection.Configure<PraetorianOptions>(options =>
            {
            });

            return serviceCollection;
        }
    }

    public class Startup
    {
        public Startup(IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Startup>();

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<SiteSettings>(Configuration.GetSection(nameof(SiteSettings)));

            services.AddDataProtection();

            services.AddMvc();

            services.AddTransient(p => Configuration);
            services.AddPraetorianProtect();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IDataProtectionProvider dataProtectionProvider)
        {
            loggerFactory.AddConsole();
            app.UsePraetorianProtect(o => o
                .WithHost<SiteSettings>(s => s.Host)
                .WithAzureTableConnectionString("")
            );

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}

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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPraetorianFileProvider, AzurePraetorianFileProvider>();
            services.AddScoped<IPraetorianProjectService, PraetorianProjectService>();
            services.AddScoped<IPraetorianFileProviderFactory, PraetorianFileProviderFactory>();
            services.Configure<PraetorianOptions>(Configuration.GetSection(nameof(PraetorianOptions)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IDataProtectionProvider dataProtectionProvider)
        {
            loggerFactory.AddConsole();
            app.UsePraetorianProxy(o => o.WithHost("praetorianprotect.localtest.me"));
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}

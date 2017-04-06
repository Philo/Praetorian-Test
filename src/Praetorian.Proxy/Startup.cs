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
using Praetorian.Proxy.StorageProviders;

namespace Praetorian.Proxy
{
    public class Startup
    {
        public Startup(IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddDataProtection();

            services.AddMvc();

            services.AddTransient(p => Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IPraetorianFileProvider, AzurePraetorianFileProvider>();
            services.AddScoped<IPraetorianProjectService, PraetorianProjectService>();
            services.AddScoped<IPraetorianFileProviderFactory, PraetorianFileProviderFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IDataProtectionProvider dataProtectionProvider)
        {
            loggerFactory.AddConsole();
            app.UsePraetorianProxy();
            app.UseMvc(o =>
            {
                o.MapRoute("default", "_praetorian/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

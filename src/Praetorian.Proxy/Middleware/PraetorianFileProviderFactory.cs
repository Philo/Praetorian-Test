using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Praetorian.Proxy.Controllers;
using Praetorian.Proxy.Extensions;
using Praetorian.Proxy.Services;
using Praetorian.Proxy.StorageProviders;

namespace Praetorian.Proxy.Middleware
{
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
            //var request = httpContextAccessor.HttpContext.Request;
            
            //var token = request.Cookies.GetPraetorianSiteCookieToken();
            //var project = await praetorianProjectService.GetProjectFromSiteReferenceToken(token);

            var clientName = httpContextAccessor.HttpContext.Items["_pclient"]?.ToString();
            var projectName = httpContextAccessor.HttpContext.Items["_pproject"]?.ToString();

            if (string.IsNullOrWhiteSpace(clientName) || string.IsNullOrWhiteSpace(projectName))
            {
                return null;
            }

            var project = await praetorianProjectService.GetProject(clientName, projectName);

            if (project == null || !project.Active)
            {
                return null;
            }

            return new AzurePraetorianFileProvider(new Uri(project.SasUri), project.ContainerName, project.DefaultDocument);
        }
    }
}
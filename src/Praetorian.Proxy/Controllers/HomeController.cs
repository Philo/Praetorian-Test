using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Praetorian.Proxy.Controllers
{
    public class PraetorianProject : TableEntity
    {
        public string Client => PartitionKey;
        public string Project => RowKey;
        public string SasUri { get; set; }
        public string ContainerName { get; set; }
        public string DefaultDocument { get; set; }
        public bool Active { get; set; }
    }
    
    public interface IPraetorianProjectService
    {
        Task<PraetorianProject> GetProject(string clientName, string projectName);
        string GenerateSiteReferenceToken(PraetorianProject project);

        Task<PraetorianProject> GetProjectFromSiteReferenceToken(string token);
    }

    public class PraetorianProjectService : IPraetorianProjectService
    {
        private readonly IConfiguration configuration;
        private readonly IDataProtector dataProtector;

        public PraetorianProjectService(IConfiguration configuration, IDataProtectionProvider dataProtectionProvider)
        {
            this.configuration = configuration;
            this.dataProtector = dataProtectionProvider.CreateProtector(nameof(Praetorian));
        }

        public string GenerateSiteReferenceToken(PraetorianProject project)
        {
            return dataProtector.Protect($"{project.Client}|{project.Project}");
        }

        public async Task<PraetorianProject> GetProjectFromSiteReferenceToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            try
            {
                var source = dataProtector.Unprotect(token);
                if (string.IsNullOrWhiteSpace(source)) return null;
                var sourceElements = source.Split('|');
                var clientName = sourceElements.ElementAtOrDefault(0);
                var projectName = sourceElements.ElementAtOrDefault(1);

                return await GetProject(clientName, projectName);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<PraetorianProject> GetProject(string clientName, string projectName)
        {
            var connectionString = configuration.GetConnectionString("default");

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference("projects");

            var query = new TableQuery<PraetorianProject>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(nameof(PraetorianProject.PartitionKey), QueryComparisons.Equal, clientName),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition(nameof(PraetorianProject.RowKey), QueryComparisons.Equal, projectName)
                    )
                )
                .Take(1);

            var result = await table.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());
            return result.Results.FirstOrDefault();
        }
    }

    public class HomeController : Controller
    {
        private readonly IPraetorianProjectService praetorianProjectService;
        public HomeController(IPraetorianProjectService praetorianProjectService)
        {
            this.praetorianProjectService = praetorianProjectService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("_praetorian/{clientName}/{projectName}")]
        public async Task<IActionResult> Index(string clientName, string projectName)
        {
            var project = await praetorianProjectService.GetProject(clientName, projectName);
            if (project == null)
            {
                return NotFound();
            }

            var token = praetorianProjectService.GenerateSiteReferenceToken(project);
            HttpContext.Response.Cookies.AddPraetorianSiteCookie(token);

            return Redirect("/");
        }
    }

    internal static class HttpCookiesExtensions
    {
        internal static void AddPraetorianSiteCookie(this IResponseCookies cookies, string token)
        {
            cookies.Append("__praetorian_siteref", token);
        }

        internal static string GetPraetorianSiteCookieToken(this IRequestCookieCollection cookies)
        {
            if (cookies.ContainsKey("__praetorian_siteref"))
            {
                return cookies["__praetorian_siteref"];
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Praetorian.Proxy.Domain;
using Praetorian.Proxy.Middleware;

namespace Praetorian.Proxy.Services
{
    public class PraetorianProjectService : IPraetorianProjectService
    {
        private readonly IOptions<PraetorianOptions> options;
        private readonly IDataProtector dataProtector;

        public PraetorianProjectService(IOptions<PraetorianOptions> options, IDataProtectionProvider dataProtectionProvider)
        {
            this.options = options;
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
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<PraetorianProject>> GetAllProjects()
        {
            var connectionString = options.Value.AzureTableConnectionString;

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference("projects");

            var query = new TableQuery<PraetorianProject>();

            var result = await table.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());
            return result.Results.ToList();
        }

        public async Task<PraetorianProject> GetProject(string clientName, string projectName)
        {
            var connectionString = options.Value.AzureTableConnectionString;

            var account = CloudStorageAccount.Parse(connectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference("projects");

            var query = new TableQuery<PraetorianProject>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(nameof(TableEntity.PartitionKey), QueryComparisons.Equal, clientName),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition(nameof(TableEntity.RowKey), QueryComparisons.Equal, projectName)
                    )
                )
                .Take(1);

            var result = await table.ExecuteQuerySegmentedAsync(query, new TableContinuationToken());
            return result.Results.FirstOrDefault();
        }
    }
}
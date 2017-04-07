using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Table;

namespace Praetorian.Proxy.Domain
{
    public class PraetorianProject : TableEntity
    {
        public string Client => PartitionKey;
        public string Project => RowKey;
        public string SasUri { get; set; }
        public string ContainerName { get; set; }
        public string DefaultDocument { get; set; }
        public bool Active { get; set; }

        public string BuildProjectUri(HttpContext context)
        {
            return $"{context.Request.Scheme}://{Project}.{Client}.{context.Request.Host.Value}";
        }
    }
}
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
}
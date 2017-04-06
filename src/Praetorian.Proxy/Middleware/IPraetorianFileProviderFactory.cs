using System.Threading.Tasks;
using Praetorian.Proxy.StorageProviders;

namespace Praetorian.Proxy.Middleware
{
    public interface IPraetorianFileProviderFactory
    {
        Task<IPraetorianFileProvider> GetProviderAsync();
    }
}
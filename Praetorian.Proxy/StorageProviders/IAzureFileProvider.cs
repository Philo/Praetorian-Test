using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Praetorian.Proxy.StorageProviders
{
    public interface IAzureFileProvider
    {
        Task<bool> FileExistsAsync(string subpath);
        Task WriteToStreamAsync(string subpath, HttpResponse response);
    }
}
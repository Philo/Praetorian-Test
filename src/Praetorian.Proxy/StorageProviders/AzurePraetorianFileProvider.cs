using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Praetorian.Proxy.StorageProviders
{
    public class AzurePraetorianFileProvider : IPraetorianFileProvider
    {
        private readonly string defaultDocument;
        private readonly CloudBlobContainer blobContainer;

        public AzurePraetorianFileProvider(Uri sasUri, string containerName, string defaultDocument = "index.html")
        {
            this.defaultDocument = defaultDocument;
            var blobClient = new CloudBlobClient(sasUri);
            blobContainer = blobClient.GetContainerReference(containerName);
        }

        public string GetBlobReferencePath(string subpath)
        {
            var blobReferencePath = subpath;
            if (blobReferencePath.Equals("/"))
            {
                blobReferencePath = GetDefaultDocument();
            }

            return blobReferencePath?.Trim('/');
        }

        public async Task<bool> FileExistsAsync(string subpath)
        {
            var blobReferencePath = GetBlobReferencePath(subpath);

            if (!string.IsNullOrWhiteSpace(blobReferencePath))
            {
                var reference = blobContainer.GetBlobReference(blobReferencePath);
                return await reference.ExistsAsync();
            }
            return false;
        }

        public async Task WriteToStreamAsync(string subpath, HttpResponse response)
        {
            var blobReferencePath = GetBlobReferencePath(subpath);

            if (!string.IsNullOrWhiteSpace(blobReferencePath))
            {
                var reference = blobContainer.GetBlobReference(blobReferencePath);
                if (await reference.ExistsAsync())
                {
                    await reference.FetchAttributesAsync();
                    response.ContentType = reference.Properties.ContentType;

                    response.Headers.Add("Content-Encoding", reference.Properties.ContentEncoding);
                    response.Headers.Add("Cache-Control", reference.Properties.CacheControl);
                    response.Headers.Add("Content-Disposition", reference.Properties.ContentDisposition);
                    response.Headers.Add("Content-Language", reference.Properties.ContentLanguage);
                    response.Headers.Add("Content-Md5", reference.Properties.ContentMD5);
                    response.Headers.Add("ETag", reference.Properties.ETag);
                    response.Headers.Add("Last-Modified", reference.Properties.LastModified.GetValueOrDefault().ToString("O"));
                    response.Headers.Add("Content-Length", reference.Properties.Length.ToString());

                    await reference.DownloadToStreamAsync(response.Body).ConfigureAwait(false);
                }
            }
        }

        private string GetDefaultDocument()
        {
            return defaultDocument;
        }
    }
}